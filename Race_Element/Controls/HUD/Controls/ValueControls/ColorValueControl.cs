using MaterialDesignThemes.Wpf;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;

namespace RaceElement.Controls.HUD.Controls.ValueControls
{
    internal class ColorValueControl : IValueControl<System.Drawing.Color>, IControl
    {
        System.Drawing.Color IValueControl<System.Drawing.Color>.Value { get; set; }
        public FrameworkElement Control => _grid;

        private readonly ConfigField _field;

        private readonly Grid _grid;
        private readonly Label _label;

        public ColorValueControl(ConfigField configField)
        {
            _field = configField;
            _grid = new Grid()
            {
                Width = 220,
                Height = 66,
                Margin = new Thickness(0, 1, 7, 1),
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(140, 2, 2, 2)),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
            };


            // add color slider

            ColorPicker colorPicker = new ColorPicker()
            {
                HueSliderPosition = Dock.Bottom,
                Margin = new Thickness(0, -5, 0, -5),
            };

            string originalValue = configField.Value.ToString();
            Debug.WriteLine(originalValue);
            System.Drawing.Color loadedColor = FromToString(configField.Value.ToString());
            colorPicker.Color = System.Windows.Media.Color.FromArgb(loadedColor.A, loadedColor.R, loadedColor.G, loadedColor.B);
            colorPicker.PreviewMouseLeftButtonUp += (s, e) =>
            {
                Debug.WriteLine($"new Color: {colorPicker.Color.ToString()}");

                var drawingColor = System.Drawing.Color.FromArgb(colorPicker.Color.A, colorPicker.Color.R, colorPicker.Color.G, colorPicker.Color.B);
                _field.Value = drawingColor.ToString();

                Debug.WriteLine($"new field value: {_field.Value}");
                Save();
            };

            _grid.Children.Add(colorPicker);
        }

        private System.Drawing.Color FromToString(string value)
        {
            // format to convert from (System.Drawing.Color.ToString())
            // Color [A=135, R=5, G=255, B=5]

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
            catch (Exception) { return System.Drawing.Color.Red; }
        }

        public void Save()
        {
            ConfigurationControls.SaveOverlayConfigField(_field);
        }
    }
}
