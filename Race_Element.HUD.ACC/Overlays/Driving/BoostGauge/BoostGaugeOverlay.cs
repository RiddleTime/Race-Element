using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System.Drawing;

namespace ACCManager.HUD.ACC.Overlays.OverlayBoostGauge;

[Overlay(Name = "Boost Gauge", Version = 1.00, OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Driving,
  Description = "Progress bar showing boost percentage.",
Authors = ["Reinier Klarenberg"])]
internal sealed class BoostGaugeOverlay : AbstractOverlay
{
    private readonly BoostConfiguration _config = new();
    private sealed class BoostConfiguration : OverlayConfiguration
    {
        public BoostConfiguration() => GenericConfiguration.AllowRescale = true;

        [ConfigGrouping("Colors", "Change the appearance of the HUD.")]
        public ColorGrouping Colors { get; init; } = new ColorGrouping();
        public sealed class ColorGrouping
        {
            [ToolTip("Sets the color of the bar")]
            public Color BarColor { get; init; } = Color.FromArgb(255, 255, 69, 0);
        }
    }

    private readonly InfoPanel _panel;

    public BoostGaugeOverlay(Rectangle rectangle) : base(rectangle, "Boost Gauge")
    {
        this.Width = 130;
        _panel = new InfoPanel(13, this.Width)
        {
            FirstRowLine = 1,
        };
        this.Height = _panel.FontHeight * 1 + 1;
        this.RefreshRateHz = 10;
    }

    public override void BeforeStart() { }
    public override void BeforeStop() { }

    public sealed override void Render(Graphics g)
    {
        using SolidBrush solidBrush = new(_config.Colors.BarColor);
        _panel.AddProgressBarWithCenteredText($"{pagePhysics.TurboBoost * 100f:F1}".FillStart(4, ' '), 0, 100, pagePhysics.TurboBoost * 100f, solidBrush);
        _panel.Draw(g);
    }
}
