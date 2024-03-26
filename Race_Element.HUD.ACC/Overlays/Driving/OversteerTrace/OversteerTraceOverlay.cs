using RaceElement.HUD.Overlay.Internal;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.OversteerTrace;

[Overlay(Name = "Oversteer Trace",
Description = "Live graph of oversteer in red and understeer in blue.",
OverlayType = OverlayType.Drive,
OverlayCategory = OverlayCategory.Physics,
Version = 1.00,
Authors = ["Reinier Klarenberg"])]
internal sealed class OversteerTraceOverlay : AbstractOverlay
{
    internal readonly OversteerTraceConfiguration _config = new();

    private OversteerDataJob _dataJob;
    private OversteerGraph _graph;

    public OversteerTraceOverlay(Rectangle rectangle) : base(rectangle, "Oversteer Trace")
    {
        this.Width = _config.Chart.Width;
        this.Height = _config.Chart.Height;
        this.RefreshRateHz = _config.Chart.HudRefreshRate;
    }

    public sealed override void BeforeStart()
    {
        _dataJob = new(this, _config.Chart.Width - 1, _config.Data.MaxSlipAngle) { IntervalMillis = 1000 / _config.Data.Herz };
        _graph = new OversteerGraph(0, 0, _config.Chart.Width - 1, _config.Chart.Height - 1, _dataJob, _config);

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
