using RaceElement.Util.SystemExtensions;
using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlaySpeedometer
{
    [Overlay(Name = "Speedometer", Description = "Shows a speedometer", Version = 1.00, OverlayType = OverlayType.Release)]
    internal sealed class SpeedometerOverlay : AbstractOverlay
    {
        private readonly SpeedometerConfiguration _config = new SpeedometerConfiguration();
        private class SpeedometerConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Info Panel", "Show or hide additional information in the panel.")]
            public InfoPanelGrouping InfoPanel { get; set; } = new InfoPanelGrouping();
            public class InfoPanelGrouping
            {
                [ToolTip("Displays the maximum speed reached on each lap.")]
                public bool MaxSpeed { get; set; } = false;

                [ToolTip("Displays the minimum speed reached on each lap.")]
                public bool MinSpeed { get; set; } = false;
            }

            public SpeedometerConfiguration()
            {
                AllowRescale = true;
            }
        }

        private readonly InfoPanel _panel;
        private float _maxSpeed = 0;
        private float _minSpeed = 0;

        public SpeedometerOverlay(Rectangle rectangle) : base(rectangle, "Speedometer")
        {
            this.Width = 130;
            _panel = new InfoPanel(13, this.Width)
            {
                FirstRowLine = 1
            };
            this.Height = _panel.FontHeight * 3 + 1;
            this.RefreshRateHz = 10;
        }

        public sealed override void BeforeStart()
        {

            if (_config.InfoPanel.MaxSpeed || _config.InfoPanel.MinSpeed)
                LapTracker.Instance.LapFinished += OnLapFinished;

            if (!_config.InfoPanel.MinSpeed)
                this.Height -= _panel.FontHeight;
            if (!_config.InfoPanel.MaxSpeed)
                this.Height -= _panel.FontHeight;
        }

        public sealed override void BeforeStop()
        {
            if (_config.InfoPanel.MinSpeed || _config.InfoPanel.MaxSpeed)
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

            if (_config.InfoPanel.MaxSpeed)
            {
                _maxSpeed.ClipMin(pagePhysics.SpeedKmh);
                _panel.AddLine("Max", $"{_maxSpeed:F2}");
            }

            if (_config.InfoPanel.MinSpeed)
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
