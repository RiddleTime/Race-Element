using ACC_Manager.Util.SystemExtensions;
using ACCManager.Data.ACC.Database.LapDataDB;
using ACCManager.Data.ACC.Tracker.Laps;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlaySpeedometer
{
#if DEBUG
    [Overlay(Name = "Speedometer", Description = "Shows a speedometer", OverlayType = OverlayType.Release)]
#endif
    internal sealed class SpeedometerOverlay : AbstractOverlay
    {
        private SpeedometerConfiguration _config = new SpeedometerConfiguration();
        private class SpeedometerConfiguration : OverlayConfiguration
        {

            public SpeedometerConfiguration()
            {
                AllowRescale = true;
            }
        }

        private readonly InfoPanel _panel;
        private float _maxSpeed = 0;

        public SpeedometerOverlay(Rectangle rectangle) : base(rectangle, "Speedometer Overlay")
        {
            this.Width = 150;
            _panel = new InfoPanel(13, this.Width);
            this.Height = _panel.FontHeight * 2 + 1;
        }

        public sealed override void BeforeStart()
        {
            LapTracker.Instance.LapFinished += OnLapFinished;
        }

        public sealed override void BeforeStop()
        {
            LapTracker.Instance.LapFinished -= OnLapFinished;
        }

        private void OnLapFinished(object sender, DbLapData e)
        {
            _maxSpeed.ClipMax(pagePhysics.SpeedKmh);
        }

        public sealed override void Render(Graphics g)
        {
            _maxSpeed.ClipMin(pagePhysics.SpeedKmh);

            _panel.AddLine("Max", $"{_maxSpeed:F2}");
            _panel.AddLine("Speed", $"{pagePhysics.SpeedKmh:F2}");

            _panel.Draw(g);
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
