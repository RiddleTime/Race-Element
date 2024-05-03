using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.Speedometer;

[Overlay(Name = "Speedometer",
Description = "A simple piece of text displaying the speed",
Authors = ["Reinier Klarenberg"])]
internal sealed class SpeedometerOverlay : AbstractOverlay
{
    private readonly SpeedometerConfiguration _config = new();
    private sealed class SpeedometerConfiguration : OverlayConfiguration
    {
        public SpeedometerConfiguration() => GenericConfiguration.AllowRescale = true;

        [ConfigGrouping("Info Panel", "Show or hide additional information in the panel.")]
        public InfoPanelGrouping InfoPanel { get; init; } = new InfoPanelGrouping();
        public sealed class InfoPanelGrouping
        {
            [ToolTip("Displays the maximum speed reached on each lap.")]
            public bool MaxSpeed { get; init; } = false;

            [ToolTip("Displays the minimum speed reached on each lap.")]
            public bool MinSpeed { get; init; } = false;
        }

        [ConfigGrouping("Colors", "Change the appearance of the HUD.")]
        public ColorGrouping Colors { get; init; } = new ColorGrouping();
        public sealed class ColorGrouping
        {
            [ToolTip("Sets the color of the bar")]
            public Color BarColor { get; init; } = Color.FromArgb(255, 255, 69, 0);
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
        using SolidBrush solidBrush = new(_config.Colors.BarColor);
        _panel.AddProgressBarWithCenteredText($"{pagePhysics.SpeedKmh:F0}".FillStart(3, ' '), 0, 320, pagePhysics.SpeedKmh, solidBrush);

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
}
