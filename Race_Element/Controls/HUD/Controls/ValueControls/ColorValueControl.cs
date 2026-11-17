using MaterialDesignThemes.Wpf;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;

namespace RaceElement.Controls.HUD.Controls.ValueControls;

internal sealed class ColorValueControl : IValueControl<System.Drawing.Color>
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
            Width = ControlConstants.ControlWidth,
            Height = 66, // RgbInputPanel matches this, see RgbInputPanel.CreateBox
            Margin = new Thickness(0, 1, 7, 1),
            Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center,
        };

        // add columns for the color picker and RGB inputs
        // inputs take what they need, color picker takes all remaining space
        _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

        ColorPicker colorPicker = new()
        {
            HueSliderPosition = Dock.Bottom,
            Margin = new Thickness(0, 0, 0, -7),
        };
        System.Drawing.Color loadedColor = FromToString(configField.Value.ToString());
        colorPicker.Color = Color.FromArgb(loadedColor.A, loadedColor.R, loadedColor.G, loadedColor.B);

        _grid.Children.Add(colorPicker);
        Grid.SetColumn(colorPicker, 0);

        RgbInputPanel rgbInput = new();
        rgbInput.SetColor(colorPicker.Color);

        _grid.Children.Add(rgbInput);
        Grid.SetColumn(rgbInput, 1);

        // apply the entered RGB values to the color picker and save them, picker alpha stays unchanged
        rgbInput.RgbEntered += (r, g, b) =>
        {
            colorPicker.Color = Color.FromArgb(colorPicker.Color.A, r, g, b);
            SaveColor();
        };

        // when a color is picked, save it and update the RGB inputs
        // changes are detected on mouse release or on key release for keyboard control of the hue slider
        colorPicker.PreviewMouseLeftButtonUp += (s, e) => OnColorPicked();
        colorPicker.PreviewKeyUp += (s, e) =>
        {
            if (e.Key is Key.Left or Key.Right or Key.Up or Key.Down or Key.Home or Key.End or Key.PageUp or Key.PageDown)
            {
                OnColorPicked();
            }
        };

        void OnColorPicked()
        {
            SaveColor();
            rgbInput.SetColor(colorPicker.Color);
        }

        void SaveColor()
        {
            var drawingColor = System.Drawing.Color.FromArgb(colorPicker.Color.A, colorPicker.Color.R, colorPicker.Color.G, colorPicker.Color.B);
            _field.Value = drawingColor.ToString();

            Save();
        }
    }

    private System.Drawing.Color FromToString(string value)
    {
        try
        {
            if (value.Contains("#") || (value.Contains("[") && !value.Contains("A=")))
            {
                string replacement = value.Replace("Color [", "");
                replacement = replacement.Replace("]", "");
                System.Drawing.Color color = (System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromString(replacement);
                return color;
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
        catch (Exception e) { Debug.WriteLine(e.StackTrace); }
        return System.Drawing.Color.Red;
    }

    public void Save()
    {
        ConfigurationControls.SaveOverlayConfigField(_field);
    }

    /// <summary>
    /// Three text boxes with validation for the red, green and blue channel values. Raises the <see cref="RgbEntered"/> event when the user commits valid values.
    /// </summary>
    private sealed class RgbInputPanel : StackPanel
    {
        private readonly TextBox _redBox;
        private readonly TextBox _greenBox;
        private readonly TextBox _blueBox;

        /// <summary>
        /// Last valid RGB values. The text boxes are restored to these on invalid input.
        /// </summary>
        private (byte R, byte G, byte B) _current;

        /// <summary>
        /// Raised when new RGB values are committed.
        /// </summary>
        public event Action<byte, byte, byte>? RgbEntered;

        public RgbInputPanel()
        {
            Orientation = Orientation.Vertical;
            Margin = new Thickness(5, 0, 7, 0);
            VerticalAlignment = VerticalAlignment.Center;

            _redBox = CreateBox("Red");
            _greenBox = CreateBox("Green");
            _blueBox = CreateBox("Blue");
            TextBox[] boxes = [_redBox, _greenBox, _blueBox];

            string[] labels = ["R", "G", "B"];
            for (int i = 0; i < boxes.Length; i++)
            {
                DockPanel row = new() { Margin = new Thickness(0, 1, 0, 1) };
                TextBlock label = new() { Text = labels[i], Width = 12, FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
                DockPanel.SetDock(label, Dock.Left);
                row.Children.Add(label);
                row.Children.Add(boxes[i]);
                Children.Add(row);
            }

            foreach (TextBox box in boxes)
            {
                // pasting does not raise PreviewTextInput, so it is validated here
                DataObject.AddPastingHandler(box, (s, e) =>
                {
                    e.CancelCommand();
                    PasteChannel(box, e.DataObject.GetData(DataFormats.UnicodeText) as string);
                });

                // read box values only on lost focus or the enter key to avoid reading half-typed values
                box.LostFocus += (s, e) => ApplyBoxes();
                box.KeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        ApplyBoxes();
                    }
                };
            }
        }

        /// <summary>
        /// Sets the displayed RGB values from <paramref name="color"/>. Alpha is ignored.
        /// </summary>
        public void SetColor(Color color)
        {
            _current = (color.R, color.G, color.B);
            ShowCurrent();
        }

        /// <summary>
        /// Renders the RGB values of <see cref="_current"/> inside the text boxes.
        /// </summary>
        private void ShowCurrent()
        {
            _redBox.Text = _current.R.ToString();
            _greenBox.Text = _current.G.ToString();
            _blueBox.Text = _current.B.ToString();
        }

        /// <summary>
        /// Commits valid RGB values of the text boxes to <see cref="_current"/> and raises <see cref="RgbEntered"/> when they change.
        /// </summary>
        private void ApplyBoxes()
        {
            // accept whole numbers from 0-255
            if (byte.TryParse(_redBox.Text, out byte r) && byte.TryParse(_greenBox.Text, out byte g) && byte.TryParse(_blueBox.Text, out byte b))
            {
                if ((r, g, b) != _current)
                {
                    _current = (r, g, b);
                    RgbEntered?.Invoke(r, g, b);
                }
            }

            // restore the last valid values or normalize valid ones (e.g. 007 -> 7)
            ShowCurrent();
        }

        /// <summary>
        /// Replaces the value of <paramref name="box"/> with the trimmed pasted text when it is<br/>
        /// a whole number from 0-255, otherwise the box is reset to its last valid value.
        /// </summary>
        private void PasteChannel(TextBox box, string? text)
        {
            if (text != null && byte.TryParse(text.Trim(), out byte value))
            {
                box.Text = value.ToString();
                box.CaretIndex = box.Text.Length;
                return;
            }

            ShowCurrent();
        }

        /// <summary>
        /// Creates a centered text box for a single color channel with digit-only typing and select-all on focus.
        /// </summary>
        /// <remarks>
        /// The box height and the 1px top and bottom row margin set in the <see cref="RgbInputPanel"/> constructor should fit<br/>
        /// within the grid height of <see cref="ColorValueControl"/>.
        /// </remarks>
        /// <param name="toolTip">Tooltip text shown on the box (e.g. the channel name).</param>
        private static TextBox CreateBox(string toolTip)
        {
            TextBox box = new()
            {
                Width = 36,
                MaxLength = 3,
                Height = 20, // 3 rows × (20 box + 1px margin top + 1px margin bottom) = 66
                FontSize = 11,
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                ToolTip = toolTip,
            };
            TextFieldAssist.SetCharacterCounterVisibility(box, Visibility.Collapsed);

            // block non-digit typing
            box.PreviewTextInput += (s, e) => e.Handled = !e.Text.All(char.IsAsciiDigit);

            // select the whole value on keyboard focus ...
            box.GotKeyboardFocus += (s, e) => box.SelectAll();
            // ... and on mouse click (WPF otherwise puts the caret on mouse-up and drops the selection)
            box.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (!box.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    box.Focus();
                }
            };

            return box;
        }
    }
}
