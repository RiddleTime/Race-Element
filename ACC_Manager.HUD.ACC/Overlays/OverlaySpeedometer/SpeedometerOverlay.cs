using ACC_Manager.Util.SystemExtensions;
using ACCManager.Data.ACC.Database.LapDataDB;
using ACCManager.Data.ACC.Tracker.Laps;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Drawing;

namespace ACCManager.HUD.ACC.Overlays.OverlaySpeedometer
{
    [Overlay(Name = "Speedometer", Description = "Shows a speedometer", Version = 1.00, OverlayType = OverlayType.Release)]
    internal sealed class SpeedometerOverlay : AbstractOverlay
    {
        private readonly SpeedometerConfiguration _config = new SpeedometerConfiguration();
        private class SpeedometerConfiguration : OverlayConfiguration
        {
            [ToolTip("Displays the maximum speed reached on each lap.")]
            public bool ShowMaxSpeed { get; set; } = false;

            [ToolTip("Displays the minimum speed reached on each lap.")]
            public bool ShowMinSpeed { get; set; } = false;

            public SpeedometerConfiguration()
            {
                AllowRescale = true;
            }
        }

        private readonly InfoPanel _panel;
        private float _maxSpeed = 0;
        private float _minSpeed = 0;

        public SpeedometerOverlay(Rectangle rectangle) : base(rectangle, "Speedometer Overlay")
        {
            this.Width = 130;
            _panel = new InfoPanel(13, this.Width)
            {
                FirstRowLine = 1
            };
            this.Height = _panel.FontHeight * 3 + 1;
        }

        public sealed override void BeforeStart()
        {

            if (_config.ShowMaxSpeed || _config.ShowMinSpeed)
                LapTracker.Instance.LapFinished += OnLapFinished;

            if (!_config.ShowMinSpeed)
                this.Height -= _panel.FontHeight;
            if (!_config.ShowMaxSpeed)
                this.Height -= _panel.FontHeight;
        }

        public sealed override void BeforeStop()
        {
            if (_config.ShowMinSpeed || _config.ShowMaxSpeed)
                LapTracker.Instance.LapFinished -= OnLapFinished;
        }

        private void OnLapFinished(object sender, DbLapData e)
        {
            _maxSpeed.ClipMax(pagePhysics.SpeedKmh);
            _minSpeed.ClipMin(pagePhysics.SpeedKmh);
        }

        public sealed override void Render(Graphics g)
        {
            _panel.AddProgressBarWithCenteredText($"{pagePhysics.SpeedKmh:F0}".FillStart(3, ' '), 0, 320, pagePhysics.SpeedKmh);

            if (_config.ShowMaxSpeed)
            {
                _maxSpeed.ClipMin(pagePhysics.SpeedKmh);
                _panel.AddLine("Max", $"{_maxSpeed:F2}");
            }

            if (_config.ShowMinSpeed)
            {
                _minSpeed.ClipMax(pagePhysics.SpeedKmh);
                _panel.AddLine("Min", $"{_minSpeed:F2}");
            }

            _panel.Draw(g);
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
