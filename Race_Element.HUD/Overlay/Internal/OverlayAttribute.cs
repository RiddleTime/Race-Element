using System;

namespace RaceElement.HUD.Overlay.Internal
{
    public class OverlayAttribute : Attribute
    {
        public string Name { get; set; }
        public OverlayType OverlayType { get; set; }
        public OverlayCategory OverlayCategory { get; set; } = OverlayCategory.All;

        public double Version { get; set; }
        public string Description { get; set; }

    }

    public enum OverlayType
    {
        Release,
        Debug,
    }

    public enum OverlayCategory
    {
        All,
        Inputs,
        Lap,
        Physics,
        Track,
        Weather,
    }
}
