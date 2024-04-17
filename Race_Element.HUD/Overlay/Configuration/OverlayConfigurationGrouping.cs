using System;

namespace RaceElement.HUD.Overlay.Configuration;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigGroupingAttribute : Attribute
{
    public string Title;
    public string Description;

    public ConfigGroupingAttribute(string title, string description)
    {
        Title = title;
        Description = description;
    }
}
