using RaceElement.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.PitstopHelper
{
    internal sealed class PitstopHelperConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Tyre Pressures", "Adjust tyre pressure targets to be used in the hud.")]
        public TyrePressures Pressures { get; init; } = new();
        public class TyrePressures
        {
            [ToolTip("TODO")]
            [FloatRange(26f, 27.3f, 0.1f, 1)]
            public float DryTyreTarget { get; init; } = 26.9f;

            [ToolTip("TODO")]
            [FloatRange(29f, 35f, 0.1f, 1)]
            public float WetTyreTarget { get; init; } = 30f;
        }
    }
}
