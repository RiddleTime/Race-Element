using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private readonly string[] _names;

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

            _names = Enum.GetNames(type);
            _comboBox = new ComboBox()
            {
                SelectedIndex = selectedIndex,
                Padding = new Thickness(6, 6, 0, 6),
                FontWeight = FontWeights.Bold,
            };
            _comboBox.SelectionChanged += (s, e) =>
            {
                Save();
            };

            ComboBoxItem[] items = new ComboBoxItem[_names.Length];
            try
            {
                for (int i = 0; i < _names.Length; i++)
                {
                    if (_names[i].Equals(field.Value))
                        selectedIndex = i;


                    items[i] = new ComboBoxItem()
                    {
                        Content = string.Concat(_names[i].Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '),
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Stretch,
                    };

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            _comboBox.ItemsSource = items;
            _comboBox.SelectedIndex = selectedIndex;

            _grid.Children.Add(_comboBox);
            _comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            _comboBox.HorizontalContentAlignment = HorizontalAlignment.Left;
        }

        public void Save()
        {
            _field.Value = _names[_comboBox.SelectedIndex];
            ConfigurationControls.SaveOverlayConfigField(_field);
        }
    }
}
