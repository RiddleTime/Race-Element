using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.Util.SystemExtensions;
using System;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayLapDeltaGraph
{
    [Overlay(Name = "Lap Delta Trace",
        Description = "Shows a graph of the lap delta.",
        OverlayCategory = OverlayCategory.Lap,
        OverlayType = OverlayType.Release,
        Version = 1)]
    internal sealed class LapDeltaTraceOverlay : AbstractOverlay
    {
        private readonly LapDeltaTraceConfiguration _config = new LapDeltaTraceConfiguration();
        internal sealed class LapDeltaTraceConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Chart", "Customize the charts refresh rate, data points or change the max amount of delta time.")]
            public ChartGrouping Chart { get; set; } = new ChartGrouping();
            public class ChartGrouping
            {
                [ToolTip("The amount of datapoints shown, this changes the width of the overlay.")]
                [IntRange(50, 800, 10)]
                public int Width { get; set; } = 300;

                [ToolTip("The amount of datapoints shown, this changes the width of the overlay.")]
                [IntRange(80, 250, 10)]
                public int Height { get; set; } = 120;

                [ToolTip("Set the thickness of the lines in the chart.")]
                [IntRange(1, 4, 1)]
                public int LineThickness { get; set; } = 2;

                [ToolTip("Sets the maximum amount of delta displayed.")]
                [FloatRange(0.5f, 3f, 0.5f, 1)]
                public float MaxDelta { get; set; } = 1f;

                [ToolTip("Sets the data collection rate, this does affect cpu usage at higher values.")]
                [IntRange(5, 20, 5)]
                public int Herz { get; set; } = 5;

                [ToolTip("Show horizontal grid lines.")]
                public bool GridLines { get; set; } = true;

                [ToolTip("Show the lap delta trace when spectating other cars.")]
                public bool Spectator { get; set; } = true;

                [ToolTip("Hide the Lap Delta Trace HUD during a Race session.")]
                public bool HideForRace { get; set; } = false;
            }

            public LapDeltaTraceConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        private readonly LapDeltaDataCollector _collector;
        private readonly LapDeltaGraph _graph;

        public LapDeltaTraceOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta Trace")
        {
            this.Width = _config.Chart.Width;
            this.Height = _config.Chart.Height;
            this.RefreshRateHz = _config.Chart.Herz;
            _collector = new LapDeltaDataCollector(_config.Chart.Width - 1)
            {
                MaxDelta = _config.Chart.MaxDelta,
            };
            _graph = new LapDeltaGraph(0, 0, _config.Chart.Width - 1, _config.Chart.Height - 1, _collector, _config);
        }

        public override void SetupPreviewData()
        {
            if (_collector != null)
            {
                _collector.PositiveDeltaData.Clear();
                _collector.NegativeDeltaData.Clear();

                for (int i = 0; i <= _config.Chart.Width + 10; i++)
                {
                    float randomDelta = (float)Math.Sin(i / Math.PI / 10f);
                    randomDelta *= (float)Math.Pow(randomDelta, _config.Chart.MaxDelta);
           
                    randomDelta.Clip(-_config.Chart.MaxDelta, _config.Chart.MaxDelta);

                    _collector.Collect(randomDelta);
                }
            }
        }

        public sealed override void BeforeStop() => _graph?.Dispose();

        public override bool ShouldRender()
        {
            if (_config.Chart.HideForRace && !this.IsRepositioning && pageGraphics.SessionType == ACCSharedMemory.AcSessionType.AC_RACE)
                return false;

            if (_config.Chart.Spectator && RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
                return true;

            return base.ShouldRender();
        }

        public override void Render(Graphics g)
        {
            _collector?.Collect(GetDelta());
            _graph?.Draw(g);
        }

        private float GetDelta()
        {
            float delta = (float)pageGraphics.DeltaLapTimeMillis / 1000;

            if (_config.Chart.Spectator)
            {
                int focusedIndex = broadCastRealTime.FocusedCarIndex;
                if (RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, focusedIndex))
                    lock (EntryListTracker.Instance.Cars)
                    {
                        if (EntryListTracker.Instance.Cars.Any())
                        {
                            var car = EntryListTracker.Instance.Cars.First(car => car.Key == focusedIndex);
                            delta = car.Value.RealtimeCarUpdate.Delta / 1000f;
                        }
                    }
            }

            delta.Clip(-_config.Chart.MaxDelta, _config.Chart.MaxDelta);

            return delta;
        }
    }
}
