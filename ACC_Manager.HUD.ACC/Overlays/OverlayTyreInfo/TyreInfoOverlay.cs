using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.HUD.ACC.Overlays.OverlayTyreInfo
{
    internal class TyreInfoOverlay : AbstractOverlay
    {
        private const int PanelWidth = 120;
        private const double FontSize = 8.5;
        InfoPanel PanelFrontLeft = new InfoPanel(FontSize, PanelWidth) { };
        InfoPanel PanelFrontRight = new InfoPanel(FontSize, PanelWidth) { X = PanelWidth + PanelWidth / 2 };
        InfoPanel PanelRearLeft = new InfoPanel(FontSize, PanelWidth) { Y = 75 };
        InfoPanel PanelRearRight = new InfoPanel(FontSize, PanelWidth) { X = PanelWidth + PanelWidth / 2, Y = 75 };

        public TyreInfoOverlay(Rectangle rectangle) : base(rectangle, "Tyre Info Overlay")
        {
            this.Width = PanelWidth + PanelWidth + PanelWidth / 2;
            this.Height = 300;
        }

        public override void BeforeStart()
        {
        }

        public override void BeforeStop()
        {
        }

        public override void Render(Graphics g)
        {
            InfoPanel[] list = new InfoPanel[] { PanelFrontLeft, PanelFrontRight, PanelRearLeft, PanelRearRight };

            for (int i = 0; i < list.Length; i++)
            {
                list[i].AddLine("PSI", $"{pagePhysics.WheelPressure[i]:F2}");
                list[i].AddLine("Tyre (C)", $"{pagePhysics.TyreTemp[i]:F1}");
                list[i].AddLine("Brake (C)", $"{pagePhysics.BrakeTemperature[i]:F1}");
                list[i].AddProgressBarWithCenteredText("Brake Pressure", 0, 1, pagePhysics.brakePressure[i]);
            }

            for (int i = 0; i < list.Length; i++)
                list[i].Draw(g);
        }

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif
            return false;
        }
    }
}
