using ACC_Manager.Util.SystemExtensions;
using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using ACCManager.Controls.Telemetry.RaceSessions;
using ACCManager.Data;
using ACCManager.Data.ACC.Database;
using ACCManager.Data.ACC.Database.GameData;
using ACCManager.Data.ACC.Database.LapDataDB;
using ACCManager.Data.ACC.Database.SessionData;
using ACCManager.Data.ACC.Database.Telemetry;
using ACCManager.Data.ACC.Session;
using ACCManager.Data.ACC.Tracks;
using ACCManager.Util;
using LiteDB;
using ScottPlot;
using ScottPlot.Styles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static ACCManager.Data.ACC.Tracks.TrackNames;
using TrackData = ACCManager.Data.ACC.Tracks.TrackNames.TrackData;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for RaceSessionBrowser.xaml
    /// </summary>
    public partial class RaceSessionBrowser : UserControl
    {
        public static RaceSessionBrowser Instance { get; private set; }
        private LiteDatabase CurrentDatabase;

        private readonly IStyle DefaultPlotStyle = ScottPlot.Style.Black;
        private readonly IPalette WheelPositionPallete = Palette.OneHalfDark;

        public RaceSessionBrowser()
        {
            InitializeComponent();

            this.Loaded += (s, e) => FindRaceWeekends();

            comboTracks.SelectionChanged += (s, e) => FillCarComboBox();
            comboCars.SelectionChanged += (s, e) => LoadSessionList();
            listViewRaceSessions.SelectionChanged += (s, e) => LoadSession();

            gridTabHeaderLocalSession.MouseRightButtonUp += (s, e) => FindRaceWeekends();

            RaceSessionTracker.Instance.OnRaceWeekendEnded += (s, e) => FindRaceWeekends();

            Instance = this;
        }

        private void FindRaceWeekends()
        {
            Dispatcher.Invoke(() =>
            {
                localRaceWeekends.Items.Clear();

                DirectoryInfo dataDir = new DirectoryInfo(FileUtil.AccManangerDataPath);
                if (!dataDir.Exists)
                    return;

                var raceWeekendFiles = new DirectoryInfo(FileUtil.AccManangerDataPath).EnumerateFiles()
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

            TimeSpan duration = session.UtcEnd.Subtract(session.UtcStart);
            sessionInfo += $" - Duration: {duration:hh\\:mm\\:ss}";

            int potentialBestLapTime = laps.GetPotentialFastestLapTime();
            if (potentialBestLapTime != -1)
                sessionInfo += $" - Potential best: {new TimeSpan(0, 0, 0, 0, potentialBestLapTime):mm\\:ss\\:fff}";

            stackerSessionViewer.Children.Add(new TextBlock()
            {
                Text = sessionInfo,
                FontSize = 14
            });

            gridSessionLaps.Children.Add(GetLapDataGrid(laps));

            transitionContentPlots.Visibility = Visibility.Collapsed;
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
                //Height = 550,
                ItemsSource = data,
                AutoGenerateColumns = false,
                CanUserDeleteRows = false,
                CanUserAddRows = false,
                IsReadOnly = true,
                EnableRowVirtualization = false,
                SelectionMode = DataGridSelectionMode.Single,
                SelectionUnit = DataGridSelectionUnit.FullRow,
                GridLinesVisibility = DataGridGridLinesVisibility.Vertical,
                AlternatingRowBackground = new SolidColorBrush(Color.FromArgb(25, 0, 0, 0)),
                RowBackground = Brushes.Transparent,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
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
            };

            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Lap",
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
                Header = "Sector 1",
                Binding = new Binding("Sector1") { Converter = new DivideBy1000ToFloatConverter() }
            });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Sector 2",
                Binding = new Binding("Sector2") { Converter = new DivideBy1000ToFloatConverter() }
            });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Sector 3",
                Binding = new Binding("Sector3") { Converter = new DivideBy1000ToFloatConverter() }
            });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Fuel Used",
                Binding = new Binding("FuelUsage") { Converter = new DivideBy1000ToFloatConverter() }
            });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Fuel in tank",
                Binding = new Binding("FuelInTank")
            });
            grid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Type",
                Binding = new Binding("LapType")
            });


            grid.SelectedCellsChanged += (s, e) =>
            {
                DbLapData lapdata = (DbLapData)grid.SelectedItem;
                CreateCharts(lapdata.Id);
            };

            return grid;
        }

        private void CreateCharts(Guid lapId)
        {
            DbLapTelemetry telemetry = LapTelemetryCollection.GetForLap(CurrentDatabase.GetCollection<DbLapTelemetry>(), lapId);
            if (telemetry == null)
            {
                transitionContentPlots.Visibility = Visibility.Collapsed;
            }
            else
            {
                transitionContentPlots.Visibility = Visibility.Visible;

                Dictionary<long, TelemetryPoint> dict = telemetry.DeserializeLapData();

                Grid inputsTabGrid = new Grid();
                tabItemInputs.Content = inputsTabGrid;
                inputsTabGrid.Children.Add(GetInputPlot(inputsTabGrid, telemetry, dict));

                Grid tyreTabGrid = new Grid();
                tabItemTyreTemps.Content = tyreTabGrid;
                tyreTabGrid.Children.Add(GetTyreTempPlot(tyreTabGrid, telemetry, dict));

                Grid tyrePressureGrid = new Grid();
                tabItemTyrePressures.Content = tyrePressureGrid;
                tyrePressureGrid.Children.Add(GetTyrePressurePlot(tyrePressureGrid, telemetry, dict));

                Grid brakeTempsGrid = new Grid();
                tabItemBrakeTemps.Content = brakeTempsGrid;
                brakeTempsGrid.Children.Add(GetBrakeTempsPlot(brakeTempsGrid, telemetry, dict));
            }

            ThreadPool.QueueUserWorkItem(x => GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true));
        }

        internal WpfPlot GetInputPlot(Grid outerGrid, DbLapTelemetry telemetry, Dictionary<long, TelemetryPoint> dict)
        {
            WpfPlot wpfPlot = new WpfPlot
            {
                Cursor = Cursors.Hand,
            };

            SetDefaultWpfPlotConfiguration(ref wpfPlot);

            wpfPlot.Height = outerGrid.ActualHeight;
            wpfPlot.MaxHeight = outerGrid.MaxHeight;
            wpfPlot.MinHeight = outerGrid.MinHeight;
            outerGrid.SizeChanged += (se, ev) =>
            {
                wpfPlot.Height = outerGrid.ActualHeight;
                wpfPlot.MaxHeight = outerGrid.MaxHeight;
                wpfPlot.MinHeight = outerGrid.MinHeight;
            };

            TrackData trackData = TrackNames.Tracks.Values.First(x => x.Guid == GetSelectedTrack());

            double[] splines = dict.Select(x => (double)x.Value.SplinePosition * trackData.TrackLength).ToArray();
            double[] gasDatas = dict.Select(x => (double)x.Value.InputsData.Gas * 100).ToArray();
            double[] brakeDatas = dict.Select(x => (double)x.Value.InputsData.Brake * 100).ToArray();
            double[] steeringDatas = dict.Select(x => (double)x.Value.InputsData.SteerAngle).ToArray();

            if (splines.Length == 0)
                return wpfPlot;

            Plot plot = wpfPlot.Plot;
            plot.SetAxisLimitsY(-5, 105);

            var gasPlot = plot.AddSignalXY(splines, gasDatas, color: System.Drawing.Color.Green, label: "Throttle");
            gasPlot.FillBelow(upperColor: System.Drawing.Color.FromArgb(95, 0, 255, 0), lowerColor: System.Drawing.Color.Transparent);

            var brakePlot = plot.AddSignalXY(splines, brakeDatas, color: System.Drawing.Color.Red, label: "Brake");
            brakePlot.FillBelow(upperColor: System.Drawing.Color.FromArgb(140, 255, 0, 0), lowerColor: System.Drawing.Color.Transparent);

            var steeringPlot = plot.AddSignalXY(splines, steeringDatas, color: System.Drawing.Color.WhiteSmoke, label: "Steering");
            steeringPlot.YAxisIndex = 1;

            plot.SetAxisLimits(xMin: 0, xMax: trackData.TrackLength, yMin: -1.05, yMax: 1.05, yAxisIndex: 1);
            plot.SetOuterViewLimits(0, trackData.TrackLength, -3, 103);
            plot.SetOuterViewLimits(0, trackData.TrackLength, -1.05, 1.05, yAxisIndex: 1);

            plot.XLabel("Distance (meters)");
            plot.YLabel("Inputs");

            plot.YAxis2.Ticks(true);
            plot.YAxis2.Label("Steering");

            plot.Palette = new ScottPlot.Palettes.PolarNight();

            SetDefaultPlotStyles(ref plot);

            wpfPlot.Refresh();

            return wpfPlot;
        }


        internal WpfPlot GetTyreTempPlot(Grid outerGrid, DbLapTelemetry telemetry, Dictionary<long, TelemetryPoint> dict)
        {
            WpfPlot wpfPlot = new WpfPlot
            {
                Cursor = Cursors.Hand,
            };

            SetDefaultWpfPlotConfiguration(ref wpfPlot);

            wpfPlot.Height = outerGrid.ActualHeight;
            wpfPlot.MaxHeight = outerGrid.MaxHeight;
            wpfPlot.MinHeight = outerGrid.MinHeight;
            outerGrid.SizeChanged += (se, ev) =>
            {
                wpfPlot.Height = outerGrid.ActualHeight;
                wpfPlot.MaxHeight = outerGrid.MaxHeight;
                wpfPlot.MinHeight = outerGrid.MinHeight;
            };

            TrackData trackData = TrackNames.Tracks.Values.First(x => x.Guid == GetSelectedTrack());

            Plot plot = wpfPlot.Plot;
            plot.Palette = WheelPositionPallete;
            plot.Benchmark(false);

            double[][] tyreTemps = new double[4][];
            double minTemp = int.MaxValue;
            double maxTemp = int.MinValue;
            double[] splines = dict.Select(x => (double)x.Value.SplinePosition).ToArray();
            if (splines.Length == 0)
                return wpfPlot;

            for (int i = 0; i < 4; i++)
            {
                tyreTemps[i] = dict.Select(x => (double)x.Value.TyreData.TyreCoreTemperature[i]).ToArray();

                minTemp.ClipMax(tyreTemps[i].Min());
                maxTemp.ClipMin(tyreTemps[i].Max());

                plot.AddSignalXY(splines, tyreTemps[i], label: Enum.GetNames(typeof(SetupConverter.Wheel))[i]);
            }

            double padding = 2;
            plot.SetAxisLimitsX(xMin: 0, xMax: trackData.TrackLength);
            plot.SetAxisLimitsY(minTemp - padding, maxTemp + padding);
            plot.SetOuterViewLimits(0, trackData.TrackLength, minTemp - padding, maxTemp + padding);
            plot.XLabel("Distance (%)");
            plot.YLabel("Temperature (C)");
            SetDefaultPlotStyles(ref plot);

            wpfPlot.Refresh();

            return wpfPlot;
        }

        internal WpfPlot GetTyrePressurePlot(Grid outerGrid, DbLapTelemetry telemetry, Dictionary<long, TelemetryPoint> dict)
        {
            WpfPlot wpfPlot = new WpfPlot
            {
                Cursor = Cursors.Hand,
            };

            TrackData trackData = TrackNames.Tracks.Values.First(x => x.Guid == GetSelectedTrack());

            SetDefaultWpfPlotConfiguration(ref wpfPlot);

            wpfPlot.Height = outerGrid.ActualHeight;
            wpfPlot.MaxHeight = outerGrid.MaxHeight;
            wpfPlot.MinHeight = outerGrid.MinHeight;
            outerGrid.SizeChanged += (se, ev) =>
            {
                wpfPlot.Height = outerGrid.ActualHeight;
                wpfPlot.MaxHeight = outerGrid.MaxHeight;
                wpfPlot.MinHeight = outerGrid.MinHeight;
            };


            Plot plot = wpfPlot.Plot;
            plot.Palette = WheelPositionPallete;
            plot.Benchmark(false);

            double[][] tyrePressures = new double[4][];
            double minPressure = int.MaxValue;
            double maxPressure = int.MinValue;

            double[] splines = dict.Select(x => (double)x.Value.SplinePosition).ToArray();
            if (splines.Length == 0)
                return wpfPlot;

            for (int i = 0; i < 4; i++)
            {
                tyrePressures[i] = dict.Select(x => (double)x.Value.TyreData.TyrePressure[i]).ToArray();

                minPressure.ClipMax(tyrePressures[i].Min());
                maxPressure.ClipMin(tyrePressures[i].Max());

                plot.AddSignalXY(splines, tyrePressures[i], label: Enum.GetNames(typeof(SetupConverter.Wheel))[i]);
            }

            double padding = 0.1;
            double defaultMinPressure = 27, defaultMaxPressure = 28;
            if (minPressure > defaultMinPressure && maxPressure < defaultMaxPressure)
            {
                minPressure.ClipMax(defaultMinPressure);
                maxPressure.ClipMin(defaultMaxPressure);
            }

            plot.SetAxisLimitsX(xMin: 0, xMax: trackData.TrackLength);
            plot.SetAxisLimitsY(minPressure - padding, maxPressure + padding);
            plot.SetOuterViewLimits(0, trackData.TrackLength, minPressure - padding, maxPressure + padding);
            plot.XLabel("Distance (%)");
            plot.YLabel("Pressure (PSI)");
            SetDefaultPlotStyles(ref plot);

            wpfPlot.Refresh();

            return wpfPlot;
        }

        internal WpfPlot GetBrakeTempsPlot(Grid outerGrid, DbLapTelemetry telemetry, Dictionary<long, TelemetryPoint> dict)
        {
            WpfPlot wpfPlot = new WpfPlot
            {
                Cursor = Cursors.Hand,
            };

            SetDefaultWpfPlotConfiguration(ref wpfPlot);

            wpfPlot.Height = outerGrid.ActualHeight;
            wpfPlot.MaxHeight = outerGrid.MaxHeight;
            wpfPlot.MinHeight = outerGrid.MinHeight;
            outerGrid.SizeChanged += (se, ev) =>
            {
                wpfPlot.Height = outerGrid.ActualHeight;
                wpfPlot.MaxHeight = outerGrid.MaxHeight;
                wpfPlot.MinHeight = outerGrid.MinHeight;
            };

            TrackData trackData = TrackNames.Tracks.Values.First(x => x.Guid == GetSelectedTrack());

            Plot plot = wpfPlot.Plot;
            plot.Palette = WheelPositionPallete;
            plot.Benchmark(false);

            double[][] brakeTemps = new double[4][];
            double minTemp = int.MaxValue;
            double maxTemp = int.MinValue;

            double[] splines = dict.Select(x => (double)x.Value.SplinePosition * trackData.TrackLength).ToArray();
            if (splines.Length == 0)
                return wpfPlot;

            for (int i = 0; i < 4; i++)
            {
                brakeTemps[i] = dict.Select(x => (double)x.Value.BrakeData.BrakeTemperature[i]).ToArray();

                minTemp.ClipMax(brakeTemps[i].Min());
                maxTemp.ClipMin(brakeTemps[i].Max());

                plot.AddSignalXY(splines, brakeTemps[i], label: Enum.GetNames(typeof(SetupConverter.Wheel))[i]);
            }


            double padding = 10;
            plot.SetAxisLimitsX(xMin: 0, xMax: trackData.TrackLength);
            plot.SetAxisLimitsY(minTemp - padding, maxTemp + padding);
            plot.SetOuterViewLimits(0, trackData.TrackLength, minTemp - padding, maxTemp + padding);
            plot.XLabel("Distance (meters)");
            plot.YLabel("Temperature (C)");

            SetDefaultPlotStyles(ref plot);
            wpfPlot.Refresh();

            return wpfPlot;
        }


        private void SetDefaultPlotStyles(ref Plot plot)
        {
            plot.YAxis.TickLabelStyle(color: System.Drawing.Color.White);
            plot.XAxis.TickLabelStyle(color: System.Drawing.Color.White);
            plot.YAxis2.TickLabelStyle(color: System.Drawing.Color.White);

            //plot.AxisZoom(1, 1);

            plot.Style(DefaultPlotStyle);
            plot.Legend(true);
            plot.YAxis.RulerMode(true);
            plot.YAxis2.RulerMode(true);
        }

        private void SetDefaultWpfPlotConfiguration(ref WpfPlot plot)
        {
            plot.Configuration.DoubleClickBenchmark = false;
            plot.Configuration.LockVerticalAxis = true;
            plot.Configuration.Quality = ScottPlot.Control.QualityMode.High;
            plot.Configuration.MiddleClickDragZoom = false;
            plot.Configuration.MiddleClickAutoAxis = true;
            plot.Configuration.RightClickDragZoom = false;
        }
    }
}
