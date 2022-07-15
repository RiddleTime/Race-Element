using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.Overlay.Internal
{
    public class OverlayAttribute : Attribute
    {
        public string Name { get; set; }
        public double Version { get; set; }
        public string Description { get; set; }
        public OverlayType OverlayType { get; set; }
    }

    public enum OverlayType
    {
        Release,
        Debug,
        Beta
    }
}
