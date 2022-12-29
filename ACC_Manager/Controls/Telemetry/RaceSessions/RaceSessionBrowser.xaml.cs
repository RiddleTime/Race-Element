using RaceElement.Broadcast;
using RaceElement.Controls.Telemetry.RaceSessions;
using RaceElement.Controls.Telemetry.RaceSessions.Plots;
using RaceElement.Data;
using RaceElement.Data.ACC.Cars;
using RaceElement.Data.ACC.Database;
using RaceElement.Data.ACC.Database.GameData;
using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Database.Telemetry;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracks;
using RaceElement.Util;
using LiteDB;
using MaterialDesignThemes.Wpf;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DataGridTextColumn = MaterialDesignThemes.Wpf.DataGridTextColumn;
using TrackData = RaceElement.Data.ACC.Tracks.TrackNames.TrackData;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for RaceSessionBrowser.xaml
    /// - TODO: refactor into OOP
    /// </summary>
    public partial class RaceSessionBrowser : UserControl
    {
        public static RaceSessionBrowser Instance { get; private set; }
        private LiteDatabase CurrentDatabase;

        private int previousTelemetryComboSelection = -1;

        public RaceSessionBrowser()
        {
            InitializeComponent();

            this.Loaded += (s, e) => ThreadPool.QueueUserWorkItem(x =>
            {
                FindRaceWeekends();
            });

            comboTracks.SelectionChanged += (s, e) => FillCarComboBox();
            comboCars.SelectionChanged += (s, e) => LoadSessionList();
            listViewRaceSessions.SelectionChanged += (s, e) => LoadSession();

            gridTabHeaderLocalSession.MouseRightButtonUp += (s, e) => FindRaceWeekends();

            RaceSessionTracker.Instance.OnRaceWeekendEnded += (s, e) => FindRaceWeekends();

            Instance = this;
        }

        private void CloseTelemetry()
        {
            comboBoxMetrics.Items.Clear();
            gridMetrics.Children.Clear();
            textBlockMetricInfo.Text = String.Empty;
            transitionContentPlots.Visibility = Visibility.Collapsed;
            trackMap.Visibility = Visibility.Collapsed;

            Grid.SetRowSpan(gridSessionViewer, 2);
            Grid.SetRowSpan(tabControlWeekends, 2);

            ThreadPool.QueueUserWorkItem(x =>
            {
                Thread.Sleep(2000);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            });
        }

        private void FindRaceWeekends()
        {
            Dispatcher.Invoke(() =>
            {
                localRaceWeekends.Items.Clear();

                DirectoryInfo dataDir = new DirectoryInfo(FileUtil.RaceElementDataPath);
                if (!dataDir.Exists)
                    return;

                var raceWeekendFiles = dataDir.EnumerateFiles()
                    .Where(x => !x.Name.Contains("log") && x.Extension == ".rwdb")
                    .OrderByDescending(x => x.LastWriteTimeUtc);

                foreach (FileInfo file in raceWeekendFiles)
                {
                    TextBlock textBlock = new TextBlock() { Text = file.Name.Replace(file.Extension, ""), FontSize = 12 };
                    ListViewItem lvi = new ListViewItem() { Content = textBlock, DataContext = file.FullName, Cursor = Cursors.Hand };
                    lvi.MouseLeftButtonUp += (s, e) =>
                    {
                        ListViewItem item = (ListViewItem)s;
                        OpenRaceWeekendDatabase((string)item.DataContext);
                    };
                    localRaceWeekends.Items.Add(lvi);
                }
            });
        }

        public void OpenRaceWeekendDatabase(string filename, bool focusCurrentWeekendTab = true)
        {
            if (CurrentDatabase != null)
                CurrentDatabase.Dispose(); ;

            CurrentDatabase = RaceWeekendDatabase.OpenDatabase(filename);
            if (CurrentDatabase != null)
            {
                FillTrackComboBox();
                if (focusCurrentWeekendTab)
                    tabCurrentWeekend.Focus();
            }
        }

        private void LoadSession()
        {
            DbRaceSession session = GetSelectedRaceSession();
            if (session == null) return;

            Dictionary<int, DbLapData> laps = LapDataCollection.GetForSession(CurrentDatabase, session.Id);
            stackerSessionViewer.Children.Clear();
            gridSessionLaps.Children.Clear();

            if (session == null) return;

            string sessionInfo = $"{(session.IsOnline ? "On" : "Off")}line {ACCSharedMemory.SessionTypeToString(session.SessionType)}";

            if (session.UtcEnd > session.UtcStart)
            {
                TimeSpan duration = session.UtcEnd.Subtract(session.UtcStart);
                sessionInfo += $" - Duration: {duration:hh\\:mm\\:ss}";
            }

            int potentialBestLapTime = laps.GetPotentialFastestLapTime();
            if (potentialBestLapTime != -1)
                sessionInfo += $" - Potential best: {new TimeSpan(0, 0, 0, 0, potentialBestLapTime):mm\\:ss\\:fff}";

            stackerSessionViewer.Children.Add(new TextBlock()
            {
                Text = sessionInfo,
                FontSize = 14
            });

            gridSessionLaps.Children.Add(GetLapDataGrid(laps));

            Grid.SetRowSpan(gridSessionViewer, 2);
            Grid.SetRowSpan(tabControlWeekends, 2);

            transitionContentPlots.Visibility = Visibility.Collapsed;
            trackMap.Visibility = Visibility.Collapsed;

            PlotUtil.AxisLimitsCustom = false;

        }

        private Guid GetSelectedTrack()
        {
            if (comboTracks.SelectedIndex == -1) return Guid.Empty;
            return (Guid)(comboTracks.SelectedItem as ComboBoxItem).DataContext;
        }

        private Guid GetSelectedCar()
        {
            if (comboCars.SelectedIndex == -1) return Guid.Empty;
            return (Guid)(comboCars.SelectedItem as ComboBoxItem).DataContext;
        }

        private DbRaceSession GetSelectedRaceSession()
        {
            if (listViewRaceSessions.SelectedIndex == -1) return null;
            return (DbRaceSession)(listViewRaceSessions.SelectedItem as ListViewItem).DataContext;
        }

        public void FillCarComboBox()
        {
            if (GetSelectedTrack() == Guid.Empty)
                return;

            List<Guid> carGuidsForTrack = RaceSessionCollection.GetAllCarsForTrack(CurrentDatabase, GetSelectedTrack());
            List<DbCarData> allCars = CarDataCollection.GetAll(CurrentDatabase);

            comboCars.Items.Clear();
            foreach (DbCarData carData in allCars.Where(x => carGuidsForTrack.Contains(x.Id)))
            {
                var carModel = ConversionFactory.ParseCarName(carData.ParseName);
                string carName = ConversionFactory.GetNameFromCarModel(carModel);
                ComboBoxItem item = new ComboBoxItem() { DataContext = carData.Id, Content = carName };
                comboCars.Items.Add(item);
            }
            comboCars.SelectedIndex = 0;
        }

        public void FillTrackComboBox()
        {
            comboTracks.Items.Clear();
            List<DbTrackData> allTracks = TrackDataCollection.GetAll(CurrentDatabase);
            if (allTracks.Any())
            {
                foreach (DbTrackData track in allTracks)
                {
                    string trackName;
                    TrackNames.Tracks.TryGetValue(track.ParseName, out TrackData trackData);
                    if (trackData == null) trackName = track.ParseName;
                    else trackName = trackData.FullName;

                    ComboBoxItem item = new ComboBoxItem() { DataContext = track.Id, Content = trackName };
                    comboTracks.Items.Add(item);
                }

                comboTracks.SelectedIndex = 0;
            }
        }

        public void LoadSessionList()
        {
            List<DbRaceSession> allsessions = RaceSessionCollection.GetAll(CurrentDatabase);

            listViewRaceSessions.Items.Clear();
            var sessionsWithCorrectTrackAndCar = allsessions
                .Where(x => x.TrackId == GetSelectedTrack() && x.CarId == GetSelectedCar())
                .OrderByDescending(x => x.UtcStart);
            if (sessionsWithCorrectTrackAndCar.Any())
            {
                foreach (DbRaceSession session in sessionsWithCorrectTrackAndCar)
                {
                    DbCarData carData = CarDataCollection.GetCarData(CurrentDatabase, session.CarId);
                    DbTrackData dbTrackData = TrackDataCollection.GetTrackData(CurrentDatabase, session.TrackId);

                    var carModel = ConversionFactory.ParseCarName(carData.ParseName);
                    string carName = ConversionFactory.GetNameFromCarModel(carModel);
                    string trackName = dbTrackData.ParseName;
                    TrackNames.Tracks.TryGetValue(dbTrackData.ParseName, out TrackData trackData);
                    if (dbTrackData != null) trackName = trackData.FullName;

                    session.UtcStart = DateTime.SpecifyKind(session.UtcStart, DateTimeKind.Utc);
                    ListViewItem listItem = new ListViewItem()
                    {
                        Content = $"{ACCSharedMemory.SessionTypeToString(session.SessionType)} - {session.UtcStart.ToLocalTime():U}",
                        DataContext = session
                    };
                    listViewRaceSessions.Items.Add(listItem);
                }

                listViewRaceSessions.SelectedIndex = 0;
            }
        }

        public DataGrid GetLapDataGrid(Dictionary<int, DbLapData> laps)
        {
            var data = laps.OrderByDescending(x => x.Key).Select(x => x.Value);
            DataGrid grid = new DataGrid()
            {
                ItemsSource = data,
                AutoGenerateColumns = false,
                CanUserDeleteRows = false,
                CanUserAddRows = false,
                IsReadOnly = true,
                EnableRowVirtualization = false,
                SelectionMode = DataGridSelectionMode.Single,
                SelectionUnit = DataGridSelectionUnit.FullRow,
                GridLinesVisibility = DataGridGridLinesVisibility.Vertical,
                FontWeight = FontWeights.ExtraBold,
                AlternatingRowBackground = new SolidColorBrush(Color.FromArgb(25, 0, 0, 0)),
                RowBackground = Brushes.Transparent,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            };

            int fastestLapIndex = laps.GetFastestLapIndex();
            grid.LoadingRow += (s, e) =>
            {
                DataGridRowEventArgs ev = e;
                DbLapData lapData = (DbLapData)ev.Row.DataContext;

                ev.Row.Margin = new Thickness(0);
                ev.Row.Padding = new Thickness(0);

                if (!lapData.IsValid)
                    ev.Row.Foreground = Brushes.OrangeRed;

                if (lapData.Index == fastestLapIndex)
                    ev.Row.Foreground = Brushes.LimeGreen;

                switch (lapData.LapType)
                {
                    case LapType.Outlap:
                        {
                            ev.Row.FontStyle = FontStyles.Italic;
                            break;
                        }
                    case LapType.Inlap:
                        {
                            ev.Row.FontStyle = FontStyles.Italic;
                            break;
                        }
                }

                ev.Row.PreviewMouseLeftButtonDown += (se, eve) =>
                {
                    if (ev.Row.IsSelected)
                    {
                        CloseTelemetry();
                        ev.Row.IsSelected = false;
                        eve.Handled = true;
                    }
                };
            };

            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = new TextBlock() { Text = "#", ToolTip = "Lap", Margin = new Thickness(0) },
                Binding = new Binding("Index"),
                SortDirection = System.ComponentModel.ListSortDirection.Descending,
                FontWeight = FontWeights.DemiBold,
            });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Time",
                Binding = new Binding("Time") { Converter = new MillisecondsToFormattedTimeSpanString() }
            });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "S 1",
                Binding = new Binding("Sector1") { Converter = new DivideBy1000ToFloatConverter() }
            });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "S 2",
                Binding = new Binding("Sector2") { Converter = new DivideBy1000ToFloatConverter() }
            });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "S 3",
                Binding = new Binding("Sector3") { Converter = new DivideBy1000ToFloatConverter() }
            });

            // fuel used
            StackPanel fuelUsagePanel = new StackPanel()
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                ToolTip = "Fuel used",
            };
            fuelUsagePanel.Children.Add(new PackIcon() { Kind = PackIconKind.Fuel });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = fuelUsagePanel,
                Binding = new Binding("FuelUsage") { Converter = new DivideBy1000ToFloatConverter() },
            });

            // fuel left
            StackPanel fuelLeftPanel = new StackPanel()
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                ToolTip = "Fuel left"
            };
            fuelLeftPanel.Children.Add(new PackIcon() { Kind = PackIconKind.Fuel });
            fuelLeftPanel.Children.Add(new TextBlock() { Text = " Left" });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = fuelLeftPanel,
                Binding = new Binding("FuelInTank") { Converter = new FormattedFloatConverter() }
            });

            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Box",
                Binding = new Binding("LapType") { Converter = new LapTypeConverter() }
            });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Track",
                Binding = new Binding("GripStatus") { }
            });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "°C Air",
                Binding = new Binding("TempAmbient") { Converter = new FormattedFloatConverter(2) }
            });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "°C Track",
                Binding = new Binding("TempTrack") { Converter = new FormattedFloatConverter(2) }
            });



            //grid.SelectedCellsChanged += (s, e) =>
            //{
            //    if (grid.SelectedIndex != -1)
            //    {
            //        DbLapData lapdata = (DbLapData)grid.SelectedItem;

            //        CreateCharts(lapdata.Id);
            //    }
            //};

            return grid;
        }


        private delegate WpfPlot Plotter(Grid g, Dictionary<long, TelemetryPoint> dictio);

        private SelectionChangedEventHandler _selectionChangedHandler;
        private Dictionary<long, TelemetryPoint> _currentData;

        private void LogTelemetrySplinesInfo(Dictionary<long, TelemetryPoint> dictio)
        {
            Debug.WriteLine($"Spline[Start: {dictio.First().Value.SplinePosition:F6}, End: {dictio.Last().Value.SplinePosition:F6}, Count: {dictio.Count}]");

            float previousSpline = -1;
            int index = 0;
            foreach (var node in dictio)
            {
                if (node.Value.SplinePosition < previousSpline)
                {
                    Debug.WriteLine($"Decreasing spline at index {index + 1}: {previousSpline}->{node.Value.SplinePosition:F12}");

                    int tenPercentCount = dictio.Count / 10;

                    break;
                }
                previousSpline = node.Value.SplinePosition;
                index++;
            }


        }

        private void FilterTelemetrySplines(Dictionary<long, TelemetryPoint> dictio)
        {
            Debug.WriteLine("-- Before Filtering --");
            LogTelemetrySplinesInfo(_currentData);


            float highestSplinePosition = -1;

            long minKeyToTranslate = -1;
            bool needsToTranslate = false;

            bool lastCloseToZero = false;
            if (dictio.Last().Value.SplinePosition < 0.1)
                lastCloseToZero = true;

            Debug.WriteLine($" --- Min Key: {minKeyToTranslate}");
            float lastSplinePosition = -1;
            foreach (var data in dictio)
            {
                if (!needsToTranslate)
                {
                    if (data.Value.SplinePosition < lastSplinePosition && minKeyToTranslate == -1)
                    {
                        needsToTranslate = true;
                        minKeyToTranslate = data.Key;
                        Debug.WriteLine($" --- Min Key: {minKeyToTranslate} - {data.Value.SplinePosition:F12}");
                    }
                    if (!needsToTranslate)
                        lastSplinePosition = data.Value.SplinePosition;
                }

                if (needsToTranslate)
                    if (data.Value.SplinePosition > highestSplinePosition)
                        highestSplinePosition = data.Value.SplinePosition;
            }

            if (needsToTranslate)
            {
                Debug.WriteLine("-- Requires Filtering --");
                float translation = 1 - dictio.First().Value.SplinePosition;
                if (lastCloseToZero)
                    translation = dictio.First().Value.SplinePosition * -1;

                _currentData.Clear();

                Debug.WriteLine($"Translation {translation}");

                bool startTranslation = false;
                foreach (var data in dictio)
                {
                    if (!startTranslation)
                    {
                        if (data.Key == minKeyToTranslate)
                            startTranslation = true;
                        else
                        {
                            var oldPoint = data.Value;
                            oldPoint.SplinePosition = (oldPoint.SplinePosition + translation);

                            if (oldPoint.SplinePosition > 1)
                                oldPoint.SplinePosition -= 1;

                            _currentData.Add(data.Key, oldPoint);
                        }
                    }

                    if (startTranslation)
                    {
                        var oldPoint = data.Value;
                        oldPoint.SplinePosition = (1 + oldPoint.SplinePosition) + translation;

                        if (oldPoint.SplinePosition > 2)
                            oldPoint.SplinePosition -= 1;

                        _currentData.Add(data.Key, oldPoint);
                    }
                }


            }

            Debug.WriteLine("-- After Filtering --");
            LogTelemetrySplinesInfo(_currentData);
        }

        private void CreateCharts(Guid lapId)
        {
            //gridSessionViewer
            comboBoxMetrics.Items.Clear();
            gridMetrics.Children.Clear();
            textBlockMetricInfo.Text = String.Empty;

            DbLapTelemetry telemetry = LapTelemetryCollection.GetForLap(CurrentDatabase.GetCollection<DbLapTelemetry>(), lapId);

            if (telemetry == null)
            {
                Grid.SetRowSpan(gridSessionViewer, 2);
                Grid.SetRowSpan(tabControlWeekends, 2);
                transitionContentPlots.Visibility = Visibility.Collapsed;
                trackMap.Visibility = Visibility.Collapsed;
            }
            else
            {
                Grid.SetRowSpan(gridSessionViewer, 1);
                Grid.SetRowSpan(tabControlWeekends, 1);
                transitionContentPlots.Visibility = Visibility.Visible;


                if (_currentData != null)
                    _currentData.Clear();

                _currentData = telemetry.DeserializeLapData();
                telemetry = null;

                FilterTelemetrySplines(_currentData.ToDictionary(x => x.Key, x => x.Value));


                TrackData trackData = TrackNames.Tracks.Values.First(x => x.Guid == GetSelectedTrack());
                PlotUtil.trackData = trackData;
                int fullSteeringLock = SteeringLock.Get(CarDataCollection.GetCarData(CurrentDatabase, GetSelectedCar()).ParseName);

                trackMap.SetData(ref _currentData);
                trackMap.Visibility = Visibility.Visible;


                Dictionary<string, Plotter> plots = new Dictionary<string, Plotter>();
                plots.Add("Speed/Gear", (g, d) => new SpeedGearPlot(trackData, ref textBlockMetricInfo).Create(g, d));
                plots.Add("Inputs", (g, d) => new InputsPlot(trackData, ref textBlockMetricInfo, fullSteeringLock).Create(g, d));
                plots.Add("Wheel Slip", (g, d) => new WheelSlipPlot(trackData, ref textBlockMetricInfo).Create(g, d));
                plots.Add("Tyre Temperatures", (g, d) => new TyreTempsPlot(trackData, ref textBlockMetricInfo).Create(g, d));
                plots.Add("Tyre Pressures", (g, d) => new TyrePressurePlot(trackData, ref textBlockMetricInfo).Create(g, d));
                plots.Add("Brake Temperatures", (g, d) => new BrakeTempsPlot(trackData, ref textBlockMetricInfo).Create(g, d));

                if (_selectionChangedHandler != null)
                {
                    comboBoxMetrics.SelectionChanged -= _selectionChangedHandler;
                    _selectionChangedHandler = null;
                }

                comboBoxMetrics.SelectionChanged += _selectionChangedHandler = new SelectionChangedEventHandler((s, e) =>
                {
                    if (comboBoxMetrics.SelectedItem == null)
                        return;

                    previousTelemetryComboSelection = comboBoxMetrics.SelectedIndex;

                    gridMetrics.Children.Clear();
                    textBlockMetricInfo.Text = String.Empty;

                    Grid grid = new Grid();
                    gridMetrics.Children.Add(grid);

                    Plotter plotter = (Plotter)(comboBoxMetrics.SelectedItem as ComboBoxItem).DataContext;
                    grid.Children.Add(plotter.Invoke(grid, _currentData));

                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        Thread.Sleep(2000);
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                    });
                });

                foreach (var plot in plots)
                {
                    ComboBoxItem boxItem = new ComboBoxItem()
                    {
                        Content = plot.Key,
                        DataContext = plot.Value
                    };
                    comboBoxMetrics.Items.Add(boxItem);
                }

                if (comboBoxMetrics.Items.Count > 0)
                {
                    int toSelect = previousTelemetryComboSelection;
                    if (toSelect == -1) toSelect = 0;
                    comboBoxMetrics.SelectedIndex = toSelect;
                }
            }
        }
    }
}
