using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayMapBar;

#if DEBUG
[Overlay(Name = "Map Bar",
    Description = "A circle showing all drivers on track",
    OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Track
    )]
#endif
internal sealed class MapBar : AbstractOverlay
{
    private readonly MapBarConfiguration _config = new();
    private sealed class MapBarConfiguration : OverlayConfiguration
    {
        public MapBarConfiguration() => AllowRescale = true;
    }

    public MapBar(Rectangle rectangle) : base(rectangle, "Map Bar")
    {
        Width = 250;
        Height = 100;
    }

    public override void Render(Graphics g)
    {
        g.FillRectangle(new SolidBrush(Color.FromArgb(170, 0, 0, 0)), new Rectangle(0, 0, (int)(Width / Scale), (int)(Height / Scale)));
    }
}
