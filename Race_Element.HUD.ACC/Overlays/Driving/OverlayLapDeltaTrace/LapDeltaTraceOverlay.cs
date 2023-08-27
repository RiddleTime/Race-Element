using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.Util.SystemExtensions;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayLapDeltaGraph
{
    [Overlay(Name = "Lap Delta Trace",
        Description = "Shows a graph of the delta.",
        OverlayCategory = OverlayCategory.Lap,
        OverlayType = OverlayType.Release,
        Version = 0.1)]
    internal class LapDeltaTraceOverlay : AbstractOverlay
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
                public int LineThickness { get; set; } = 1;

                [ToolTip("Sets the maximum amount of delta displayed.")]
                [FloatRange(0.2f, 10f, 0.1f, 1)]
                public float MaxDelta { get; set; } = 1.5f;

                [ToolTip("Sets the data collection rate, this does affect cpu usage at higher values.")]
                [IntRange(5, 20, 5)]
                public int Herz { get; set; } = 5;

                [ToolTip("Show the lap delta trace")]
                public bool Spectator { get; set; } = true;
            }

            public LapDeltaTraceConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        private LapDeltaDataCollector _collector;
        private LapDeltaGraph _graph;

        public LapDeltaTraceOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta Trace")
        {
            this.Width = _config.Chart.Width;
            this.Height = _config.Chart.Height;
            this.RefreshRateHz = _config.Chart.Herz;
        }

        public sealed override void BeforeStart()
        {
            _collector = new LapDeltaDataCollector(_config.Chart.Width - 1)
            {
                MaxDelta = _config.Chart.MaxDelta,
                Herz = _config.Chart.Herz
            };
            _graph = new LapDeltaGraph(0, 0, _config.Chart.Width - 1, _config.Chart.Height - 1, _collector, _config);
        }

        public sealed override void BeforeStop() => _graph.Dispose();


        public override bool ShouldRender()
        {
            if (_config.Chart.Spectator && RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
                return true;

            return base.ShouldRender();
        }

        public override void Render(Graphics g)
        {
            _collector.Collect(GetDelta());
            _graph.Draw(g);
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
