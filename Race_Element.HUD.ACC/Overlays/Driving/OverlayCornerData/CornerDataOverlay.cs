using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.Data.ACC.Tracks;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayCornerData
{
    [Overlay(Name = "Corner Data",
            Description = "Shows Corner Delta and other data like speeds for each corner.",
            OverlayCategory = OverlayCategory.Lap,
            OverlayType = OverlayType.Release)]
    internal sealed class CornerDataOverlay : AbstractOverlay
    {
        internal readonly CornerDataConfiguration _config = new CornerDataConfiguration();

        internal class CornerData
        {
            public int CornerNumber { get; set; }
            public float MinimumSpeed { get; set; }
            public float AverageSpeed { get; set; }
            public float MaxLatG { get; set; }
            public int EntryDeltaMilliseconds { get; set; }
            public int ExitDeltaMilliseconds { get; set; }
        }

        private CornerDataCollector _collector;
        private InfoTable _table;

        internal readonly List<CornerData> _cornerDatas;
        internal Dictionary<int, CornerData> _bestLapCorners;
        internal AbstractTrackData _currentTrack;
        internal int _previousCorner = -1;
        internal CornerData _currentCorner;

        public CornerDataOverlay(Rectangle rectangle) : base(rectangle, "Corner Data")
        {
            RefreshRateHz = 2;
            _cornerDatas = new List<CornerData>();
            _bestLapCorners = new Dictionary<int, CornerData>();
        }

        public override void SetupPreviewData()
        {
            var trackKyalami = TrackData.Tracks.FirstOrDefault(x => x.GameName == "Kyalami");
            if (trackKyalami == null)
                return;
            pageStatic.Track = trackKyalami.GameName;
            _currentTrack = GetCurrentTrack();

            Random rand = new Random();
            int delta = rand.Next(-200, 600);
            for (int i = 1; i < _currentTrack.CornerNames.Count + 1; i++)
            {
                delta += rand.Next(-15, 15);
                float minimumSpeed = (float)(rand.NextDouble() * 230f);
                minimumSpeed.ClipMin((float)rand.NextDouble() * rand.Next(60, 120));
                float maxLatG = (float)(rand.NextDouble() * 3);
                _cornerDatas.Add(new CornerData()
                {
                    CornerNumber = i,
                    MinimumSpeed = minimumSpeed,
                    AverageSpeed = minimumSpeed + (float)(rand.NextDouble() * 2.5f),
                    MaxLatG = maxLatG,
                    EntryDeltaMilliseconds = delta,
                    ExitDeltaMilliseconds = delta + rand.Next(-30, 30),
                });

                float randMinSpeed = (float)(rand.NextDouble() * 2.5f);
                if (rand.NextDouble() > 0.617d)
                    randMinSpeed *= -1;
                _bestLapCorners.Add(i, new CornerData()
                {
                    CornerNumber = i,
                    MinimumSpeed = minimumSpeed + randMinSpeed,
                    AverageSpeed = minimumSpeed + 2 + (float)(rand.NextDouble() * 4.5f),
                    MaxLatG = (float)(maxLatG + rand.NextDouble() + .5 * 0.08f),
                    EntryDeltaMilliseconds = delta - 1,
                    ExitDeltaMilliseconds = delta - 1 + rand.Next(-30, 30),
                });
            }
        }

        public override void BeforeStart()
        {
            _collector = new CornerDataCollector();

            // create info table based on selected config
            List<int> columnWidths = new List<int> { 90 }; // delta

            if (_config.Data.MinimumSpeed)
            {
                columnWidths.Add(80);
                if (_config.Data.DeltaSource != CornerDataConfiguration.DeltaSource.Off)
                    columnWidths.Add(50);
            }

            if (_config.Data.AverageSpeed)
            {
                columnWidths.Add(80);
                if (_config.Data.DeltaSource != CornerDataConfiguration.DeltaSource.Off)
                    columnWidths.Add(50);
            }

            if (_config.Data.MaxLatG)
                columnWidths.Add(60);

            _table = new InfoTable(12, columnWidths.ToArray());

            // set Width and Height of HUD based on amount of rows and columns in table
            Height = (int)Math.Abs(_table.Font.GetHeight(120) * _config.Table.CornerAmount);
            if (_config.Table.Header) Height += (int)Math.Abs(_table.Font.GetHeight(120));
            Width = 30 + columnWidths.Sum();

            RaceSessionTracker.Instance.OnNewSessionStarted += OnNewSessionStarted;
            RaceSessionTracker.Instance.OnSessionIndexChanged += Instance_OnSessionIndexChanged;// reset during sessions?
            if (_config.Data.DeltaSource != CornerDataConfiguration.DeltaSource.Off) LapTracker.Instance.LapFinished += OnLapFinished;

            _collector.Start(this);
        }

        private void Instance_OnSessionIndexChanged(object sender, int e)
        {
            _currentTrack = GetCurrentTrack();
            _cornerDatas.Clear();
        }

        private void OnLapFinished(object sender, DbLapData finishedLap)
        {
            // don't update if source is set to best session lap.. (obviously don't update when finished lap isn't the fastest one or is an in or outlap)
            if (_config.Data.DeltaSource == CornerDataConfiguration.DeltaSource.BestSessionLap
                && (finishedLap.Index != LapTracker.Instance.Laps.GetFastestLapIndex() || finishedLap.LapType != Broadcast.LapType.Regular))
                return;

            // update best lap corner data 
            Trace.WriteLine($"Updating best lap corner data:\n{finishedLap}");
            int maxCornerCount = _currentTrack.CornerNames.Count;
            var cornerData = _cornerDatas.Take(maxCornerCount);
            if (cornerData.Any())
            {
                //Debug.WriteLine("first corner " + cornerData.First().CornerNumber);
                //Debug.WriteLine("last corner " + cornerData.Last().CornerNumber);
                _bestLapCorners.Clear();
                foreach (var data in cornerData)
                    _bestLapCorners.Add(data.CornerNumber, data);
            }
        }

        private void OnNewSessionStarted(object sender, DbRaceSession rs)
        {
            _currentTrack = GetCurrentTrack();
            _cornerDatas.Clear();
        }

        public override void BeforeStop()
        {
            RaceSessionTracker.Instance.OnNewSessionStarted -= OnNewSessionStarted;
            RaceSessionTracker.Instance.OnSessionIndexChanged += Instance_OnSessionIndexChanged;
            if (_config.Data.DeltaSource != CornerDataConfiguration.DeltaSource.Off) LapTracker.Instance.LapFinished -= OnLapFinished;

            _cornerDatas.Clear();
            _collector.Stop();
        }

        internal AbstractTrackData GetCurrentTrack()
        {
            if (pageStatic.Track == string.Empty) return null;

            return Tracks.Find(x => x.GameName == pageStatic.Track);
        }

        internal int GetCurrentCorner(float normalizedTrackPosition)
        {
            _currentTrack ??= GetCurrentTrack();

            if (_currentTrack == null) return -1;
            if (pageStatic.Track != _currentTrack.GameName)
            {
                _currentTrack = GetCurrentTrack();
                if (_currentTrack == null) return -1;
            }

            try
            {
                return _currentTrack.CornerNames.First(x => normalizedTrackPosition > x.Key.From && normalizedTrackPosition < x.Key.To).Value.Item1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public override void Render(Graphics g)
        {
            if (_config.Table.Header)
                AddHeaderRow();

            int currentCorner = GetCurrentCorner(pageGraphics.NormalizedCarPosition);
            bool isInCorner = false;
            if (currentCorner != -1)
                if (currentCorner == _previousCorner)
                {
                    isInCorner = true;

                    List<string> columns = new List<string>();
                    List<Color> colors = new List<Color>();

                    // add delta column
                    int delta = _currentCorner.EntryDeltaMilliseconds - pageGraphics.DeltaLapTimeMillis;
                    columns.Add($"{delta / 1000:F3}");
                    colors.Add(Color.FromArgb(190, Color.White));

                    if (_config.Data.MinimumSpeed)
                    {
                        // add min speed column
                        string minSpeed = string.Empty;
                        if (_currentCorner.MinimumSpeed != float.MaxValue) // initial value for min speed is float.maxvalue
                            minSpeed = $"{_currentCorner.MinimumSpeed:F2}";
                        minSpeed = minSpeed.FillStart(6, ' ');
                        columns.Add($"{minSpeed}");
                        colors.Add(Color.FromArgb(190, Color.White));

                        if (_config.Data.DeltaSource != CornerDataConfiguration.DeltaSource.Off)
                            columns.Add(string.Empty);
                    }

                    if (_config.Data.AverageSpeed)
                    {
                        string avgSpeed = $"{_currentCorner.AverageSpeed:F2}";
                        avgSpeed.FillStart(6, ' ');
                        columns.Add(avgSpeed);
                        colors.Add(Color.FromArgb(190, Color.White));

                        if (_config.Data.DeltaSource != CornerDataConfiguration.DeltaSource.Off)
                        {
                            columns.Add(string.Empty);
                            colors.Add(Color.FromArgb(190, Color.White));
                        }
                    }

                    // add max lateral g column
                    if (_config.Data.MaxLatG)
                    {
                        columns.Add($"{_currentCorner.MaxLatG:F2}");
                        colors.Add(Color.FromArgb(190, Color.White));
                    }

                    _table.AddRow($"{currentCorner.ToString().FillStart(2, ' ')}", columns.ToArray(), colors.ToArray());
                }

            foreach (var corner in _cornerDatas.Skip(_cornerDatas.Count - _config.Table.CornerAmount).Reverse().Take(isInCorner ? _config.Table.CornerAmount - 1 : _config.Table.CornerAmount))
            {
                List<string> columns = new List<string>();
                List<Color> colors = new List<Color>();

                // add delta column
                if (_bestLapCorners.Count == _currentTrack.CornerNames.Count)
                {
                    int deltaDelta = corner.ExitDeltaMilliseconds - corner.EntryDeltaMilliseconds;
                    string deltaDeltaString = $"{deltaDelta / 1000f:F3}";
                    if (deltaDelta < 0) deltaDeltaString = deltaDeltaString.Substring(1);
                    deltaDeltaString.FillStart(6, ' ');
                    columns.Add(deltaDeltaString);
                    Color deltaColor = deltaDelta < 0 ? _config.Colors.DeltaFaster : _config.Colors.DeltaSlower;
                    colors.Add(deltaColor);
                }
                else
                {
                    columns.Add("-");
                    colors.Add(Color.FromArgb(190, Color.White));
                }

                if (_config.Data.MinimumSpeed)
                {
                    // add min speed column
                    string minSpeed = $"{corner.MinimumSpeed:F2}";
                    minSpeed = minSpeed.FillStart(6, ' ');
                    if (corner.MinimumSpeed == float.MaxValue)
                        minSpeed = string.Empty;
                    columns.Add($"{minSpeed}");
                    colors.Add(Color.White);

                    // if any delta source is enabled add delta compared to best for min speed
                    if (_config.Data.DeltaSource != CornerDataConfiguration.DeltaSource.Off)
                    {
                        if (_bestLapCorners.TryGetValue(corner.CornerNumber, out CornerData bestCorner))
                        {
                            float minSpeedDelta = bestCorner.MinimumSpeed - corner.MinimumSpeed;

                            string deltaText = $"{minSpeedDelta:F1}";
                            if (minSpeedDelta > 0)
                                deltaText = "+" + deltaText;
                            deltaText.FillStart(4, ' ');
                            columns.Add(deltaText);

                            Color minSpeedDeltaColor = minSpeedDelta switch
                            {
                                var d when d > 0 => _config.Colors.SpeedFaster,
                                var d when d < 0 => _config.Colors.SpeedSlower,
                                _ => Color.White,
                            };
                            colors.Add(minSpeedDeltaColor);
                        }
                        else
                            columns.Add(string.Empty);
                    }
                }

                // if average speed is enabled, add column for it
                if (_config.Data.AverageSpeed)
                {
                    string avgSpeed = $"{corner.AverageSpeed:F2}";
                    avgSpeed = avgSpeed.FillStart(6, ' ');
                    columns.Add(avgSpeed);
                    colors.Add(Color.White);

                    // if delta is enabled add delta compared to selected best
                    if (_config.Data.DeltaSource != CornerDataConfiguration.DeltaSource.Off)
                    {
                        if (_bestLapCorners.TryGetValue(corner.CornerNumber, out CornerData best))
                        {
                            float avgSpeedDelta = best.AverageSpeed - corner.AverageSpeed;

                            string deltaText = $"{avgSpeedDelta:F1}";
                            if (avgSpeedDelta > 0)
                                deltaText = "+" + deltaText;
                            deltaText.FillStart(4, ' ');
                            columns.Add(deltaText);

                            Color avgSpeedDeltaColor = avgSpeedDelta switch
                            {
                                var d when d > 0 => _config.Colors.SpeedFaster,
                                var d when d < 0 => _config.Colors.SpeedSlower,
                                _ => Color.White,
                            };
                            colors.Add(avgSpeedDeltaColor);
                        }
                        else
                            columns.Add(string.Empty);
                    }
                }


                if (_config.Data.MaxLatG)
                    columns.Add($"{corner.MaxLatG:F2}");

                _table.AddRow($"{corner.CornerNumber.ToString().FillStart(2, ' ')}", columns.ToArray(), colors.ToArray());
            }

            // draw table of previous corners, min speed? corner g? min gear? 
            _table.Draw(g);
        }

        private void AddHeaderRow()
        {
            List<string> headerColumns = new List<string> { "Delta" };
            List<Color> headerColours = new List<Color> { Color.White };

            if (_config.Data.MinimumSpeed)
            {
                headerColumns.Add("MinKmh");
                headerColours.Add(Color.White);

                if (_config.Data.DeltaSource != CornerDataConfiguration.DeltaSource.Off)
                    headerColumns.Add("Δ");
                switch (_config.Data.DeltaSource)
                {
                    case CornerDataConfiguration.DeltaSource.LastLap: headerColours.Add(Color.Cyan); break;
                    case CornerDataConfiguration.DeltaSource.BestSessionLap: headerColours.Add(Color.LightGreen); break;
                    case CornerDataConfiguration.DeltaSource.Off: break;
                }
            }

            if (_config.Data.AverageSpeed)
            {
                headerColumns.Add("AvgKmh");
                headerColours.Add(Color.White);

                if (_config.Data.DeltaSource != CornerDataConfiguration.DeltaSource.Off)
                    headerColumns.Add("Δ");

                switch (_config.Data.DeltaSource)
                {
                    case CornerDataConfiguration.DeltaSource.LastLap: headerColours.Add(Color.Cyan); break;
                    case CornerDataConfiguration.DeltaSource.BestSessionLap: headerColours.Add(Color.LightGreen); break;
                    case CornerDataConfiguration.DeltaSource.Off: break;
                }
            }

            if (_config.Data.MaxLatG)
                headerColumns.Add("LatG");

            _table.AddRow("  ", headerColumns.ToArray(), headerColours.ToArray());
        }

    }
}
