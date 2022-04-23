using ACCSetupApp.Controls.HUD.Overlay.Internal;
using ACCSetupApp.SetupParser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.ACCSharedMemory;
using static ACCSetupApp.SetupParser.ConversionFactory;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayEcuMapInfo
{
    internal class EcuMapOverlay : AbstractOverlay
    {
        private Font inputFont = new Font("Roboto", 10);
        public EcuMapOverlay(Rectangle rectangle) : base(rectangle)
        {
            this.Width = 700;
        }

        public override void BeforeStart()
        {

        }

        public override void BeforeStop()
        {

        }

        public override bool ShouldRender()
        {
            bool shouldRender = true;
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }

        public override void Render(Graphics g)
        {
            EcuMap current = GetCurrentMap();
            if (current != null)
            {
                g.DrawString($"Engine map: {current.Index}, Power: {current.Power}, Condition: {current.Conditon}, Consumption: {current.FuelConsumption}, Throttle map: {current.ThrottleMap}", inputFont, Brushes.White, 0, 0);
            }
        }

        private EcuMap GetCurrentMap()
        {
            CarModels carModel = ParseNames[pageStatic.CarModel];

            EcuMap[] carMaps = null;
            foreach (KeyValuePair<CarModels[], EcuMap[]> maps in EcuMaps.maps.ToList())
            {
                foreach (CarModels mapModel in maps.Key)
                {
                    if (carModel == mapModel)
                    {
                        carMaps = maps.Value;
                        break;
                    }
                }
                if (carMaps != null) break;
            }

            return carMaps[pageGraphics.EngineMap];
        }
    }
}
