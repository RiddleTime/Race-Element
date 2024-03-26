using RaceElement.HUD.Overlay.Internal;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.OversteerTrace;

[Overlay(Name = "Oversteer Trace", Version = 1.00,
    Description = "Live graph of oversteer in red and understeer in blue.",
    OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Physics,
Authors = ["Reinier Klarenberg"])]
internal sealed class OversteerTraceOverlay : AbstractOverlay
{
    private readonly OversteerTraceConfiguration _config = new();

    private OversteerDataCollector _collector;
    private OversteerGraph _graph;

    public OversteerTraceOverlay(Rectangle rectangle) : base(rectangle, "Oversteer Trace")
    {
        this.Width = _config.Chart.Width;
        this.Height = _config.Chart.Height;
        this.RefreshRateHz = _config.Chart.Herz;
    }

    public sealed override void BeforeStart()
    {
        _collector = new OversteerDataCollector(_config.Chart.Width - 1)
        {
            MaxSlipAngle = _config.Chart.MaxSlipAngle,
            Herz = _config.Chart.Herz
        };
        _graph = new OversteerGraph(0, 0, _config.Chart.Width - 1, _config.Chart.Height - 1, _collector, _config);
    }

    public sealed override void BeforeStop() => _graph?.Dispose();

    public sealed override void Render(Graphics g)
    {
        _collector.Collect(pagePhysics);
        _graph.Draw(g);
    }
}
