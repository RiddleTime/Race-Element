using ACC_Manager.Util.Settings;
using ACC_Manager.Util.SystemExtensions;
using ACCManager.Controls.Util.SetupImage;
using ACCManager.HUD.ACC;
using ACCManager.HUD.ACC.Overlays.OverlayMousePosition;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.Util;
using Gma.System.MouseKeyHook;
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
using static ACCManager.HUD.Overlay.Configuration.OverlayConfiguration;
using static ACCManager.HUD.Overlay.Configuration.OverlaySettings;

// https://github.com/gmamaladze/globalmousekeyhook
namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for HudOptions.xaml
    /// </summary>
    public partial class HudOptions : UserControl
    {
        private IKeyboardMouseEvents m_GlobalHook;

        private static HudOptions _instance;
        public static HudOptions Instance { get { return _instance; } }

        private readonly Dictionary<string, CachedPreview> _cachedPreviews = new Dictionary<string, CachedPreview>();

        private readonly object[] DefaultOverlayArgs = new object[] { new System.Drawing.Rectangle((int)SystemParameters.PrimaryScreenWidth / 2, (int)SystemParameters.PrimaryScreenHeight / 2, 300, 150) };

        private class CachedPreview
        {
            public int Width;
            public int Height;
            public CachedBitmap CachedBitmap;
        }

        public HudOptions()
        {
            InitializeComponent();

            listOverlays.SelectionChanged += ListOverlays_SelectionChanged;
            listDebugOverlays.SelectionChanged += ListDebugOverlays_SelectionChanged;
            tabControlListOverlays.SelectionChanged += (s, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    if (s is ListViewItem)
                    {
                        e.Handled = true;
                        return;
                    }
                }

                configStackPanel.Children.Clear();
                previewImage.Source = null;
                listDebugOverlays.SelectedIndex = -1;
                listOverlays.SelectedIndex = -1;
            };

            bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
                try
                {
                    BuildOverlayPanel();

                    checkBoxReposition.Checked += (s, e) =>
                    {
                        SetRepositionMode(true);

                    };
                    checkBoxReposition.Unchecked += (s, e) =>
                    {
                        SetRepositionMode(false);
                    };

                    checkBoxDemoMode.Checked += (s, e) =>
                    {
                        HudSettings.DemoMode = true;
                    };

                    checkBoxDemoMode.Unchecked += (s, e) =>
                    {
                        HudSettings.DemoMode = false;
                    };

                    this.PreviewMouseUp += (s, e) =>
                    {
                        if (e.ChangedButton == MouseButton.Middle)
                        {
                            this.checkBoxReposition.IsChecked = !this.checkBoxReposition.IsChecked;
                            e.Handled = true;
                        }
                    };

                    gridRepositionToggler.MouseUp += (s, e) =>
                    {
                        this.checkBoxReposition.IsChecked = !this.checkBoxReposition.IsChecked;
                        e.Handled = true;
                    };
                    gridDemoToggler.MouseUp += (s, e) =>
                    {
                        this.checkBoxDemoMode.IsChecked = !this.checkBoxDemoMode.IsChecked;
                    };

                    m_GlobalHook = Hook.GlobalEvents();
                    m_GlobalHook.OnCombination(new Dictionary<Combination, Action> { { Combination.FromString("Control+Home"), () => this.checkBoxReposition.IsChecked = !this.checkBoxReposition.IsChecked } });

                    this.PreviewKeyDown += HudOptions_PreviewKeyDown;

#if DEBUG
                    checkBoxDemoMode.IsChecked = true;
#endif
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    LogWriter.WriteToLog(ex);
                }

            _instance = this;
        }


        private DateTime _lastOverlayStart = DateTime.MinValue;
        private void HudOptions_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // kind of stupid but it works.. gotta travel the generated tree in #BuildOverlayConfigPanel();
            if (e.Key == Key.Enter)
                if (_lastOverlayStart.AddMilliseconds(200) < DateTime.Now)
                    foreach (UIElement element in configStackPanel.Children)
                        if (element is StackPanel panel)
                            foreach (UIElement child in panel.Children)
                                if (child is ToggleButton toggle)
                                {
                                    toggle.IsChecked = !toggle.IsChecked;
                                    _lastOverlayStart = DateTime.Now;
                                    e.Handled = true;
                                    break;
                                }
        }

        private void ListOverlays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)e.AddedItems[0];
                KeyValuePair<string, Type> kv = (KeyValuePair<string, Type>)item.DataContext;
                BuildOverlayConfigPanel(item, kv.Value);
                e.Handled = true;
            }
            else
                configStackPanel.Children.Clear();
        }

        private void ListDebugOverlays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)e.AddedItems[0];
                KeyValuePair<string, Type> kv = (KeyValuePair<string, Type>)item.DataContext;
                BuildOverlayConfigPanel(item, kv.Value);
                e.Handled = true;
            }
            else
                configStackPanel.Children.Clear();
        }

        private void BuildOverlayConfigPanel(ListViewItem listViewItem, Type type)
        {
            configStackPanel.Children.Clear();

            AbstractOverlay tempAbstractOverlay = (AbstractOverlay)Activator.CreateInstance(type, DefaultOverlayArgs);
            OverlaySettingsJson tempOverlaySettings = OverlaySettings.LoadOverlaySettings(tempAbstractOverlay.Name);
            tempAbstractOverlay.Dispose();

            OverlayAttribute overlayAttribute = GetOverlayAttribute(type);
            GeneratePreview(overlayAttribute.Name);

            StackPanel configStacker = GetConfigStacker(type, Orientation.Vertical);

            Label overlayNameLabel = new Label()
            {
                Content = overlayAttribute.Name,
                FontFamily = FindResource("FontRedemption") as FontFamily,
                BorderBrush = Brushes.OrangeRed,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Margin = new Thickness(0, 0, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 30,
                FontStyle = FontStyles.Italic
            };
            TextBlock overlayDescription = new TextBlock()
            {
                Text = overlayAttribute.Description,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontSize = 13.5,
                Margin = new Thickness(0, 0, 0, 3),
            };
            StackPanel stackerOverlayInfo = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 3, 0, 3),
                Background = new SolidColorBrush(Color.FromArgb(190, 0, 0, 0)),
            };
            stackerOverlayInfo.Children.Add(overlayNameLabel);
            stackerOverlayInfo.Children.Add(overlayDescription);
            configStackPanel.Children.Add(stackerOverlayInfo);

            StackPanel activationPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand,
                Name = "activationStacker",
                ToolTip = "You can else press Enter to activate this overlay."
            };
            Label nameLabel = new Label() { Content = tempOverlaySettings.Enabled ? "Deactivate" : "Activate", VerticalAlignment = VerticalAlignment.Center };
            ToggleButton toggle = new ToggleButton() { Height = 35, Width = 50, VerticalAlignment = VerticalAlignment.Center };
            toggle.PreviewKeyDown += (s, e) => { if (e.Key == Key.Enter) e.Handled = true; };
            toggle.Checked += (s, e) =>
            {
                lock (OverlaysACC.ActiveOverlays)
                {
                    AbstractOverlay overlay = OverlaysACC.ActiveOverlays.Find(f => f.GetType() == type);

                    if (overlay == null)
                    {
                        listViewItem.Background = new SolidColorBrush(Color.FromArgb(50, 10, 255, 10));
                        nameLabel.Content = "Deactivate";
                        overlay = (AbstractOverlay)Activator.CreateInstance(type, DefaultOverlayArgs);

                        overlay.Start();

                        SaveOverlaySettings(overlay, true);

                        configStacker.IsEnabled = false;
                        OverlaysACC.ActiveOverlays.Add(overlay);
                    }
                }
            };
            toggle.Unchecked += (s, e) =>
            {
                lock (OverlaysACC.ActiveOverlays)
                {
                    listViewItem.Background = Brushes.Transparent;
                    nameLabel.Content = "Activate";
                    AbstractOverlay overlay = OverlaysACC.ActiveOverlays.Find(f => f.GetType() == type);

                    SaveOverlaySettings(overlay, false);

                    Task.Run(() =>
                    {
                        overlay?.Stop();
                    });

                    OverlaysACC.ActiveOverlays.Remove(overlay);
                    configStacker.IsEnabled = true;
                }
            };
            activationPanel.MouseLeftButtonUp += (s, e) => toggle.IsChecked = !toggle.IsChecked;

            if (tempOverlaySettings.Enabled)
            {

                toggle.IsChecked = true;
                configStacker.IsEnabled = false;
            }

            activationPanel.Children.Add(toggle);
            activationPanel.Children.Add(nameLabel);
            configStackPanel.Children.Add(activationPanel);

            // click overlay title/description to toggle overlay
            stackerOverlayInfo.Cursor = Cursors.Hand;
            stackerOverlayInfo.PreviewMouseUp += (s, e) =>
            {
                if (_lastOverlayStart.AddMilliseconds(200) < DateTime.Now)
                {
                    toggle.IsChecked = !toggle.IsChecked;
                    _lastOverlayStart = DateTime.Now;
                }
            };


            // add config stacker
            configStackPanel.Children.Add(configStacker);

            // add preview iamge
            _cachedPreviews.TryGetValue(overlayAttribute.Name, out CachedPreview preview);
            if (preview == null)
                previewImage.Source = null;
            else
            {
                previewImage.Stretch = Stretch.UniformToFill;
                previewImage.Width = preview.Width;
                previewImage.Height = preview.Height;
                previewImage.Source = ImageControlCreator.CreateImage(preview.Width, preview.Height, preview.CachedBitmap).Source;
            }
        }

        public void DisposeKeyboardHooks()
        {
            if (m_GlobalHook != null)
                m_GlobalHook.Dispose();
        }

        private MousePositionOverlay mousePositionOverlay;
        private void SetRepositionMode(bool enabled)
        {
            gridOverlays.IsEnabled = !enabled;

            if (enabled)
            {
                mousePositionOverlay = new MousePositionOverlay(new System.Drawing.Rectangle(0, 0, 150, 150), "Mouse Position");
                mousePositionOverlay.Start(false);
            }
            else
            {
                if (mousePositionOverlay != null)
                    mousePositionOverlay.Stop();
            }

            lock (OverlaysACC.ActiveOverlays)
                foreach (AbstractOverlay overlay in OverlaysACC.ActiveOverlays)
                {
                    overlay.EnableReposition(enabled);
                }

        }

        private void BuildOverlayPanel()
        {
            OverlaysACC.GenerateDictionary();

            //BuildOverlayStackPanel(stackPanelOverlaysRelease, OverlayType.Release);
            //BuildOverlayStackPanel(stackPanelOverlaysDebug, OverlayType.Debug);

            BuildOverlayListView(listOverlays, OverlayType.Release);
            BuildOverlayListView(listDebugOverlays, OverlayType.Debug);
        }

        private void BuildOverlayListView(ListView listView, OverlayType overlayType)
        {
            listView.Items.Clear();

            foreach (KeyValuePair<string, Type> x in OverlaysACC.AbstractOverlays)
            {
                object[] args = new object[] { new System.Drawing.Rectangle((int)SystemParameters.PrimaryScreenWidth / 2, (int)SystemParameters.PrimaryScreenHeight / 2, 300, 150) };

                AbstractOverlay tempAbstractOverlay = (AbstractOverlay)Activator.CreateInstance(x.Value, args);
                OverlaySettingsJson tempOverlaySettings = OverlaySettings.LoadOverlaySettings(tempAbstractOverlay.Name);
                tempAbstractOverlay.Dispose();

                var overlayAttribute = GetOverlayAttribute(x.Value);

                if (overlayAttribute.OverlayType != overlayType)
                    continue;

                TextBlock listViewText = new TextBlock() { Text = x.Key, Style = Resources["MaterialDesignButtonTextBlock"] as Style, };

                ListViewItem listViewItem = new ListViewItem()
                {
                    Content = listViewText,
                    DataContext = x
                };
                if (tempOverlaySettings != null)
                    if (tempOverlaySettings.Enabled)
                    {
                        listViewItem.Background = new SolidColorBrush(Color.FromArgb(50, 10, 255, 10));
                        lock (OverlaysACC.ActiveOverlays)
                        {
                            AbstractOverlay overlay = (AbstractOverlay)Activator.CreateInstance(x.Value, args);

                            overlay.Start();

                            SaveOverlaySettings(overlay, true);

                            OverlaysACC.ActiveOverlays.Add(overlay);
                        }
                    }

                listView.Items.Add(listViewItem);
            }
        }

        private void GeneratePreview(string overlayName)
        {
            OverlaysACC.AbstractOverlays.TryGetValue(overlayName, out Type overlayType);

            if (overlayType == null)
                return;

            AbstractOverlay overlay;
            try
            {
                overlay = (AbstractOverlay)Activator.CreateInstance(overlayType, DefaultOverlayArgs);
            }
            catch (Exception)
            {
                return;
            }

            ACCSharedMemory mem = new ACCSharedMemory();
            overlay.pageGraphics = mem.ReadGraphicsPageFile();
            overlay.pageGraphics.NumberOfLaps = 30;
            overlay.pageGraphics.FuelXLap = 3.012f;

            overlay.pagePhysics = mem.ReadPhysicsPageFile();
            overlay.pagePhysics.Fuel = 13.37f;
            overlay.pagePhysics.Rpms = 8500;
            overlay.pagePhysics.Gear = 3;
            overlay.pagePhysics.WheelPressure = new float[] { 27.6f, 27.5f, 26.9f, 26.1f };
            overlay.pagePhysics.TyreCoreTemperature = new float[] { 92.6f, 88.5f, 65.9f, 67.2f };
            overlay.pagePhysics.PadLife = new float[] { 24f, 24f, 25f, 25f };
            overlay.pagePhysics.BrakeTemperature = new float[] { 300f, 250f, 450f, 460f };

            overlay.pageStatic = mem.ReadStaticPageFile();
            overlay.pageStatic.MaxFuel = 120f;
            overlay.pageStatic.MaxRpm = 9250;
            overlay.pageStatic.CarModel = "porsche_991ii_gt3_r";

            try
            {
                overlay.BeforeStart();
                CachedPreview cachedPreview = new CachedPreview()
                {
                    Width = overlay.Width,
                    Height = overlay.Height,
                    CachedBitmap = new CachedBitmap(overlay.Width, overlay.Height, g => overlay.Render(g))
                };

                if (_cachedPreviews.ContainsKey(overlayName))
                    _cachedPreviews[overlayName] = cachedPreview;
                else
                    _cachedPreviews.Add(overlayName, cachedPreview);

                overlay.BeforeStop();
            }
            catch (Exception)
            {

            }
            finally
            {
                overlay.Dispose();
                overlay = null;
            }
        }

        private StackPanel GetConfigStacker(Type overlayType, Orientation orientation)
        {
            StackPanel stacker = new StackPanel()
            {
                Margin = new Thickness(10, 0, 0, 0),
                Orientation = orientation,
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
                            args.Handled = true;
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
                        Margin = new Thickness(0, 3, 5, 3),
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
                                Orientation = Orientation.Horizontal,
                                VerticalAlignment = VerticalAlignment.Center,
                                Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
                                ToolTip = "Right click to reset. Scroll Me.",
                            };
                            ToolTipService.SetShowDuration(sliderStacker, int.MaxValue);

                            double min = 0.5;
                            double max = 2.0;
                            double tickFrequency = 0.01;
                            double sliderValue = double.Parse(configField.Value.ToString());
                            sliderValue.Clip(min, max);

                            string floatLabel = string.Concat(configField.Name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

                            Label sliderLabel = new Label
                            {
                                Content = $"{floatLabel}: {sliderValue:F2}",
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
                                sliderLabel.Content = $"{floatLabel}: {slider.Value:F2}";
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
                                args.Handled = true;
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
                    _cachedPreviews.TryGetValue(actualOverlayName, out CachedPreview preview);
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
            AbstractOverlay tempOverlay = (AbstractOverlay)Activator.CreateInstance(overlay, DefaultOverlayArgs);
            string name = tempOverlay.Name;
            tempOverlay.Dispose();

            return name;
        }

        private OverlayAttribute GetOverlayAttribute(Type overlay)
        {
            OverlayAttribute overlayAttribute = null;
            try
            {
                overlayAttribute = overlay.GetCustomAttribute<OverlayAttribute>();
            }
            catch (Exception)
            {
            }
            return overlayAttribute;
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
