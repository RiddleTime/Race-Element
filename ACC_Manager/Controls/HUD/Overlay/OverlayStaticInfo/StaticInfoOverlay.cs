using ACCSetupApp.Controls.HUD.Overlay.Internal;
using ACCSetupApp.Controls.Telemetry.SharedMemory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayStaticInfo
{
    internal class StaticInfoOverlay : AbstractOverlay
    {
        private Font inputFont = new Font("Arial", 16);

        public StaticInfoOverlay(Rectangle rectangle) : base(rectangle)
        {
        }

        public override void BeforeStart()
        {
        }

        public override void BeforeStop()
        {
        }

        public override void Render(Graphics g)
        {
            g.DrawString($"CAR: {pageStatic.CarModel}", inputFont, Brushes.White, 0, 0);
        }

        public override bool ShouldRender()
        {
            return true;
        }
    }
}
