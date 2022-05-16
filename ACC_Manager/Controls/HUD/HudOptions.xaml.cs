using ACC_Manager.Util.NumberExtensions;
using ACCManager.HUD.ACC;
using ACCManager.HUD.Overlay.Internal;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ACCManager.HUD.Overlay.Internal.OverlayConfiguration;
using static ACCManager.HUD.Overlay.Internal.OverlaySettings;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for HudOptions.xaml
    /// </summary>
    public partial class HudOptions : UserControl
    {
        public HudOptions()
        {
            InitializeComponent();

            BuildOverlayStackPanel();
            labelReposition.MouseUp += (o, e) => { checkBoxReposition.IsChecked = !checkBoxReposition.IsChecked; };
            checkBoxReposition.Checked += (s, e) => SetRepositionMode(true);
            checkBoxReposition.Unchecked += (s, e) => SetRepositionMode(false);
        }

        private void SetRepositionMode(bool enabled)
        {
            stackPanelOverlayCheckboxes.IsEnabled = !enabled;

            lock (OverlaysACC.ActiveOverlays)
                foreach (AbstractOverlay overlay in OverlaysACC.ActiveOverlays)
                {
                    overlay.EnableReposition(enabled);
                }
        }

        private void BuildOverlayStackPanel()
        {
            int screenMiddleX = (int)(System.Windows.SystemParameters.FullPrimaryScreenWidth / 2);
            int screenMiddleY = (int)(System.Windows.SystemParameters.FullPrimaryScreenHeight / 2);

            stackPanelOverlayCheckboxes.Children.Clear();
            foreach (KeyValuePair<string, Type> x in OverlaysACC.AbstractOverlays)
            {
                object[] args = new object[] { new System.Drawing.Rectangle((int)System.Windows.SystemParameters.PrimaryScreenWidth / 2, (int)System.Windows.SystemParameters.PrimaryScreenHeight / 2, 300, 150) };

                Card card = new Card() { Margin = new Thickness(2) };
                StackPanel stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                card.Content = stackPanel;

                ToggleButton toggle = new ToggleButton() { Height = 35, Width = 50, Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Center };
                stackPanel.Children.Add(toggle);
                Label label = new Label() { Content = x.Key, FontSize = 16, Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Center };
                label.MouseUp += (s, e) => { toggle.IsChecked = !toggle.IsChecked; };
                stackPanel.Children.Add(label);
                StackPanel configStacker = GetConfigStacker(x.Value);
                stackPanel.Children.Add(configStacker);

                toggle.Checked += (s, e) =>
                {
                    lock (OverlaysACC.ActiveOverlays)
                    {
                        AbstractOverlay overlay = (AbstractOverlay)Activator.CreateInstance(x.Value, args);

                        overlay.Start();

                        SaveOverlaySettings(overlay, true);

                        configStacker.IsEnabled = false;
                        OverlaysACC.ActiveOverlays.Add(overlay);
                    }
                };

                toggle.Unchecked += (s, e) =>
                {
                    lock (OverlaysACC.ActiveOverlays)
                    {
                        AbstractOverlay overlay = OverlaysACC.ActiveOverlays.Find(f => f.GetType() == x.Value);

                        SaveOverlaySettings(overlay, false);

                        overlay?.Stop();
                        OverlaysACC.ActiveOverlays.Remove(overlay);
                        configStacker.IsEnabled = true;
                    }
                };

                AbstractOverlay tempOverlay = (AbstractOverlay)Activator.CreateInstance(x.Value, args);
                OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(tempOverlay.Name);
                if (settings != null)
                {
                    if (settings.Enabled)
                    {
                        toggle.IsChecked = true;
                    }
                }
                tempOverlay.Dispose();

                stackPanelOverlayCheckboxes.Children.Add(card);
            }
        }

        private StackPanel GetConfigStacker(Type overlayType)
        {
            StackPanel stacker = new StackPanel() { Margin = new Thickness(10, 0, 0, 0), Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            List<FrameworkElement> configStackers = new List<FrameworkElement>();

            OverlayConfiguration overlayConfig = GetOverlayConfig(overlayType);
            if (overlayConfig == null) return stacker;


            string overlayName = GetOverlayName(overlayType);

            List<ConfigField> configFields = null;
            OverlaySettingsJson overlaySettings = OverlaySettings.LoadOverlaySettings(overlayName);
            if (overlaySettings == null)
                configFields = overlayConfig.GetConfigFields();
            else
                configFields = overlaySettings.Config;

            if (configFields == null)
                configFields = overlayConfig.GetConfigFields();

            List<PropertyInfo> props = overlayConfig.GetProperties();
            if (props.Count != configFields.Count)
            {
                configFields = overlayConfig.GetConfigFields();
            }
            else
            {
                foreach (PropertyInfo property in props)
                {
                    ConfigField field = configFields.Find(x => x.Name == property.Name);
                    if (field == null)
                    {
                        configFields = overlayConfig.GetConfigFields();
                        break;
                    }
                }
            }

            foreach (PropertyInfo pi in props)
            {
                if (pi.PropertyType == typeof(bool))
                {
                    ConfigField configField = configFields.Where(cf => cf.Name == pi.Name).First();
                    string checkBoxlabel = string.Concat(configField.Name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                    CheckBox box = new CheckBox()
                    {
                        Content = checkBoxlabel,
                        IsChecked = (bool)configField.Value,
                        Margin = new Thickness(5, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    box.Checked += (sender, args) =>
                    {
                        configField.Value = true;
                        configFields.RemoveAt(configFields.IndexOf(configField));
                        configFields.Add(configField);

                        SaveOverlayConfigFields(overlayName, configFields);
                    };
                    box.Unchecked += (sender, args) =>
                    {
                        configField.Value = false;
                        configFields.RemoveAt(configFields.IndexOf(configField));
                        configFields.Add(configField);

                        SaveOverlayConfigFields(overlayName, configFields);
                    };
                    configStackers.Add(box);
                }

                if (pi.PropertyType == typeof(float))
                {
                    ConfigField configField = configFields.Where(cf => cf.Name == pi.Name).First();
                    if (configField.Name == "Scale")
                    {
                        object[] tempOverlayArgs = new object[] { new System.Drawing.Rectangle(0, 0, 300, 150) };
                        AbstractOverlay tempOverlay = (AbstractOverlay)Activator.CreateInstance(overlayType, tempOverlayArgs);
                        bool allowsRescaling = overlayConfig.AllowRescale;
                        tempOverlay.Dispose();

                        if (allowsRescaling)
                        {
                            StackPanel sliderStacker = new StackPanel()
                            {
                                Name = "ScaleSlider",
                                Margin = new Thickness(10, 0, 10, 0),
                                Orientation = Orientation.Horizontal,
                                VerticalAlignment = VerticalAlignment.Center,
                                Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0))
                            };

                            double min = 0.5;
                            double max = 2.0;
                            double tickFrequency = 0.1;
                            double sliderValue = double.Parse(configField.Value.ToString());
                            sliderValue.Clip(min, max);

                            string floatLabel = string.Concat(configField.Name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

                            Label sliderLabel = new Label
                            {
                                Content = $"{floatLabel}: {sliderValue:F1}",
                                VerticalAlignment = VerticalAlignment.Center,
                                VerticalContentAlignment = VerticalAlignment.Center,
                            };
                            sliderStacker.Children.Add(sliderLabel);

                            Slider slider = new Slider()
                            {
                                Minimum = min,
                                Maximum = max,
                                IsSnapToTickEnabled = true,
                                TickFrequency = tickFrequency,
                                Value = sliderValue,
                                Width = 100,
                                VerticalAlignment = VerticalAlignment.Center,
                                VerticalContentAlignment = VerticalAlignment.Center,
                                ToolTip = "Scroll Me.\nRight click to reset."
                            };
                            slider.ValueChanged += (sender, args) =>
                            {
                                sliderLabel.Content = $"{floatLabel}: {slider.Value:F1}";
                                configField.Value = slider.Value;
                                configFields.RemoveAt(configFields.IndexOf(configField));
                                configFields.Add(configField);

                                SaveOverlayConfigFields(overlayName, configFields);
                            };
                            slider.MouseRightButtonUp += (sender, args) => { slider.Value = 1.0; };
                        

                            sliderStacker.Children.Add(slider);

                            sliderStacker.MouseWheel += (sender, args) =>
                            {
                                int delta = args.Delta;
                                slider.Value += delta.Clip(-1, 1) * tickFrequency;
                            };

                            configStackers.Add(sliderStacker);
                        }
                    }
                }
            };

            configStackers.Sort((a, b) =>
            {
                if (b.Name == "ScaleSlider")
                    return 1;

                return a.Name.CompareTo(b.Name);
            });


            configStackers.ForEach(x => stacker.Children.Add(x));


            return stacker;
        }

        private void SaveOverlayConfigFields(string overlayName, List<ConfigField> configFields)
        {
            OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(overlayName);
            settings.Config = configFields;
            OverlaySettings.SaveOverlaySettings(overlayName, settings);
        }

        private void SaveOverlaySettings(AbstractOverlay overlay, bool isEnabled)
        {
            OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(overlay.Name);
            if (settings == null)
            {
                settings = new OverlaySettingsJson() { X = overlay.X, Y = overlay.Y };
            }

            settings.Enabled = isEnabled;

            OverlaySettings.SaveOverlaySettings(overlay.Name, settings);
        }


        private void SaveOverlayConfig(Type overlay, OverlayConfiguration overlayConfiguration)
        {
            object[] args = new object[] { new System.Drawing.Rectangle(0, 0, 300, 150) };
            AbstractOverlay tempOverlay = (AbstractOverlay)Activator.CreateInstance(overlay, args);
            SaveOverlayConfig(tempOverlay, overlayConfiguration);
            tempOverlay.Dispose();
        }

        private void SaveOverlayConfig(AbstractOverlay overlay, OverlayConfiguration overlayConfiguration)
        {
            OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(overlay.Name);
            if (settings == null)
            {
                settings = new OverlaySettingsJson() { X = overlay.X, Y = overlay.Y };
            }

            settings.Config = overlayConfiguration.GetConfigFields();

            OverlaySettings.SaveOverlaySettings(overlay.Name, settings);
        }

        private string GetOverlayName(Type overlay)
        {
            object[] args = new object[] { new System.Drawing.Rectangle(0, 0, 300, 150) };
            AbstractOverlay tempOverlay = (AbstractOverlay)Activator.CreateInstance(overlay, args);
            string name = tempOverlay.Name;
            tempOverlay.Dispose();

            return name;
        }

        private OverlayConfiguration GetOverlayConfig(Type overlay)
        {
            object[] args = new object[] { new System.Drawing.Rectangle(0, 0, 300, 150) };
            AbstractOverlay tempOverlay = (AbstractOverlay)Activator.CreateInstance(overlay, args);

            OverlayConfiguration temp = null;

            Debug.WriteLine($"Finding OverlayConfiguration in {tempOverlay.Name}");
            FieldInfo[] fields = tempOverlay.GetType().GetRuntimeFields().ToArray();
            foreach (var nested in fields)
            {
                if (nested.FieldType.BaseType == typeof(OverlayConfiguration))
                {
                    //Debug.WriteLine($"Found {nested.Name} - {nested.GetValue(overlay)}");
                    temp = (OverlayConfiguration)Activator.CreateInstance(nested.FieldType, new object[] { });
                    return temp;
                }
            }

            tempOverlay.Dispose();

            return temp;
        }
    }
}
