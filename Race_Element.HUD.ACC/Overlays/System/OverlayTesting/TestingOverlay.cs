using Newtonsoft.Json;

using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.HUD.Overlay.Internal;

using System.Diagnostics;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.System.OverlayTesting
{
    [Overlay(Name = "Testing", Description = "some testing ")]
    internal class TestingOverlay : AbstractOverlay
    {
        public TestingOverlay(Rectangle rectangle) : base(rectangle, "Testing")
        {
            Width = 1;
            Height = 1;
        }

        public override bool ShouldRender() => true;
        public override void Render(Graphics g)
        {
            SimulatorDataProvider.Update();

            var session = SimulatorDataProvider.Session;
            var localCar = SimulatorDataProvider.LocalCar;

            Debug.WriteLine(JsonConvert.SerializeObject(session, Formatting.Indented));
            Debug.WriteLine(JsonConvert.SerializeObject(localCar));
        }
    }
}
