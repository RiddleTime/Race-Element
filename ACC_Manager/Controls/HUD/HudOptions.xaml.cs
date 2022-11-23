using ACC_Manager.Util.Settings;
using ACC_Manager.Util.SystemExtensions;
using ACCManager.Controls.HUD;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        public static HudOptions Instance { get; private set; }

        private HudSettings _hudSettings;
        private HudSettingsJson _hudSettingsJson;

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

            Dispatcher.BeginInvoke(new Action(() =>
            {
                _hudSettings = new HudSettings();
                _hudSettingsJson = _hudSettings.Get();


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
                            gridRepositionToggler.Background = new SolidColorBrush(Color.FromArgb(47, 69, 255, 00));
                        };
                        checkBoxReposition.Unchecked += (s, e) =>
                        {
                            SetRepositionMode(false);
                            gridRepositionToggler.Background = new SolidColorBrush(Color.FromArgb(47, 255, 69, 00));
                        };

                        checkBoxDemoMode.Checked += (s, e) =>
                        {
                            _hudSettingsJson.DemoMode = true;
                            _hudSettings.Save(_hudSettingsJson);

                            gridDemoToggler.Background = new SolidColorBrush(Color.FromArgb(47, 69, 255, 00));
                        };

                        checkBoxDemoMode.Unchecked += (s, e) =>
                        {
                            _hudSettingsJson.DemoMode = false;
                            _hudSettings.Save(_hudSettingsJson);
                            gridDemoToggler.Background = new SolidColorBrush(Color.FromArgb(47, 255, 69, 00));
                        };

                        this.PreviewMouseUp += (s, e) =>
                        {
                            if (e.ChangedButton == MouseButton.Middle)
                            {
                                this.checkBoxReposition.IsChecked = !this.checkBoxReposition.IsChecked;
                                e.Handled = true;
                            }
                        };

                        listOverlays.MouseDoubleClick += (s, e) => { if (ToggleViewingOverlay()) e.Handled = true; };
                        listDebugOverlays.MouseDoubleClick += (s, e) => { if (ToggleViewingOverlay()) e.Handled = true; };

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


                        checkBoxDemoMode.IsChecked = _hudSettingsJson.DemoMode;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        LogWriter.WriteToLog(ex);
                    }
            }));
            Instance = this;
        }


        private DateTime _lastOverlayStart = DateTime.MinValue;
        private void HudOptions_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (ToggleViewingOverlay())
                    e.Handled = true;
        }

        private bool ToggleViewingOverlay()
        {
            // kind of stupid but it works.. gotta travel the generated tree in #BuildOverlayConfigPanel();

            if (_lastOverlayStart.AddMilliseconds(200) < DateTime.Now)
                foreach (UIElement element in configStackPanel.Children)
                    if (element is StackPanel panel)
                        foreach (UIElement child in panel.Children)
                            if (child is ToggleButton toggle)
                            {
                                toggle.IsChecked = !toggle.IsChecked;
                                _lastOverlayStart = DateTime.Now;
                                return true;
                            }
            return false;
        }

        private void ListOverlays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ListViewItem listView = (ListViewItem)e.AddedItems[0];
                KeyValuePair<string, Type> kv = (KeyValuePair<string, Type>)listView.DataContext;
                BuildOverlayConfigPanel(listView, kv.Value);
                e.Handled = true;
            }
            else
                configStackPanel.Children.Clear();
        }

        private void ListDebugOverlays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ListViewItem listView = (ListViewItem)e.AddedItems[0];
                KeyValuePair<string, Type> kv = (KeyValuePair<string, Type>)listView.DataContext;
                BuildOverlayConfigPanel(listView, kv.Value);
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
            PreviewCache.GeneratePreview(overlayAttribute.Name, true);

            StackPanel configStacker = GetConfigStacker(type, Orientation.Vertical);

            Label overlayNameLabel = new Label()
            {
                Content = overlayAttribute.Name,
                FontFamily = FindResource("FontRedemption") as FontFamily,
                BorderBrush = Brushes.OrangeRed,
                BorderThickness = new Thickness(0, 0, 0, 1.5),
                Margin = new Thickness(0, -1, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 30,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.White
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
                Margin = new Thickness(7, 3, 7, 7),
                Background = new SolidColorBrush(Color.FromArgb(140, 0, 0, 0)),
            };

            stackerOverlayInfo.Children.Add(overlayNameLabel);
            stackerOverlayInfo.Children.Add(overlayDescription);

            StackPanel activationPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand,
                Name = "activationStacker",
                Margin = new Thickness(0, 0, 0, 0),
            };
            ToggleButton toggle = new ToggleButton()
            {
                Visibility = Visibility.Collapsed,
                VerticalAlignment = VerticalAlignment.Center
            };
            toggle.PreviewKeyDown += (s, e) => { if (e.Key == Key.Enter) e.Handled = true; };
            toggle.Checked += (s, e) =>
            {
                toggle.Background = Brushes.Green;

                stackerOverlayInfo.Background = new SolidColorBrush(Color.FromArgb(140, 0, 0, 0));
                overlayNameLabel.Foreground = Brushes.LimeGreen;

                lock (OverlaysACC.ActiveOverlays)
                {
                    AbstractOverlay overlay = OverlaysACC.ActiveOverlays.Find(f => f.GetType() == type);

                    if (overlay == null)
                    {

                        overlayNameLabel.BorderBrush = Brushes.Green;
                        listViewItem.Background = new SolidColorBrush(Color.FromArgb(50, 10, 255, 10));
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
                toggle.Background = Brushes.Transparent;

                stackerOverlayInfo.Background = new SolidColorBrush(Color.FromArgb(140, 0, 0, 0));
                overlayNameLabel.BorderBrush = Brushes.OrangeRed;
                overlayNameLabel.Foreground = Brushes.White;

                lock (OverlaysACC.ActiveOverlays)
                {
                    listViewItem.Background = Brushes.Transparent;
                    AbstractOverlay overlay = OverlaysACC.ActiveOverlays.Find(f => f.GetType() == type);

                    SaveOverlaySettings(overlay, false);

                    Task.Run(() =>
                    {
                        overlay?.Stop(true);
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
                overlayNameLabel.BorderBrush = Brushes.Green;
                overlayNameLabel.Foreground = Brushes.LimeGreen;
            }

            stackerOverlayInfo.MouseEnter += (s, e) =>
            {
                stackerOverlayInfo.Background = stackerOverlayInfo.Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0));

                switch (toggle.IsChecked)
                {
                    case true:
                        {
                            overlayNameLabel.BorderBrush = Brushes.OrangeRed;
                            stackerOverlayInfo.Background = stackerOverlayInfo.Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0)); ;
                            break;
                        }
                    case false:
                        {
                            overlayNameLabel.BorderBrush = Brushes.LimeGreen;
                            stackerOverlayInfo.Background = stackerOverlayInfo.Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0)); ;
                            break;
                        }
                }
            };
            stackerOverlayInfo.MouseLeave += (s, e) =>
            {
                stackerOverlayInfo.Background = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));

                switch (toggle.IsChecked)
                {
                    case true:
                        {
                            overlayNameLabel.BorderBrush = Brushes.Green;
                            break;
                        }
                    case false:
                        {
                            overlayNameLabel.BorderBrush = Brushes.OrangeRed;
                            break;
                        }
                }
            };

            activationPanel.Children.Add(toggle);
            configStackPanel.Children.Add(activationPanel);
            configStackPanel.Children.Add(stackerOverlayInfo);

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
            PreviewCache._cachedPreviews.TryGetValue(overlayAttribute.Name, out PreviewCache.CachedPreview preview);
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

                TextBlock listViewText = new TextBlock() { Text = x.Key, Style = Resources["MaterialDesignButtonTextBlock"] as Style };


                double marginTopBottom = 6.5d;

                ListViewItem listViewItem = new ListViewItem()
                {
                    Content = listViewText,
                    DataContext = x,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Padding = new Thickness(0, marginTopBottom, 0, marginTopBottom),
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

        private StackPanel GetConfigStacker(Type overlayType, Orientation orientation)
        {
            int fontSize = 14;

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





            // discover classes (Config Groupings) in OverlaySettings class
            PropertyInfo[] types = overlayConfig.GetType().GetProperties().Where(x => x.IsDefined(typeof(ConfigGroupingAttribute))).ToArray();
            foreach (PropertyInfo type in types)
            {

                ConfigGroupingAttribute cga = null;
                foreach (Attribute cad in Attribute.GetCustomAttributes(type))
                    if (cad is ConfigGroupingAttribute attribute)
                        cga = attribute;

                if (cga != null)
                {
                    Debug.WriteLine($"{type.Name} -  {type.ReflectedType.FullName} - {type.PropertyType.Name}");

                    StackPanel boxStacker = new StackPanel() { Orientation = Orientation.Vertical };
                    GroupBox box = new GroupBox()
                    {
                        Header = $"{cga.Title} - {cga.Description}",
                        Content = boxStacker,
                        Background = Brushes.Black,
                        Foreground = Brushes.Black,
                        Margin = new Thickness(2),
                    };
                    foreach (PropertyInfo subType in type.PropertyType.GetProperties())
                    {
                        // Add control elements here..
                        //boxStacker.Children.Add(CreateUserControls(subType, configFields, type.Name, fontSize, overlayName));
                        Debug.WriteLine($"   {subType.Name} -  {subType.ReflectedType.FullName} - {subType.PropertyType.Name}");
                    }

                    stacker.Children.Add(box);
                }
            }

            // translate properties into user controls using the given fields
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
                                FontSize = fontSize
                            };
                            sliderStacker.Children.Add(sliderLabel);

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

        private StackPanel CreateUserControls(PropertyInfo pi, List<ConfigField> configFields, string configFieldGrouping, int fontSize, string overlayName)
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
                return checkStacker;
            }
            return null;
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
                    PreviewCache.GeneratePreview(actualOverlayName);
                    PreviewCache._cachedPreviews.TryGetValue(actualOverlayName, out PreviewCache.CachedPreview preview);
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
