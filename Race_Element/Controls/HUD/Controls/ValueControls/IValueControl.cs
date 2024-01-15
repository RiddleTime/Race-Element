using System.Windows;

namespace RaceElement.Controls.HUD.Controls.ValueControls;

internal interface IControl
{
    FrameworkElement Control { get; }
}

internal interface IValueControl<T> : IControl
{
    T Value { get; set; }

    void Save();
}
