using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayCornerSpeeds
{
    [Overlay(Name = "Corner Data",
            Description = "Shows minimum speed and other data for each corner.",
            OverlayCategory = OverlayCategory.Lap,
            OverlayType = OverlayType.Release)]
    internal sealed class CornerDataOverlay : AbstractOverlay
    {
        private readonly CornerDataConfiguration _config = new CornerDataConfiguration();

        private readonly List<CornerData> _cornerDatas;
        private InfoTable _table;
        private AbstractTrackData _currentTrack;
        private int _previousCorner = -1;
        private CornerData _currentCorner;

        public CornerDataOverlay(Rectangle rectangle) : base(rectangle, "Corner Data")
        {
            _cornerDatas = new List<CornerData>();
        }

        public override void SetupPreviewData()
        {
            Random rand = new Random();
            for (int i = 1; i < _config.Table.CornerCount + 1; i++)
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
            List<int> columnWidths = new List<int>();
            columnWidths.Add(90);
            if (_config.Data.MaxLatG)
                columnWidths.Add(60);
            _table = new InfoTable(12, columnWidths.ToArray());

            // set Width and Height of HUD based on amount of rows and columns in table
            Height = (int)Math.Abs(_table.Font.GetHeight(120) * _config.Table.CornerCount);
            if (_config.Table.ShowHeader) Height += (int)Math.Abs(_table.Font.GetHeight(120));
            Width = 30 + columnWidths.Sum();

            RaceSessionTracker.Instance.OnNewSessionStarted += OnNewSessionStarted;
        }

        private void OnNewSessionStarted(object sender, DbRaceSession rs)
        {
            _currentTrack = GetCurrentTrack();
            _cornerDatas.Clear();
        }

        public override void BeforeStop()
        {
            RaceSessionTracker.Instance.OnNewSessionStarted -= OnNewSessionStarted;
        }

        public AbstractTrackData GetCurrentTrack()
        {
            if (pageStatic.Track == string.Empty) return null;

            return Tracks.Find(x => x.GameName == pageStatic.Track);
        }

        public int GetCurrentCorner(float normalizedTrackPosition)
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

        private struct CornerData
        {
            public int CornerNumber { get; set; }
            public float MinimumSpeed { get; set; }
            public float MaxLatG { get; set; }
        }

        public override void Render(Graphics g)
        {
            if (_config.Table.ShowHeader)
            {
                List<string> headerColumns = new List<string>();
                headerColumns.Add("MinKmh");

                if (_config.Data.MaxLatG)
                    headerColumns.Add("LatG");

                _table.AddRow("  ", headerColumns.ToArray());
            }

            int currentCorner = GetCurrentCorner(pageGraphics.NormalizedCarPosition);
            if (currentCorner == -1 && _previousCorner != -1)
            {  // corner exited
                _cornerDatas.Add(_currentCorner);
                _previousCorner = -1;
            }

            bool isInCorner = false;
            if (currentCorner != -1)
            {
                if (currentCorner == _previousCorner)
                {
                    isInCorner = true;
                    // we're still in the current corner..., check the data and build the first row
                    if (_currentCorner.MinimumSpeed > pagePhysics.SpeedKmh)
                        _currentCorner.MinimumSpeed = pagePhysics.SpeedKmh;

                    List<string> columns = new List<string>();
                    string minSpeed = $"{_currentCorner.MinimumSpeed:F1}";
                    minSpeed = minSpeed.FillStart(5, ' ');
                    columns.Add($"{minSpeed}");

                    if (_config.Data.MaxLatG)
                    {
                        float latG = pagePhysics.AccG[0];
                        if (latG < 0) latG *= -1;
                        if (_currentCorner.MaxLatG < latG)
                            _currentCorner.MaxLatG = latG;

                        columns.Add($"{_currentCorner.MaxLatG:F2}");
                    }

                    _table.AddRow($"{currentCorner.ToString().FillStart(2, ' ')}", columns.ToArray());
                }
                else
                {
                    _previousCorner = currentCorner;
                    Debug.WriteLine("Entered a new corner!");
                    _currentCorner = new CornerData()
                    {
                        CornerNumber = currentCorner,
                        MinimumSpeed = float.MaxValue
                    };
                }
            }

            foreach (var corner in _cornerDatas.Skip(_cornerDatas.Count - _config.Table.CornerCount).Reverse().Take(isInCorner ? _config.Table.CornerCount - 1 : _config.Table.CornerCount))
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
