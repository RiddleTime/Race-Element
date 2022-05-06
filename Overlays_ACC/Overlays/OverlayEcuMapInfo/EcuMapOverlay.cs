using ACCSetupApp.Controls.HUD.Overlay.Internal;
using ACCSetupApp.SetupParser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.ACCSharedMemory;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayEcuMapInfo
{
    internal class EcuMapOverlay : AbstractOverlay
    {
        EcuMap current;
        private Font inputFont = new Font("Roboto", 10);
        public EcuMapOverlay(Rectangle rectangle) : base(rectangle, "Ecu Maps Overlay")
        {
            this.AllowReposition = false;

            this.Width = 1;
            this.Height = inputFont.Height + 2;
            this.Y = ScreenHeight - Height - 2;
        }

        public override void BeforeStart()
        {
        }

        public override void BeforeStop() { }

        public override bool ShouldRender()
        {
            bool shouldRender = true;
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }

        public override void Render(Graphics g)
        {
            EcuMap next = EcuMaps.GetMap(pageStatic.CarModel, pageGraphics.EngineMap);

            bool updateWidth = false;
            if (current != next)
            {
                current = next;
                updateWidth = true;
            }

            if (current != null)
            {
                string EcuInfo = $"ECU Map: {current.Index}, Power: {current.Power}, Condition: {current.Conditon}, " +
                                    $"Consumption: {current.FuelConsumption}, Throttle map: {current.ThrottleMap}";

                if (updateWidth)
                {
                    int width = (int)g.MeasureString(EcuInfo, inputFont).Width;

                    this.Width = width;
                    this.X = ScreenWidth - this.Width - 2;
                }

                g.FillRectangle(new SolidBrush(Color.FromArgb(140, 0, 0, 0)), 0, 0, this.Width, this.Height);
                g.DrawString(EcuInfo, inputFont, Brushes.White, 0, 0);
            }
        }

    }
}
