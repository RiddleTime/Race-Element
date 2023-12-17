using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayTrackBar
{
    [Overlay(Name = "Track Bar",
             Description = "A bar displaying a flat and zoomed in version of the Track Circle HUD.")]
    internal sealed class TrackBarOverlay : AbstractOverlay
    {
        public TrackBarOverlay(Rectangle rectangle) : base(rectangle, "Track Bar")
        {
        }

        public override void Render(Graphics g)
        {
        }
    }
}
