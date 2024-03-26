using RaceElement.HUD.Overlay.Internal;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.GForceTrace;

[Overlay(Name = "G-Force Trace",
Description = "Live graph of lateral and longitudinal G-forces.\nLateral forces are shown in Yellow, Longitudinal forces in light grey.",
OverlayCategory = OverlayCategory.Physics,
OverlayType = OverlayType.Drive,
Version = 1.00,
Authors = ["Reinier Klarenberg"])]
internal sealed class GForceTraceOverlay : AbstractOverlay
{
    internal readonly GForceTraceConfiguration _config = new();
    private GForceDataJob _dataJob;
    private GForceGraph _graph;

    public GForceTraceOverlay(Rectangle rectangle) : base(rectangle, "G-Force Trace")
    {
        RefreshRateHz = 30;
        this.Width = _config.Chart.Width;
        this.Height = _config.Chart.Height;
    }

    public sealed override void BeforeStart()
    {
        _dataJob = new GForceDataJob(this, _config.Chart.Width - 1) { IntervalMillis = 1000 / _config.Chart.Herz };
        _graph = new GForceGraph(0, 0, _config.Chart.Width - 1, _config.Chart.Height - 1, _dataJob, _config);

        if (!IsPreviewing)
            _dataJob.Run();
    }
    public sealed override void BeforeStop()
    {
        _graph?.Dispose();

        if (!IsPreviewing)
            _dataJob?.CancelJoin();
    }

    public sealed override void Render(Graphics g) => _graph?.Draw(g);
}
