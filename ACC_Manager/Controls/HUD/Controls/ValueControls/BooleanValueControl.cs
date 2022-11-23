using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static ACCManager.HUD.Overlay.Configuration.OverlayConfiguration;

namespace ACCManager.Controls.HUD.Controls.ValueControls
{
    internal class BooleanValueControl : IValueControl<bool>, IControl
    {
        private readonly Grid _grid;
        private readonly Label _label;
        private readonly ToggleButton _toggleButton;

        public FrameworkElement Control => _grid;
        public bool Value { get; set; }

        public BooleanValueControl(ConfigField configField)
        {
            _grid = new Grid() { Width = 200, Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)), Cursor = Cursors.Hand };
            _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8, GridUnitType.Star) });

            _label = new Label()
            {
                HorizontalContentAlignment = HorizontalAlignment.Right
            };
            _grid.Children.Add(_label);
            Grid.SetColumn(_label, 0);

            _toggleButton = new ToggleButton();
            _toggleButton.Checked += (s, e) => UpdateLabel();
            _toggleButton.Unchecked += (s, e) => UpdateLabel();
            _toggleButton.IsChecked = bool.Parse(configField.Value.ToString());
            _grid.Children.Add(_toggleButton);
            Grid.SetColumn(_toggleButton, 1);

            _grid.MouseLeftButtonUp += (s, e) => { _toggleButton.IsChecked = !_toggleButton.IsChecked; };
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            _label.Content = _toggleButton.IsChecked.Value ? "On" : "Off";
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
