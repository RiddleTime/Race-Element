using ACC_Manager.Util.Settings;
using ACC_Manager.Util.SystemExtensions;
using ACCManager.Controls.HUD;
using ACCManager.Controls.HUD.Controls;
using ACCManager.Controls.Util.SetupImage;
using ACCManager.HUD.ACC;
using ACCManager.HUD.ACC.Overlays.OverlayMousePosition;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
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

                            if (e.ChangedButton == MouseButton.Right)
                            {
                                // activate selected hud
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
                        this.PreviewMouseDown += HudOptions_PreviewMouseDown;

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

        private void HudOptions_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (ToggleViewingOverlay())
                    e.Handled = true;
            }
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

        public string GetCurrentlyViewedOverlayName()
        {
            foreach (UIElement element in configStackPanel.Children)
                if (element is StackPanel panel)
                    foreach (UIElement child in panel.Children)
                        if (child is Label label)
                            return label.Content.ToString();

            return string.Empty;
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
            OverlayAttribute overlayAttribute = GetOverlayAttribute(type);
            AbstractOverlay tempAbstractOverlay = (AbstractOverlay)Activator.CreateInstance(type, DefaultOverlayArgs);
            OverlaySettingsJson tempOverlaySettings = OverlaySettings.LoadOverlaySettings(overlayAttribute.Name);
            tempAbstractOverlay.Dispose();

            PreviewCache.GeneratePreview(overlayAttribute.Name, true);

            StackPanel configStacker = GetConfigStacker(type);

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
                var overlayAttribute = GetOverlayAttribute(x.Value);
                OverlaySettingsJson tempOverlaySettings = OverlaySettings.LoadOverlaySettings(overlayAttribute.Name);
                tempAbstractOverlay.Dispose();


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

        internal OverlayConfiguration overlayConfig;
        internal List<ConfigField> configFields;
        /// <summary>
        /// NEEDS REFACTOR
        ///   decouple
        ///   generics
        /// </summary>
        /// <param name="overlayType"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        private StackPanel GetConfigStacker(Type overlayType)
        {
            StackPanel stacker = new StackPanel()
            {
                Margin = new Thickness(10, 0, 0, 0),
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
            };

            overlayConfig = GetOverlayConfig(overlayType);
            if (overlayConfig == null) return stacker;

            string overlayName = GetOverlayName(overlayType);

            configFields = null;
            OverlaySettingsJson overlaySettings = OverlaySettings.LoadOverlaySettings(overlayName);
            if (overlaySettings == null)
                configFields = OverlayConfiguration.GetConfigFields(overlayConfig);
            else
                configFields = overlaySettings.Config;

            if (configFields == null)
                configFields = OverlayConfiguration.GetConfigFields(overlayConfig);


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

                    ListView listView = new ListView()
                    {
                        Width = 360,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 0, 0, 5),
                        Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                    };

                    listView.Items.Add(new ListViewItem()
                    {
                        Content = $" {cga.Title}",
                        Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        FontWeight = FontWeights.Bold,
                        FontStyle = FontStyles.Italic,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        FontSize = 18,
                        ToolTip = $"{cga.Description}",
                        Focusable = false,
                        Margin = new Thickness(0),
                        Padding = new Thickness(2, 2, 0, 2),
                        BorderThickness = new Thickness(1, 1, 0, 0),
                        BorderBrush = Brushes.OrangeRed
                    });

                    foreach (PropertyInfo subType in type.PropertyType.GetProperties())
                    {
                        ConfigField configField = configFields.Where(x => x.Name == $"{type.Name}.{subType.Name}").FirstOrDefault();
                        if (configField == null)
                        {
                            var typeValue = type.GetValue(overlayConfig);
                            configField = new ConfigField() { Name = $"{type.Name}.{subType.Name}", Value = subType.GetValue(typeValue).ToString() };
                        }

                        if (configField != null)
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                listView.Items.Add(ControlFactory.Instance.GenerateOption($"{type.Name}", $"{subType.Name}", subType, configField));
                            }));
                        }
                    }

                    stacker.Children.Add(listView);
                }
            }

            return stacker;
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

            settings.Config = OverlayConfiguration.GetConfigFields(overlayConfiguration);

            OverlaySettings.SaveOverlaySettings(overlay.Name, settings);
        }

        private string GetOverlayName(Type overlay)
        {
            var attribute = GetOverlayAttribute(overlay);

            if (attribute == null)
                return String.Empty;

            return attribute.Name;
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
