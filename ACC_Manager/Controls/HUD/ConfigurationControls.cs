using ACC_Manager.Util.SystemExtensions;
using ACCManager.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.HUD.Overlay.Configuration.OverlayConfiguration;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using ACCManager.Controls.Util.SetupImage;
using static ACCManager.HUD.Overlay.Configuration.OverlaySettings;
using System.Windows.Input;
using static ACCManager.Controls.HUD.PreviewCache;

namespace ACCManager.Controls.HUD
{
    internal class ConfigurationControls
    {
        internal static StackPanel CreateUserControls(PropertyInfo pi, List<ConfigField> configFields, string configFieldGrouping, int fontSize, string overlayName)
        {
            if (pi.PropertyType == typeof(byte))
            {
                ByteRangeAttribute byteRange = null;
                ToolTipAttribute toolTip = null;
                foreach (Attribute cad in Attribute.GetCustomAttributes(pi))
                {
                    if (cad is ByteRangeAttribute)
                        byteRange = (ByteRangeAttribute)cad;

                    if (cad is ToolTipAttribute)
                        toolTip = (ToolTipAttribute)cad;
                }

                if (byteRange == null)
                    Debug.WriteLine($"Specify a ByteRangeAttribute for {pi.Name}");
                else
                {
                    ConfigField field = configFields.Where(x => x.Name == configFieldGrouping).ToArray().Where(x => x.Name == pi.Name).First();
                    Debug.WriteLine("Found field " + field.Name + " " + field.Value);

                    ConfigField configField = configFields.Where(cf => cf.Name == pi.Name).First();
                    string byteLabel = string.Concat(configField.Name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

                    StackPanel intStacker = new StackPanel()
                    {
                        Name = byteLabel.Replace(" ", "_"),
                        Margin = new Thickness(0, 0, 0, 0),
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Center,
                        Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
                    };

                    if (toolTip != null)
                        intStacker.ToolTip = toolTip.ToolTip;
                    ToolTipService.SetShowDuration(intStacker, int.MaxValue);

                    int min = byteRange.Min;
                    int max = byteRange.Max;
                    int tickFrequency = byteRange.Increment;
                    int sliderValue = int.Parse(configField.Value.ToString());
                    sliderValue.Clip(min, max);

                    int maxValueChars = $"{max}".Length;

                    Label sliderLabel = new Label
                    {
                        Content = $"{byteLabel}: {sliderValue.ToString("F0").FillStart(maxValueChars, ' ')}",
                        VerticalAlignment = VerticalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        FontSize = fontSize
                    };
                    intStacker.Children.Add(sliderLabel);

                    Slider slider = new Slider()
                    {
                        Minimum = min,
                        Maximum = max,
                        IsSnapToTickEnabled = true,
                        TickFrequency = tickFrequency,
                        Value = sliderValue,
                        Width = 150,
                        Margin = new Thickness(0, 0, 3, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,

                    };
                    slider.ValueChanged += (sender, args) =>
                    {
                        sliderLabel.Content = $"{byteLabel}: {slider.Value.ToString("F0").FillStart(maxValueChars, ' ')}";
                        configField.Value = (int)slider.Value;
                        configFields.RemoveAt(configFields.IndexOf(configField));
                        configFields.Add(configField);

                        //SaveOverlayConfigFields(overlayName, configFields);
                    };

                    //intStacker.MouseRightButtonUp += (sender, args) => { slider.Value = 1.0; };
                    intStacker.MouseWheel += (sender, args) =>
                    {
                        int delta = args.Delta;
                        slider.Value += delta.Clip(-1, 1) * tickFrequency;
                        args.Handled = true;
                    };
                    intStacker.MouseEnter += (sender, args) => { intStacker.Background = new SolidColorBrush(Color.FromArgb(50, 140, 0, 0)); }; ;
                    intStacker.MouseLeave += (sender, args) => { intStacker.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)); };

                    intStacker.Children.Add(slider);

                    return intStacker;
                }
            }


            if (pi.PropertyType == typeof(int))
            {
                IntRangeAttribute intRange = null;
                ToolTipAttribute toolTip = null;
                foreach (Attribute cad in Attribute.GetCustomAttributes(pi))
                {
                    if (cad is IntRangeAttribute)
                        intRange = (IntRangeAttribute)cad;

                    if (cad is ToolTipAttribute)
                        toolTip = (ToolTipAttribute)cad;
                }

                if (intRange == null)
                    Debug.WriteLine($"Specify an IntRangeAttribute for {pi.Name}");
                else
                {
                    var field = configFields.Where(x => x.Name == configFieldGrouping).First();

                    var actualField = (Newtonsoft.Json.Linq.JObject)field.Value;
                    var field2 = actualField.Children().Where(x => x.Path == pi.Name).First();

                    //.Where(x => x.Name == pi.Name).First();
                    string value = " asdasd";
                    Debug.WriteLine("Found field " + field2.Path + " " + field2.First);


                    ConfigField configField = configFields.Where(cf => cf.Name == pi.Name).First();
                    string intLabel = string.Concat(configField.Name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

                    StackPanel intStacker = new StackPanel()
                    {
                        Name = intLabel.Replace(" ", "_"),
                        Margin = new Thickness(0, 0, 0, 0),
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Center,
                        Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
                    };

                    if (toolTip != null)
                        intStacker.ToolTip = toolTip.ToolTip;
                    ToolTipService.SetShowDuration(intStacker, int.MaxValue);

                    int min = intRange.Min;
                    int max = intRange.Max;
                    int tickFrequency = intRange.Increment;
                    int sliderValue = int.Parse(configField.Value.ToString());
                    sliderValue.Clip(min, max);

                    int maxValueChars = $"{max}".Length;

                    Label sliderLabel = new Label
                    {
                        Content = $"{intLabel}: {sliderValue.ToString("F0").FillStart(maxValueChars, ' ')}",
                        VerticalAlignment = VerticalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        FontSize = fontSize
                    };
                    intStacker.Children.Add(sliderLabel);

                    Slider slider = new Slider()
                    {
                        Minimum = min,
                        Maximum = max,
                        IsSnapToTickEnabled = true,
                        TickFrequency = tickFrequency,
                        Value = sliderValue,
                        Width = 150,
                        Margin = new Thickness(0, 0, 3, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,

                    };
                    slider.ValueChanged += (sender, args) =>
                    {
                        sliderLabel.Content = $"{intLabel}: {slider.Value.ToString("F0").FillStart(maxValueChars, ' ')}";
                        configField.Value = (int)slider.Value;
                        configFields.RemoveAt(configFields.IndexOf(configField));
                        configFields.Add(configField);

                        //SaveOverlayConfigFields(overlayName, configFields);
                    };

                    //intStacker.MouseRightButtonUp += (sender, args) => { slider.Value = 1.0; };
                    intStacker.MouseWheel += (sender, args) =>
                    {
                        int delta = args.Delta;
                        slider.Value += delta.Clip(-1, 1) * tickFrequency;
                        args.Handled = true;
                    };
                    intStacker.MouseEnter += (sender, args) => { intStacker.Background = new SolidColorBrush(Color.FromArgb(50, 140, 0, 0)); }; ;
                    intStacker.MouseLeave += (sender, args) => { intStacker.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)); };

                    intStacker.Children.Add(slider);

                    return intStacker;
                }
            }

            if (pi.PropertyType == typeof(bool))
            {
                ToolTipAttribute toolTip = null;
                foreach (Attribute cad in Attribute.GetCustomAttributes(pi))
                {
                    if (cad is ToolTipAttribute)
                        toolTip = (ToolTipAttribute)cad;
                }

                StackPanel checkStacker = new StackPanel()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Cursor = Cursors.Hand,
                };
                if (toolTip != null)
                    checkStacker.ToolTip = toolTip.ToolTip;
                ToolTipService.SetShowDuration(checkStacker, int.MaxValue);


                ConfigField field = configFields.Where(x => x.Name == configFieldGrouping).ToArray().Where(x => x.Name == pi.Name).First();
                Debug.WriteLine("Found field " + field.Name + " " + field.Value);

                ConfigField configField = configFields.Where(cf => cf.Name == pi.Name).First();
                string checkBoxlabel = string.Concat(configField.Name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                CheckBox box = new CheckBox()
                {
                    Name = checkBoxlabel.Replace(" ", "_"),
                    Content = checkBoxlabel,
                    IsChecked = (bool)configField.Value,
                    Margin = new Thickness(0, 3, 5, 3),
                    VerticalAlignment = VerticalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontSize = fontSize
                };
                checkStacker.PreviewMouseDown += (s, e) => { if (s == checkStacker && e.LeftButton == MouseButtonState.Pressed) { box.IsChecked = !box.IsChecked; e.Handled = true; } };
                checkStacker.MouseEnter += (s, e) => checkStacker.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
                checkStacker.MouseLeave += (s, e) => checkStacker.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                box.Checked += (sender, args) =>
                {
                    configField.Value = true;
                    configFields.RemoveAt(configFields.IndexOf(configField));
                    configFields.Add(configField);

                    //SaveOverlayConfigFields(overlayName, configFields);
                };
                box.Unchecked += (sender, args) =>
                {
                    configField.Value = false;
                    configFields.RemoveAt(configFields.IndexOf(configField));
                    configFields.Add(configField);

                    //SaveOverlayConfigFields(overlayName, configFields);
                };
                checkStacker.Children.Add(box);
                return checkStacker;
            }
            return null;
        }

        private static void SaveOverlayConfigFields(ListView listOverlays, Image previewImage, string overlayName, List<ConfigField> configFields)
        {
            OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(overlayName);
            if (settings == null)
            {
                int screenMiddleX = (int)(SystemParameters.PrimaryScreenHeight / 2);
                int screenMiddleY = (int)(SystemParameters.PrimaryScreenHeight / 2);
                settings = new OverlaySettingsJson() { X = screenMiddleX, Y = screenMiddleY };
            }

            settings.Config = configFields;

            OverlaySettings.SaveOverlaySettings(overlayName, settings);

            // update preview image
            if (listOverlays.SelectedIndex >= 0)
            {
                ListViewItem lvi = (ListViewItem)listOverlays.SelectedItem;
                TextBlock tb = (TextBlock)lvi.Content;
                string actualOverlayName = overlayName.Replace("Overlay", "").Trim();
                if (tb.Text.Equals(actualOverlayName))
                {
                    GeneratePreview(actualOverlayName);
                    PreviewCache._cachedPreviews.TryGetValue(actualOverlayName, out CachedPreview preview);
                    if (preview != null)
                    {
                        previewImage.Stretch = Stretch.UniformToFill;
                        previewImage.Width = preview.Width;
                        previewImage.Height = preview.Height;
                        previewImage.Source = ImageControlCreator.CreateImage(preview.Width, preview.Height, preview.CachedBitmap).Source;
                    }
                    else
                    {
                        previewImage.Source = null;
                    }
                }
            }
        }


    }
}
