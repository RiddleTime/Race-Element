using System;

namespace RaceElement.HUD.Overlay.Configuration;

[AttributeUsage(AttributeTargets.Property)]
public sealed class LinkTextAttribute(string linkText) : Attribute
{
    public string LinkText { get; init; } = linkText;
}
