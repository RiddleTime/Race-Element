using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.System.OverlayTesting
{
    [Overlay(Name = "Testing", Description = "some testing ")]
    internal class TestingOverlay : AbstractOverlay
    {
        private readonly AbstractLoopJob _job;
        private readonly InfoPanel _panel;

        public TestingOverlay(Rectangle rectangle) : base(rectangle, "Testing")
        {
            Width = 500;
            Height = 100;
            RefreshRateHz = 30;
            SubscribeToACCData = false;

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

        public override bool ShouldRender() => SimDataProvider.LocalCar.Engine.IsRunning;
        public override void Render(Graphics g)
        {
            var localCar = SimDataProvider.LocalCar;
            _panel.AddLine($"Speed", $"{localCar.Physics.Velocity:F2}");
            _panel.AddLine($"Rotation", $"{localCar.Physics.Rotation}");

            var session = SimDataProvider.Session;
            _panel.AddLine($"Track T", $"{session.Track.TrackTemperature:F1}");
            _panel.Draw(g);
        }
    }
}
