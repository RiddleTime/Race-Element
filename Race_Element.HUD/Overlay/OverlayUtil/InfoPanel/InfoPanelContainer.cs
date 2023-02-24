using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Overlay.OverlayUtil.InfoPanel
{
    public sealed class InfoPanelContainer
    {
        public List<PanelText> HeaderPanels = new List<PanelText>();
        public List<PanelText> ValuePanels = new List<PanelText>();

        public InfoPanelContainer()
        {

        }

        public void AddItem(PanelText header, PanelText value)
        {
            HeaderPanels.Add(header);
            ValuePanels.Add(value);
        }

        public void Dispose()
        {
            foreach (PanelText header in HeaderPanels) { header.Dispose(); }
            foreach (PanelText value in ValuePanels) { value.Dispose(); }
        }
    }
}
