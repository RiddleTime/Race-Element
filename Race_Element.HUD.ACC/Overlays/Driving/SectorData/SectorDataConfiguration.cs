using RaceElement.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.SectorData
{
    internal sealed class SectorDataConfiguration : OverlayConfiguration
    {
        public SectorDataConfiguration() => GenericConfiguration.AllowRescale = true;
    }
}
