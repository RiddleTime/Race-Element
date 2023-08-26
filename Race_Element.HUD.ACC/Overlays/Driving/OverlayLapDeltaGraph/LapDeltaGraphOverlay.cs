using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayLapDeltaGraph
{
    [Overlay(Name = "Lap Delta Graph", Description = "Shows a trace line of the delta over time.", OverlayType = OverlayType.Release, Version = 0.1)]
    internal class LapDeltaGraphOverlay : AbstractOverlay
    {
        public LapDeltaGraphOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta Graph")
        {
        }

        public override void Render(Graphics g)
        {
            
        }
    }
}
