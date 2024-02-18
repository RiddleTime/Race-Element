using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayPressureTrace;

[Overlay(Name = "Pressure Trace",
    Version = 1.00,
    OverlayType = OverlayType.Drive,
    Description = "Live graphs of the tyre pressures, green is within range, red is too high, blue is too low.",
    OverlayCategory = OverlayCategory.Physics,
Authors = ["Reinier Klarenberg"])]
internal sealed class PressureTraceOverlay : AbstractOverlay
{
    private readonly PressureTraceOverlayConfig _config = new();
    private sealed class PressureTraceOverlayConfig : OverlayConfiguration
    {
        public PressureTraceOverlayConfig()
        {
            GenericConfiguration.AllowRescale = false;
        }
    }

    internal static PressureTraceOverlay Instance;
    private TyrePressureDataCollector _dataCollector;

    public PressureTraceOverlay(Rectangle rectangle) : base(rectangle, "Pressure Trace")
    {
        this.Width = 140;
        this.Height = 60 * 2;
        this.RequestsDrawItself = true;
    }

    public sealed override void BeforeStart()
    {
        Instance = this;

        _dataCollector = new TyrePressureDataCollector() { TraceCount = this.Width / 2 - 1 };
        _dataCollector.Start();
        RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;
    }

    private void Instance_OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e)
    {
        TyrePressureGraph.PressureRange = TyrePressures.GetCurrentRange(pageGraphics.TyreCompound, pageStatic.CarModel);
    }

    public sealed override void BeforeStop()
    {
        RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
        _dataCollector.Stop();
        Instance = null;
    }

    public sealed override void Render(Graphics g)
    {
        TyrePressureGraph.PressureRange = TyrePressures.GetCurrentRange(pageGraphics.TyreCompound, pageStatic.CarModel);
        TyrePressureGraph graph = new(0, 0, this.Width / 2 - 1, (this.Height / 2) - 1, _dataCollector.FrontLeft);
        TyrePressureGraph graph1 = new(this.Width / 2, 0, this.Width / 2 - 1, (this.Height / 2) - 1, _dataCollector.FrontRight);
        TyrePressureGraph graph2 = new(0, (this.Height / 2) * 1, this.Width / 2 - 1, (this.Height / 2) - 1, _dataCollector.RearLeft);
        TyrePressureGraph graph3 = new(this.Width / 2, (this.Height / 2) * 1, this.Width / 2 - 1, (this.Height / 2) - 1, _dataCollector.RearRight);

        graph.Draw(g);
        graph1.Draw(g);
        graph2.Draw(g);
        graph3.Draw(g);
    }
}
