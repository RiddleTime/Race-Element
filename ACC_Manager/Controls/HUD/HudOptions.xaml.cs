using ACC_Manager.Util.NumberExtensions;
using ACCManager.Controls.HUD;
using ACCManager.HUD.ACC;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.Util;
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
using static ACCManager.HUD.Overlay.Configuration.OverlayConfiguration;
using static ACCManager.HUD.Overlay.Configuration.OverlaySettings;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for HudOptions.xaml
    /// </summary>
    public partial class HudOptions : UserControl
    {
        private KeyboardHook _hook = new KeyboardHook();

        private static HudOptions _instance;
        public static HudOptions Instance { get { return _instance; } }

        public HudOptions()
        {
            InitializeComponent();

            try
            {
                BuildOverlayStackPanel();
                checkBoxReposition.Checked += (s, e) => SetRepositionMode(true);
                checkBoxReposition.Unchecked += (s, e) => SetRepositionMode(false);

                this.PreviewMouseUp += (s, e) =>
                {
                    if (e.ChangedButton == MouseButton.Middle)
                        this.checkBoxReposition.IsChecked = !this.checkBoxReposition.IsChecked;
                };

                _hook.RegisterHotKey(HUD.ModifierKeys.Control | HUD.ModifierKeys.Shift, System.Windows.Forms.Keys.M);
                _hook.KeyPressed += (s, ev) => { this.checkBoxReposition.IsChecked = !this.checkBoxReposition.IsChecked; };

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                LogWriter.WriteToLog(ex);
            }

            _instance = this;
        }

        public void DisposeKeyboardHooks()
        {
            Debug.WriteLine("Disposing HUD Keyboard hooks");
            _hook.Dispose();
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
                Label label = new Label() { Content = x.Key, FontSize = 16, Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Center, Width = 180 };
                stackPanel.Children.Add(label);
                StackPanel configStacker = GetConfigStacker(x.Value);
                stackPanel.Children.Add(configStacker);

                card.MouseLeftButtonDown += (s, e) => { if (s == card) { toggle.IsChecked = !toggle.IsChecked; } };
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

                        Task.Run(() =>
                        {
                            overlay?.Stop();
                        });

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
            StackPanel stacker = new StackPanel()
            {
                Margin = new Thickness(10, 0, 0, 0),
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
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
                        ConfigField configField = configFields.Where(cf => cf.Name == pi.Name).First();
                        string intLabel = string.Concat(configField.Name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

                        StackPanel intStacker = new StackPanel()
                        {
                            Name = intLabel.Replace(" ", "_"),
                            Margin = new Thickness(5, 0, 10, 0),
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


                        Label sliderLabel = new Label
                        {
                            Content = $"{intLabel}: {sliderValue:F0}",
                            VerticalAlignment = VerticalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                        };
                        intStacker.Children.Add(sliderLabel);

                        Slider slider = new Slider()
                        {
                            Minimum = min,
                            Maximum = max,
                            IsSnapToTickEnabled = true,
                            TickFrequency = tickFrequency,
                            Value = sliderValue,
                            Width = 100,
                            Margin = new Thickness(0, 0, 3, 0),
                            VerticalAlignment = VerticalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,

                        };
                        slider.ValueChanged += (sender, args) =>
                        {
                            sliderLabel.Content = $"{intLabel}: {slider.Value:F0}";
                            configField.Value = (int)slider.Value;
                            configFields.RemoveAt(configFields.IndexOf(configField));
                            configFields.Add(configField);

                            SaveOverlayConfigFields(overlayName, configFields);
                        };

                        //intStacker.MouseRightButtonUp += (sender, args) => { slider.Value = 1.0; };
                        intStacker.MouseWheel += (sender, args) =>
                        {
                            int delta = args.Delta;
                            slider.Value += delta.Clip(-1, 1) * tickFrequency;
                        };
                        intStacker.MouseEnter += (sender, args) => { intStacker.Background = new SolidColorBrush(Color.FromArgb(50, 140, 0, 0)); }; ;
                        intStacker.MouseLeave += (sender, args) => { intStacker.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)); };

                        intStacker.Children.Add(slider);

                        configStackers.Add(intStacker);
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

                    ConfigField configField = configFields.Where(cf => cf.Name == pi.Name).First();
                    string checkBoxlabel = string.Concat(configField.Name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                    CheckBox box = new CheckBox()
                    {
                        Name = checkBoxlabel.Replace(" ", "_"),
                        Content = checkBoxlabel,
                        IsChecked = (bool)configField.Value,
                        Margin = new Thickness(5, 3, 5, 3),
                        VerticalAlignment = VerticalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };
                    checkStacker.PreviewMouseDown += (s, e) => { if (s == checkStacker && e.LeftButton == MouseButtonState.Pressed) { box.IsChecked = !box.IsChecked; e.Handled = true; } };
                    checkStacker.MouseEnter += (s, e) => checkStacker.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
                    checkStacker.MouseLeave += (s, e) => checkStacker.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
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
                    checkStacker.Children.Add(box);
                    configStackers.Add(checkStacker);
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
                                Margin = new Thickness(0, 0, 10, 0),
                                Orientation = Orientation.Horizontal,
                                VerticalAlignment = VerticalAlignment.Center,
                                Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
                                ToolTip = "Right click to reset. Scroll Me.",
                            };
                            ToolTipService.SetShowDuration(sliderStacker, int.MaxValue);

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
                                Margin = new Thickness(0, 0, 3, 0),
                                VerticalAlignment = VerticalAlignment.Center,
                                VerticalContentAlignment = VerticalAlignment.Center,

                            };
                            slider.ValueChanged += (sender, args) =>
                            {
                                sliderLabel.Content = $"{floatLabel}: {slider.Value:F1}";
                                configField.Value = slider.Value;
                                configFields.RemoveAt(configFields.IndexOf(configField));
                                configFields.Add(configField);

                                SaveOverlayConfigFields(overlayName, configFields);
                            };

                            sliderStacker.MouseRightButtonUp += (sender, args) => { slider.Value = 1.0; };
                            sliderStacker.MouseWheel += (sender, args) =>
                            {
                                int delta = args.Delta;
                                slider.Value += delta.Clip(-1, 1) * tickFrequency;
                            };
                            sliderStacker.MouseEnter += (sender, args) => { sliderStacker.Background = new SolidColorBrush(Color.FromArgb(50, 180, 0, 0)); }; ;
                            sliderStacker.MouseLeave += (sender, args) => { sliderStacker.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)); };

                            sliderStacker.Children.Add(slider);

                            stacker.Children.Add(sliderStacker);
                        }
                    }
                }
            };

            configStackers.Sort((a, b) =>
            {
                return a.Name.CompareTo(b.Name);
            });


            configStackers.ForEach(x => stacker.Children.Add(x));


            return stacker;
        }

        private void SaveOverlayConfigFields(string overlayName, List<ConfigField> configFields)
        {
            OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(overlayName);
            if (settings == null)
            {
                int screenMiddleX = (int)(System.Windows.SystemParameters.FullPrimaryScreenWidth / 2);
                int screenMiddleY = (int)(System.Windows.SystemParameters.FullPrimaryScreenHeight / 2);
                settings = new OverlaySettingsJson() { X = screenMiddleX, Y = screenMiddleY };
            }

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

            //Debug.WriteLine($"Finding OverlayConfiguration in {tempOverlay.Name}");
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
