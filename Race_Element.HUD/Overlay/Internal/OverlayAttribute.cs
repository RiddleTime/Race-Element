using System;

namespace RaceElement.HUD.Overlay.Internal;

[AttributeUsage(AttributeTargets.Class)]
public class OverlayAttribute : Attribute
{
    public required string Name { get; set; }
    public OverlayType OverlayType { get; set; }
    public OverlayCategory OverlayCategory { get; set; } = OverlayCategory.All;

    public double Version { get; set; }
    public string Description { get; set; } = string.Empty;
}

public enum OverlayType
{
    Drive,
    Pitwall,
}

public enum OverlayCategory
{
    All,
    Car,
    Driving,
    Inputs,
    Lap,
    Physics,
    Track,
}
