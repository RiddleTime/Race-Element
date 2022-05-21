using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.Overlay.Configuration
{
    public class ToolTipAttribute : Attribute
    {
        public string ToolTip { get; private set; } = string.Empty;
        public ToolTipAttribute(string toolTip)
        {
            this.ToolTip = toolTip;
        }
    }
}
