using System;

namespace RaceElement.HUD.Overlay.Internal
{
    public class OverlayAttribute : Attribute
    {
        public string Name { get; set; }
        public OverlayType OverlayType { get; set; }
        public OverlayCategory OverlayCategory { get; set; } = OverlayCategory.All;

        public double Version { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public enum OverlayType
    {
        Release,
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
}
