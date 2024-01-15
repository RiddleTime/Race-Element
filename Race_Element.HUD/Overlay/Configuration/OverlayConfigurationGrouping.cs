using System;

namespace RaceElement.HUD.Overlay.Configuration;

public class ConfigGroupingAttribute : Attribute
{
    public string Title;
    public string Description;

    public ConfigGroupingAttribute(string title, string description)
    {
        Title = title;
        Description = description;
    }
}
