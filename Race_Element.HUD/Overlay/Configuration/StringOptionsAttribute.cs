using System;

namespace RaceElement.HUD.Overlay.Configuration;

[AttributeUsage(AttributeTargets.Property)]
public sealed class StringOptionsAttribute : Attribute
{
    public bool IsPassword { get; private set; }
    public StringOptionsAttribute(bool isPassword = false)
    {
        IsPassword = isPassword;
    }
}
