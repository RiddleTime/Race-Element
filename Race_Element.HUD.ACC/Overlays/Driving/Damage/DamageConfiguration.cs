using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayDamage;

internal sealed class DamageConfiguration : OverlayConfiguration
{
    public DamageConfiguration() => GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Damage", "Changes the behavior of the Damage HUD")]
    public DamageGrouping Damage { get; init; } = new();
    public sealed class DamageGrouping
    {
        [ToolTip("Only show the HUD when there is actual damage on the car.")]
        public bool AutoHide { get; set; } = true;

        [ToolTip("Displays the total repair time in the center of the HUD with red colored text.")]
        public bool TotalRepairTime { get; init; } = true;
    }

    [ConfigGrouping("Colors", "Change the appearance of the damage colors used in the HUD.")]
    public ColorGrouping Colors { get; init; } = new();
    public sealed class ColorGrouping
    {
        public Color MinorColor { get; init; } = Color.FromArgb(255, 255, 255, 0);
        public Color LightColor { get; init; } = Color.FromArgb(255, 250, 165, 0);
        public Color MediumColor { get; init; } = Color.FromArgb(255, 255, 69, 0);
        public Color MajorColor { get; init; } = Color.FromArgb(255, 255, 0, 0);
    }
}
