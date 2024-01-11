using System.Windows;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace RaceElement.Controls.HUD.Controls.ValueControls
{
    internal class StringValueControl : IValueControl<string>, IControl
    {
        string IValueControl<string>.Value { get; set; }
        FrameworkElement IControl.Control => _grid;

        private readonly Grid _grid;
        private readonly ConfigField _field;
        private readonly TextBox _textBox;

        public StringValueControl(ConfigField configField)
        {
            _field = configField;
            _grid = new Grid()
            {
                Width = 290,
                Margin = new Thickness(0, 0, 7, 0),
                Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
                Cursor = Cursors.Hand
            };

            // add text box
            _textBox = new TextBox()
            {
                Text = _field.Value.ToString(),
                Width = _grid.Width,
            };
            _textBox.TextChanged += TextBox_TextChanged;
            _grid.Children.Add(_textBox);
            Grid.SetColumn(_textBox, 0);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _field.Value = _textBox.Text;
            Save();
        }

        public void Save()
        {
            ConfigurationControls.SaveOverlayConfigField(_field);
        }
    }
}
