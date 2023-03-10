using RaceElement.HUD.Overlay.Internal;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlaySpotter
{
#if DEBUG
    [Overlay(Name = "Spotter",
        Description = "TODO (spots things?)",
        Version = 1.00,
        OverlayType = OverlayType.Release,
        OverlayCategory = OverlayCategory.Driving
    )]
#endif
    internal sealed class SpotterOverlay : AbstractOverlay
    {
        public SpotterOverlay(Rectangle rectangle) : base(rectangle, "Spotter")
        {
        }

        public override void Render(Graphics g)
        {
        }
    }
}
