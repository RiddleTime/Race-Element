using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;

namespace RaceElement.Controls.HUD.Controls.ValueControls;

internal sealed class StringValueControl : IValueControl<string>, IControl
{
    string IValueControl<string>.Value { get; set; }
    FrameworkElement IControl.Control => _grid;

    private readonly Grid _grid;
    private readonly ConfigField _field;
    private readonly TextBox _textBox;
    private readonly PasswordBox _passwordBox;

    public StringValueControl(ConfigField configField, bool isPassword = false)
    {
        _field = configField;
        _grid = new Grid()
        {
            Width = ControlConstants.ControlWidth,
            Margin = new Thickness(0, 0, 7, 0),
            Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
            Cursor = Cursors.Hand
        };

        // add text box or password box

        if (isPassword)
        {
            _passwordBox = new PasswordBox()
            {
                Password = _field.Value.ToString(),
                Width = _grid.Width,
            };

            _passwordBox.PasswordChanged += (s, e) =>
            {
                _field.Value = _passwordBox.Password;
                Save();
            };
            _grid.Children.Add(_passwordBox);
            Grid.SetColumn(_passwordBox, 0);
        }
        else
        {
            _textBox = new TextBox()
            {
                Text = _field.Value.ToString(),
                Width = _grid.Width,
            };

            _textBox.TextChanged += (s, e) =>
            {
                _field.Value = _textBox.Text;
                Save();
            };
            _grid.Children.Add(_textBox);
            Grid.SetColumn(_textBox, 0);
        }
    }

    public void Save()
    {
        ConfigurationControls.SaveOverlayConfigField(_field);
    }
}
