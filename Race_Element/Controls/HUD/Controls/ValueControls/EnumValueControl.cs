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
        FrameworkElement IControl.Control => _grid;
        
        private readonly Grid _grid;
        private readonly ConfigField _field;
        private readonly ComboBox _comboBox;

        public EnumValueControl(ConfigField field, Type type)
        {
            _field = field;
            _grid = new Grid()
            {
                Width = 290,
                Margin = new Thickness(0, 1, 7, 1),
                Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
                Cursor = Cursors.Hand,
            };

            int selectedIndex = 0;

            string[] names = Enum.GetNames(type);
            _comboBox = new ComboBox()
            {
                SelectedIndex = selectedIndex,
                Padding = new Thickness(6, 6, 0, 6),
                FontWeight = FontWeights.Bold,
            };
            _comboBox.SelectionChanged += (s, e) =>
            {
                _field.Value = names[_comboBox.SelectedIndex];
                Save();
            };
            

           
            ComboBoxItem[] items = new ComboBoxItem[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Equals(field.Value))
                    selectedIndex = i;

                items[i] = new ComboBoxItem() { Content = string.Concat(names[i].Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ') };
            }
            _comboBox.ItemsSource = items;

            _grid.Children.Add(_comboBox);
            _comboBox.HorizontalAlignment= HorizontalAlignment.Stretch;
            _comboBox.HorizontalContentAlignment= HorizontalAlignment.Left;
        }

        public void Save() => ConfigurationControls.SaveOverlayConfigField(_field);
    }
}
