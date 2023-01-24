using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;

namespace RaceElement.Controls.HUD.Controls.ValueControls
{
    internal class ColorValueControl : IValueControl<System.Drawing.Color>
    {
        System.Drawing.Color IValueControl<System.Drawing.Color>.Value { get; set; }
        public FrameworkElement Control => _grid;

        private readonly ConfigField _field;
        private readonly Grid _grid;

        public ColorValueControl(ConfigField configField)
        {
            _field = configField;
            _grid = new Grid()
            {
                Width = 250,
                Height = 66,
                Margin = new Thickness(0, 1, 7, 1),
                Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
            };

            // add color picker
            ColorPicker colorPicker = new ColorPicker()
            {
                HueSliderPosition = Dock.Bottom,
                Margin = new Thickness(0, 0, 0, -7),
            };
            System.Drawing.Color loadedColor = FromToString(configField.Value.ToString());
            colorPicker.Color = Color.FromArgb(loadedColor.A, loadedColor.R, loadedColor.G, loadedColor.B);
            colorPicker.PreviewMouseLeftButtonUp += (s, e) =>
            {
                var drawingColor = System.Drawing.Color.FromArgb(colorPicker.Color.A, colorPicker.Color.R, colorPicker.Color.G, colorPicker.Color.B);
                _field.Value = drawingColor.ToString();

                Save();
            };
            _grid.Children.Add(colorPicker);
        }

        private System.Drawing.Color FromToString(string value)
        {
            if (value.Contains("#"))
            {
                value = value.Replace("Color [", "");
                value = value.Replace("]", "");
                return (System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromString(value);
            }

            try
            {
                int a = int.Parse(value.Split('A')[1].Split(',')[0].Replace("=", ""));
                int r = int.Parse(value.Split('R')[1].Split(',')[0].Replace("=", ""));
                int g = int.Parse(value.Split('G')[1].Split(',')[0].Replace("=", ""));
                int b = int.Parse(value.Split('B')[1].Split(']')[0].Replace("=", ""));
                return System.Drawing.Color.FromArgb(a, r, g, b);
            }
            catch (Exception)
            {
                try
                {
                    string[] split = value.Split(',');
                    int r = int.Parse(split[0]);
                    int g = int.Parse(split[1]);
                    int b = int.Parse(split[2]);
                    return System.Drawing.Color.FromArgb(255, r, g, b);
                }
                catch (Exception)
                {
                    return System.Drawing.Color.Red;
                }
            }
        }

        public void Save()
        {
            ConfigurationControls.SaveOverlayConfigField(_field);
        }
    }
}
