using RaceElement.Data.ACC.Database.RaceWeekend;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracks;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
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
            Description = "Shows corner data for each corner.",
            OverlayCategory = OverlayCategory.Lap,
            OverlayType = OverlayType.Release)]
    internal sealed class CornerDataOverlay : AbstractOverlay
    {
        private readonly CornerSpeedsConfiguration _config = new CornerSpeedsConfiguration();
        private sealed class CornerSpeedsConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Table", "Adjust what is shown in the table")]
            public readonly TableGrouping Table = new TableGrouping();
            public sealed class TableGrouping
            {
                [ToolTip("Adjust the amount corners shown as history.")]
                [IntRange(1, 5, 1)]
                public int CornerCount { get; set; } = 3;
            }

            private class DataGrouping
            {
                public bool CornerG { get; set; } = false;
            }

            public CornerSpeedsConfiguration() => AllowRescale = true;
        }

        private InfoTable _table;
        private AbstractTrackData _currentTrack;
        private int _previousCorner = -1;
        private List<CornerData> _cornerDatas = new List<CornerData>();

        public CornerDataOverlay(Rectangle rectangle) : base(rectangle, "Corner Data")
        {
            Width = 300;
            Height = 150;

        }

        public override void BeforeStart()
        {
            _table = new InfoTable(10, new int[] { 10, 70 });
            RaceSessionTracker.Instance.OnNewSessionStarted += OnNewSessionStarted;
        }

        private void OnNewSessionStarted(object sender, DbRaceSession rs)
        {
            _currentTrack = GetCurrentTrack();
        }

        public override void SetupPreviewData()
        {
            Random rand = new Random();
            for (int i = 1; i < _config.Table.CornerCount + 1; i++)
            {
                _cornerDatas.Add(new CornerData()
                {
                    CornerNumber = i,
                    MinimumSpeed = (float)(rand.NextDouble() * 100f)
                });
            }
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

        public int GetCurrentCorner()
        {
            if (_currentTrack == null) return -1;

            try
            {
                return _currentTrack.CornerNames.First(x => pageGraphics.NormalizedCarPosition > x.Key.From && pageGraphics.NormalizedCarPosition < x.Key.To).Value.Item1;
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
        }

        public override void Render(Graphics g)
        {
            int currentCorner = GetCurrentCorner();
            if (currentCorner != -1)
            {  // in a corner


                if (currentCorner != _previousCorner)
                {
                    Debug.WriteLine("Next Corner!");
                    _cornerDatas.Add(new CornerData()
                    {
                        CornerNumber = _previousCorner,
                        MinimumSpeed = pagePhysics.SpeedKmh
                    });
                    _previousCorner = currentCorner;
                    Debug.WriteLine("We just entered a new corner!");
                }
            }
            else
            {  // no corner

            }
            foreach (var corner in _cornerDatas.Skip(_cornerDatas.Count - _config.Table.CornerCount))
            {
                _table.AddRow($"{corner.CornerNumber}", new string[] { $"{corner.MinimumSpeed:F2}" });
            }
            // draw table of previous corners, min speed? corner g? min gear? 
            _table.Draw(g);
        }
    }
}
