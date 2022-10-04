using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using System.Drawing;

namespace ACCManager.HUD.ACC.Overlays.OverlayPressureTrace
{
    [Overlay(Name = "Pressure Trace", Version = 1.00, OverlayType = OverlayType.Release,
    Description = "Live graphs of the tyre pressures, green is within range, red is too high, blue is too low.")]
    internal sealed class PressureTraceOverlay : AbstractOverlay
    {
        private PressureTraceOverlayConfig _config = new PressureTraceOverlayConfig();
        private class PressureTraceOverlayConfig : OverlayConfiguration { }

        internal static PressureTraceOverlay Instance;
        private TyrePressureDataCollector _dataCollector;

        public PressureTraceOverlay(Rectangle rectangle) : base(rectangle, "Pressure Trace Overlay")
        {
            this.Width = 140;
            this.Height = 60 * 2;
            this.RequestsDrawItself = true;
        }

        public sealed override void BeforeStart()
        {
            Instance = this;

            TyrePressureGraph.PressureRange = TyrePressures.GetCurrentRange(pageGraphics.TyreCompound, pageStatic.CarModel);
            _dataCollector = new TyrePressureDataCollector() { TraceCount = this.Width / 2 - 1 };
            _dataCollector.Start();
        }

        public sealed override void BeforeStop()
        {
            _dataCollector.Stop();
            Instance = null;
        }

        public sealed override void Render(Graphics g)
        {
            TyrePressureGraph.PressureRange = TyrePressures.GetCurrentRange(pageGraphics.TyreCompound, pageStatic.CarModel);
            TyrePressureGraph graph = new TyrePressureGraph(0, 0, this.Width / 2 - 1, (this.Height / 2) - 1, _dataCollector.FrontLeft);
            TyrePressureGraph graph1 = new TyrePressureGraph(this.Width / 2, 0, this.Width / 2 - 1, (this.Height / 2) - 1, _dataCollector.FrontRight);
            TyrePressureGraph graph2 = new TyrePressureGraph(0, (this.Height / 2) * 1, this.Width / 2 - 1, (this.Height / 2) - 1, _dataCollector.RearLeft);
            TyrePressureGraph graph3 = new TyrePressureGraph(this.Width / 2, (this.Height / 2) * 1, this.Width / 2 - 1, (this.Height / 2) - 1, _dataCollector.RearRight);

            graph.Draw(g);
            graph1.Draw(g);
            graph2.Draw(g);
            graph3.Draw(g);
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
