using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Driving.OverlayTesting
{
    [Overlay(Name = "Testing", Description = "some testing ", OverlayType = OverlayType.Drive)]
    internal class TestingOverlay : AbstractOverlay
    {
        private readonly AbstractLoopJob _job;
        private readonly InfoPanel _panel;

        public TestingOverlay(Rectangle rectangle) : base(rectangle, "Testing")
        {
            Width = 500;
            Height = 150;
            RefreshRateHz = 50;

            _panel = new InfoPanel(10, 500);
            _job = new SimpleLoopJob() { Action = () => SimDataProvider.Update(), IntervalMillis = 1000 / 50 };
        }

        public override void BeforeStart()
        {
            _job.Run();
        }

        public override void BeforeStop()
        {
            _job.CancelJoin();
        }

        public override bool ShouldRender() => true;// SimDataProvider.LocalCar.Engine.RPM > 0;
        public override void Render(Graphics g)
        {
            var localCar = SimDataProvider.LocalCar;
            _panel.AddLine($"Speed", $"{localCar.Physics.Velocity:F2} km/h");
            _panel.AddLine($"#", $"{localCar.Race.CarNumber}");
            _panel.AddLine("P", $"{localCar.Race.GlobalPosition}");

            var session = SimDataProvider.Session;
            _panel.AddLine($"Temps", $"Air: {session.Weather.AirTemperature:F1} °C, Track: {session.Track.Temperature} °C{(session.Weather.AirPressure > 0 ? $", Pressure: {session.Weather.AirPressure}" : "")}");
            _panel.AddLine($"Track", $"{session.Track.GameName}");
            //_panel.AddLine($"Car", $"{localCar.CarModel.GameName}");
            _panel.AddLine("RPM", $"{localCar.Engine.Rpm}/{localCar.Engine.MaxRpm}");

            //_panel.AddLine($"WindHeading", $"{session.Weather.AirDirection}");

            _panel.Draw(g);

            //Debug.WriteLine(JsonConvert.SerializeObject(SimDataProvider.GameData));
        }
    }
}
