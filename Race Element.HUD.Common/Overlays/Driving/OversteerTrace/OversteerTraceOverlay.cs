using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.Util.SystemExtensions;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Driving.OversteerTrace;

[Overlay(Name = "Oversteer Trace",
Description = "Live graph of oversteer in red and understeer in blue.",
OverlayType = OverlayType.Drive,
OverlayCategory = OverlayCategory.Physics,
Version = 1.00,
Game = Game.AssettoCorsa1 | Game.AssettoCorsaEvo | Game.RaceRoom | Game.ForzaHorizon5 | Game.LeMansUltimate | Game.rFactor2 | Game.WRC_Generations,
Authors = ["Reinier Klarenberg"])]
internal sealed class OversteerTraceOverlay : CommonAbstractOverlay
{
    internal readonly OversteerTraceConfiguration _config = new();

    private OversteerDataJob _dataJob;
    private OversteerGraph _graph;

    public OversteerTraceOverlay(Rectangle rectangle) : base(rectangle, "Oversteer Trace")
    {
        Width = _config.Chart.Width;
        Height = _config.Chart.Height;
        RefreshRateHz = _config.Chart.HudRefreshRate;
        RefreshRateHz.ClipMax(_config.Data.Herz);
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
