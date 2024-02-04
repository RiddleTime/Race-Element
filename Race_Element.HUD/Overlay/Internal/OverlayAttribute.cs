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

    /// <summary>
    /// Initial creator at 0 index.
    /// </summary>
    public string[] Authors { get; set; } = [];
}

/// <summary>
/// Determines the Global Type, Drive overlays are focussed on info when playing, pitwall is for debug and other things.
/// </summary>
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
