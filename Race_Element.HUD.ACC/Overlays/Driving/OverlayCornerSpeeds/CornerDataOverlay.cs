using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayCornerSpeeds
{
    [Overlay(Name = "Corner Data",
            Description = "Shows minimum speed and other data for each corner.",
            OverlayCategory = OverlayCategory.Lap,
            OverlayType = OverlayType.Release)]
    internal sealed class CornerDataOverlay : AbstractOverlay
    {
        internal readonly CornerDataConfiguration _config = new CornerDataConfiguration();

        internal struct CornerData
        {
            public int CornerNumber { get; set; }
            public float MinimumSpeed { get; set; }
            public float MaxLatG { get; set; }
        }

        private CornerDataCollector _collector;

        internal readonly List<CornerData> _cornerDatas;
        internal InfoTable _table;
        internal AbstractTrackData _currentTrack;
        internal int _previousCorner = -1;
        internal CornerData _currentCorner;

        public CornerDataOverlay(Rectangle rectangle) : base(rectangle, "Corner Data")
        {
            RefreshRateHz = 3;
            _cornerDatas = new List<CornerData>();
        }

        public override void SetupPreviewData()
        {
            Random rand = new Random();
            for (int i = 1; i < _config.Table.CornerAmount + 1; i++)
            {
                _cornerDatas.Add(new CornerData()
                {
                    CornerNumber = i,
                    MinimumSpeed = (float)(rand.NextDouble() * 220f),
                    MaxLatG = (float)(rand.NextDouble() * 3)
                });
            }
        }

        public override void BeforeStart()
        {
            _collector = new CornerDataCollector();

            List<int> columnWidths = new List<int> { 90 };
            if (_config.Data.MaxLatG)
                columnWidths.Add(60);
            _table = new InfoTable(12, columnWidths.ToArray());

            // set Width and Height of HUD based on amount of rows and columns in table
            Height = (int)Math.Abs(_table.Font.GetHeight(120) * _config.Table.CornerAmount);
            if (_config.Table.Header) Height += (int)Math.Abs(_table.Font.GetHeight(120));
            Width = 30 + columnWidths.Sum();

            RaceSessionTracker.Instance.OnNewSessionStarted += OnNewSessionStarted;
            _collector.Start(this);
        }

        private void OnNewSessionStarted(object sender, DbRaceSession rs)
        {
            _currentTrack = GetCurrentTrack();
            _cornerDatas.Clear();
        }

        public override void BeforeStop()
        {
            RaceSessionTracker.Instance.OnNewSessionStarted -= OnNewSessionStarted;
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
            {
                List<string> headerColumns = new List<string> { "MinKmh" };

                if (_config.Data.MaxLatG)
                    headerColumns.Add("LatG");

                _table.AddRow("  ", headerColumns.ToArray());
            }

            int currentCorner = GetCurrentCorner(pageGraphics.NormalizedCarPosition);
            bool isInCorner = false;
            if (currentCorner != -1)
                if (currentCorner == _previousCorner)
                {
                    isInCorner = true;

                    List<string> columns = new List<string>();
                    List<Color> colors = new List<Color>();

                    string minSpeed = $"{_currentCorner.MinimumSpeed:F1}";
                    minSpeed = minSpeed.FillStart(5, ' ');
                    columns.Add($"{minSpeed}");
                    colors.Add(Color.LightSlateGray);

                    if (_config.Data.MaxLatG)
                    {
                        columns.Add($"{_currentCorner.MaxLatG:F2}");
                        colors.Add(Color.LightSlateGray);
                    }

                    _table.AddRow($"{currentCorner.ToString().FillStart(2, ' ')}", columns.ToArray(), colors.ToArray());
                }

            foreach (var corner in _cornerDatas.Skip(_cornerDatas.Count - _config.Table.CornerAmount).Reverse().Take(isInCorner ? _config.Table.CornerAmount - 1 : _config.Table.CornerAmount))
            {
                List<string> columns = new List<string>();

                string minSpeed = $"{corner.MinimumSpeed:F1}";
                minSpeed = minSpeed.FillStart(5, ' ');
                columns.Add($"{minSpeed}");

                if (_config.Data.MaxLatG)
                    columns.Add($"{corner.MaxLatG:F2}");

                _table.AddRow($"{corner.CornerNumber.ToString().FillStart(2, ' ')}", columns.ToArray());
            }

            // draw table of previous corners, min speed? corner g? min gear? 
            _table.Draw(g);
        }
    }
}
