using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml.Linq;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;

namespace RaceElement.Controls.HUD.Controls.ValueControls
{
    internal class EnumValueControl : IValueControl<Enum>
    {
        Enum IValueControl<Enum>.Value { get; set; }

        private readonly ConfigField _field;
        FrameworkElement IControl.Control => _grid;
        private readonly Grid _grid;

        private readonly ComboBox _comboBox;

        public EnumValueControl(ConfigField field, Type type)
        {
            _field = field;
            _grid = new Grid()
            {
                Width = 250,
                Margin = new Thickness(0, 1, 7, 1),
                Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
            };

            string[] names = Enum.GetNames(type);
            int selectedIndex = 0;
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Equals(field.Value))
                    selectedIndex = i;
            }

            Debug.WriteLine($"Configured fieldValue: {field.Value}");
            _comboBox = new ComboBox()
            {
                ItemsSource = names,
                SelectedIndex = selectedIndex,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(6, 6, 0, 6),
                FontWeight = FontWeights.Bold,
            };
            _comboBox.SelectionChanged += (s, e) =>
            {
                Debug.WriteLine($"Saving: {names[_comboBox.SelectedIndex]}");
                _field.Value = names[_comboBox.SelectedIndex];
                Debug.WriteLine($"Saved: {_field.Value}");
                Save();
            };

            _grid.Children.Add(_comboBox);
        }

        public void Save()
        {
            Debug.WriteLine($"Saving from Value Control: {_field.Value}");

            ConfigurationControls.SaveOverlayConfigField(_field);
        }
    }
}
