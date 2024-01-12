using System;

namespace RaceElement.HUD.Overlay.Configuration
{
    public sealed class StringOptionsAttribute : Attribute
    {
        public bool IsPassword { get; private set; }
        public StringOptionsAttribute(bool isPassword = false)
        {
            IsPassword = isPassword;
        }
    }
}
