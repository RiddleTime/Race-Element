using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using System.Diagnostics;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Pitwall.DualSenseInternal;

#if DEBUG
[Overlay(Name = "DSI",
    Description = "Adds active triggers for the DualSense Controller.",
    OverlayCategory = OverlayCategory.Inputs,
    OverlayType = OverlayType.Drive,
    Game = Game.RaceRoom | Game.AssettoCorsa1 | Game.AssettoCorsaEvo,
    Authors = ["Reinier Klarenberg"]
)]
#endif
internal sealed class DsiOverlay : CommonAbstractOverlay
{
    internal readonly DsiConfiguration _config = new();
    private DsiJob _dsiJob;

    public DsiOverlay(Rectangle rectangle) : base(rectangle, "DSI")
    {
        Width = 1; Height = 1;
        RefreshRateHz = 1;
        AllowReposition = false;
    }

    public sealed override void BeforeStart()
    {
        if (IsPreviewing) return;

        _dsiJob = new DsiJob(this) { IntervalMillis = 1000 / 200 };
        _dsiJob.Run();
    }
    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        _dsiJob?.CancelJoin();
    }

    public sealed override bool ShouldRender() => true;// DefaultShouldRender() && !IsPreviewing;

    public sealed override void Render(Graphics g) { }
}
