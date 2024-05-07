using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.PressureHistory;

[Overlay(Name = "Pressure History",
Description = "TODO",
OverlayCategory = OverlayCategory.Physics,
OverlayType = OverlayType.Drive,
Authors = ["Reinier Klarenberg"],
Version = 1.0)]
internal sealed class PressureHistoryOverlay : AbstractOverlay
{
    private readonly PressureHistoryConfiguration _config = new();
    private sealed class PressureHistoryConfiguration : OverlayConfiguration
    {

    }

    private PressureHistoryJob _historyJob;

    public PressureHistoryOverlay(Rectangle rectangle) : base(rectangle, "Pressure History")
    {
    }

    public sealed override void BeforeStart()
    {
        if (IsPreviewing) return;

        _historyJob = new(this) { IntervalMillis = 100 };
        _historyJob.Run();
    }

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        _historyJob?.CancelJoin();
    }

    public sealed override void Render(Graphics g)
    {
    }
}
