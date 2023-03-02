using MaterialDesignThemes.Wpf;
using System;
using System.Diagnostics;
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

        private readonly ConfigField _field;
        public FrameworkElement Control => _grid;
        private readonly Grid _grid;

        public ColorValueControl(ConfigField configField)
        {
            _field = configField;
            _grid = new Grid()
            {
                Width = 290,
                Height = 66,
                Margin = new Thickness(0, 1, 7, 1),
                Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
            };
            //_grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(235, GridUnitType.Pixel) });
            //_grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(55, GridUnitType.Pixel) });


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
            //Grid.SetColumn(colorPicker, 0);


            //StackPanel rgbStacker = new StackPanel()
            //{
            //    Orientation = Orientation.Vertical,
            //    Margin = new Thickness(5),
            //};

            //int fontSize = 13;
            //rgbStacker.Children.Add(new TextBox() { Text = $"{loadedColor.R}", FontSize = fontSize, Margin = new Thickness(0), Padding = new Thickness(0), });
            //rgbStacker.Children.Add(new TextBox() { Text = $"{loadedColor.G}", FontSize = fontSize, Margin = new Thickness(0), Padding = new Thickness(0), });
            //rgbStacker.Children.Add(new TextBox() { Text = $"{loadedColor.B}", FontSize = fontSize, Margin = new Thickness(0), Padding = new Thickness(0), });

            //_grid.Children.Add(rgbStacker);
            //Grid.SetColumn(rgbStacker, 1);

        }

        private System.Drawing.Color FromToString(string value)
        {
            if (value.Contains("#"))
            {
                value = value.Replace("Color [", "");
                value = value.Replace("]", "");
                return (System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromString(value);
            }

            if (value.Contains("A") && value.Contains("R") && value.Contains("G") && value.Contains("B"))
            {
                try
                {
                    int a = int.Parse(value.Split('A')[1].Split(',')[0].Replace("=", ""));
                    int r = int.Parse(value.Split('R')[1].Split(',')[0].Replace("=", ""));
                    int g = int.Parse(value.Split('G')[1].Split(',')[0].Replace("=", ""));
                    int b = int.Parse(value.Split('B')[1].Split(']')[0].Replace("=", ""));
                    return System.Drawing.Color.FromArgb(a, r, g, b);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                    return System.Drawing.Color.Red;
                }
            }
            else
            {
                try
                {
                    string[] split = value.Split(',');
                    int r = int.Parse(split[0]);
                    int g = int.Parse(split[1]);
                    int b = int.Parse(split[2]);
                    return System.Drawing.Color.FromArgb(255, r, g, b);
                }
                catch (Exception ea)
                {
                    Debug.WriteLine(ea);
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
