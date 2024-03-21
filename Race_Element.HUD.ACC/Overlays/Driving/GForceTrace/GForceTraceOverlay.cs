using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RaceElement.HUD.ACC.Overlays.Driving.GForceTrace;

[Overlay(Name = "G-Force Trace",
Description = "A graph that shows you both lateral and longitudinal forces over time.")]
internal sealed class GForceTraceOverlay : AbstractOverlay
{
    private sealed class GForceTraceConfiguration : OverlayConfiguration
    {
        public GForceTraceConfiguration() => GenericConfiguration.AllowRescale = true;
    }

    public GForceTraceOverlay(Rectangle rectangle) : base(rectangle, "G-Force Trace")
    {
    }

    public override void Render(Graphics g)
    {
        throw new NotImplementedException();
    }
}
