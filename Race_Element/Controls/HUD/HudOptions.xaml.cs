using Gma.System.MouseKeyHook;
using RaceElement.Controls.HUD;
using RaceElement.Controls.HUD.Controls;
using RaceElement.Controls.Util.SetupImage;
using RaceElement.Data.Games;
using RaceElement.HUD.ACC;
using RaceElement.HUD.ACC.Overlays.OverlayMousePosition;
using RaceElement.HUD.Common;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.Util;
using RaceElement.Util.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;
using static RaceElement.HUD.Overlay.Configuration.OverlaySettings;

// https://github.com/gmamaladze/globalmousekeyhook
namespace RaceElement.Controls;

/// <summary>
/// Interaction logic for HudOptions.xaml
/// </summary>
public partial class HudOptions : UserControl
{
    private IKeyboardMouseEvents m_GlobalHook;

    public static HudOptions Instance { get; private set; }

    private HudSettings _hudSettings;
    private HudSettingsJson _hudSettingsJson;

    private readonly object[] DefaultOverlayArgs = [new System.Drawing.Rectangle((int)SystemParameters.PrimaryScreenWidth / 2, (int)SystemParameters.PrimaryScreenHeight / 2, 300, 150)];

    private DateTime _lastMovementModeChange = DateTime.MinValue;
    private const int MovementModeDebounce = 250;

    public HudOptions()
    {
        InitializeComponent();

        Dispatcher.BeginInvoke(new Action(() =>
        {
            _hudSettings = new HudSettings();
            _hudSettingsJson = _hudSettings.Get();

            ComboBoxHudReset.SelectionChanged += ComboBoxHudReset_SelectionChanged;
            ToolTipService.SetInitialShowDelay(ComboBoxHudReset, 0);

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
                titleBar.Children.Clear();

                previewImage.Source = null;
                listDebugOverlays.SelectedIndex = -1;
                listOverlays.SelectedIndex = -1;
            };

            GameManager.OnGameChanged += (s, newGame) =>
            {
                BuildOverlayPanel();
            };

            bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
                try
                {
                    PopulateCategoryCombobox(comboOverlays, listOverlays, OverlayType.Drive);
                    PopulateCategoryCombobox(comboDebugOverlays, listDebugOverlays, OverlayType.Pitwall);
                    BuildOverlayPanel();

                    ToolTipService.SetInitialShowDelay(listBoxItemToggleDemoMode, 1);
                    ToolTipService.SetInitialShowDelay(listBoxItemToggleMovementMode, 1);

                    listBoxItemToggleMovementMode.PreviewMouseDown += (s, e) =>
                    {
                        if (_lastMovementModeChange.AddMilliseconds(MovementModeDebounce) > DateTime.Now)
                            e.Handled = true;
                    };
                    listBoxItemToggleMovementMode.Selected += (s, e) =>
                    {
                        _lastMovementModeChange = DateTime.Now;

                        SetRepositionMode(true);
                        listBoxItemToggleMovementMode.Foreground = Brushes.LimeGreen;
                    };
                    listBoxItemToggleMovementMode.Unselected += (s, e) =>
                    {
                        _lastMovementModeChange = DateTime.Now;

                        SetRepositionMode(false);
                        listBoxItemToggleMovementMode.Foreground = Brushes.DarkGray;
                    };

                    // middle button to activate reposition mode
                    this.MouseUp += (s, e) =>
                    {
                        if (e.ChangedButton == MouseButton.Middle && _lastMovementModeChange.AddMilliseconds(MovementModeDebounce) < DateTime.Now)
                        {
                            e.Handled = true;
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                this.listBoxItemToggleMovementMode.IsSelected = !this.listBoxItemToggleMovementMode.IsSelected;
                            }));
                        }
                    };


                    listBoxItemToggleDemoMode.Selected += (s, e) =>
                    {
                        _hudSettingsJson.DemoMode = true;
                        _hudSettings.Save(_hudSettingsJson);
                        listBoxItemToggleDemoMode.Foreground = Brushes.Cyan;
                    };
                    listBoxItemToggleDemoMode.Unselected += (s, e) =>
                    {
                        _hudSettingsJson.DemoMode = false;
                        _hudSettings.Save(_hudSettingsJson);
                        listBoxItemToggleDemoMode.Foreground = Brushes.DarkGray;
                    };

                    // double click to activate overlays in the lists
                    listOverlays.MouseDoubleClick += (s, e) => { if (ToggleViewingOverlay()) e.Handled = true; };
                    listOverlays.MouseLeftButtonDown += (s, e) => { if (ToggleViewingOverlay()) e.Handled = true; };
                    listDebugOverlays.MouseDoubleClick += (s, e) => { if (ToggleViewingOverlay()) e.Handled = true; };

                    m_GlobalHook = Hook.GlobalEvents();
                    m_GlobalHook.OnCombination(new Dictionary<Combination, Action> {
                        { Combination.FromString("Control+Home"), () => {
                            if (_lastMovementModeChange.AddMilliseconds(MovementModeDebounce) < DateTime.Now) {
                                Dispatcher.BeginInvoke(new Action(() => {
                                    this.listBoxItemToggleMovementMode.IsSelected = !this.listBoxItemToggleMovementMode.IsSelected;
                                }));
                            }
                        }}
                    });

                    this.PreviewKeyDown += HudOptions_PreviewKeyDown;
                    this.PreviewMouseDown += HudOptions_PreviewMouseDown;

                    listBoxItemToggleDemoMode.IsSelected = _hudSettingsJson.DemoMode;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    LogWriter.WriteToLog(ex);
                }
        }));

        Instance = this;
    }

    private void ComboBoxHudReset_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxHudReset.SelectedIndex == 0) { e.Handled = true; return; }

        // index 1 = reset position
        // index 2 = reset config

        string currentlyViewedOverlay = GetCurrentlyViewedOverlayName();
        if (currentlyViewedOverlay == string.Empty)
        {
            MainWindow.Instance.EnqueueSnackbarMessage("To reset HUD properties first select a HUD in the list.");
            ComboBoxHudReset.SelectedIndex = 0; return;
        }

        switch (ComboBoxHudReset.SelectedIndex)
        {
            case 1:
                {
                    ConfigurationControls.ResetConfigurationPosition(currentlyViewedOverlay);
                    break;
                }
            case 2:
                {
                    if (!ConfigurationControls.ResetConfigurationFields(currentlyViewedOverlay))
                        break;

                    configStackPanel.Children.Clear();

                    ListView listView = tabControlListOverlays.SelectedIndex == 0 ? listOverlays : listDebugOverlays;
                    ListViewItem selected = (ListViewItem)listView.SelectedItem;
                    KeyValuePair<string, Type> kv = (KeyValuePair<string, Type>)selected.DataContext;
                    BuildOverlayConfigPanel(selected, kv.Value);
                    PreviewCache.UpdatePreviewImage(listOverlays, previewImage, currentlyViewedOverlay);
                    MainWindow.Instance.EnqueueSnackbarMessage($"Config has been reset for {currentlyViewedOverlay} HUD.");
                    break;
                }
            default: break;
        }

        ComboBoxHudReset.SelectedIndex = 0;
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
        if (listBoxItemToggleMovementMode.IsSelected)
            return false;

        // kind of stupid but it works.. gotta travel the generated tree in #BuildOverlayConfigPanel();
        if (_lastOverlayStart.AddMilliseconds(200) < DateTime.Now)
            foreach (UIElement element in titleBar.Children)
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
        foreach (UIElement element in titleBar.Children)
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
        {
            configStackPanel.Children.Clear();
            titleBar.Children.Clear();
        }
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
        {
            configStackPanel.Children.Clear();
            titleBar.Children.Clear();
        }
    }

    private bool _collectingGarbage = false;
    private void CollectGarbage()
    {
        if (!_collectingGarbage)
            ThreadPool.QueueUserWorkItem(x =>
            {
                _collectingGarbage = true;
                Debug.WriteLine("Collecting garbage");
                Thread.Sleep(10 * 1000);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false, true);
                _collectingGarbage = false;
            });
    }

    private void BuildOverlayConfigPanel(ListViewItem listViewItem, Type type)
    {

        cardOverlayConfig.Visibility = Visibility.Collapsed;

        CollectGarbage();

        configStackPanel.Children.Clear();
        titleBar.Children.Clear();

        OverlayAttribute overlayAttribute = GetOverlayAttribute(type);
        OverlaySettingsJson tempOverlaySettings = OverlaySettings.LoadOverlaySettings(overlayAttribute.Name);

        PreviewCache.GeneratePreview(overlayAttribute.Name, true);

        StackPanel configStacker = GetConfigStacker(type);

        Label overlayNameLabel = new()
        {
            Content = overlayAttribute.Name,
            FontFamily = FindResource("Conthrax") as FontFamily,
            BorderBrush = Brushes.OrangeRed,
            BorderThickness = new Thickness(0, 0, 0, 1.5),
            Margin = new Thickness(0, 0, 0, 5),
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 32,
            Foreground = Brushes.White
        };
        TextBlock overlayDescription = new()
        {
            Text = overlayAttribute.Description,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            FontSize = 13.5,
            Margin = new Thickness(0, 0, 0, 2),
        };
        StackPanel stackerOverlayInfo = new()
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 0, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(140, 0, 0, 0)),
        };


        stackerOverlayInfo.Children.Add(overlayNameLabel);
        stackerOverlayInfo.Children.Add(overlayDescription);

        StackPanel activationPanel = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = Cursors.Hand,
            Name = "activationStacker",
            Margin = new Thickness(0, 0, 0, 0),
        };
        ToggleButton toggle = new()
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

            lock (OverlaysAcc.ActiveOverlays)
            {
                AbstractOverlay overlay = OverlaysAcc.ActiveOverlays.Find(f => f.GetType() == type);

                if (overlay == null)
                {

                    overlayNameLabel.BorderBrush = Brushes.Green;
                    listViewItem.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
                    listViewItem.BorderBrush = new SolidColorBrush(Colors.LimeGreen);
                    overlay = (AbstractOverlay)Activator.CreateInstance(type, DefaultOverlayArgs);

                    overlay.Start();

                    SaveOverlaySettings(overlay, true);

                    configStacker.IsEnabled = false;

                    if (OverlaysAcc.ActiveOverlays.FindIndex(o => o.Name == overlay.Name) == -1)
                        OverlaysAcc.ActiveOverlays.Add(overlay);
                }
            }
        };
        toggle.Unchecked += (s, e) =>
        {
            toggle.Background = Brushes.Transparent;

            stackerOverlayInfo.Background = new SolidColorBrush(Color.FromArgb(140, 0, 0, 0));
            overlayNameLabel.BorderBrush = Brushes.OrangeRed;
            overlayNameLabel.Foreground = Brushes.White;

            lock (OverlaysAcc.ActiveOverlays)
            {
                listViewItem.Background = Brushes.Transparent;
                listViewItem.BorderBrush = new SolidColorBrush(Colors.Transparent);
                AbstractOverlay overlay = OverlaysAcc.ActiveOverlays.Find(f => f.GetType() == type);

                SaveOverlaySettings(overlay, false);



                int index = OverlaysAcc.ActiveOverlays.FindIndex(o => o.Name == overlay.Name);
                if (index != -1)
                    OverlaysAcc.ActiveOverlays.RemoveAt(index);
                Task.Run(() =>
                {
                    overlay?.Stop();
                });
                configStacker.IsEnabled = true;
            }
        };
        activationPanel.PreviewMouseLeftButtonDown += (s, e) => toggle.IsChecked = !toggle.IsChecked;

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
        titleBar.Children.Add(activationPanel);
        titleBar.Children.Add(stackerOverlayInfo);

        // click overlay title/description to toggle overlay
        stackerOverlayInfo.Cursor = Cursors.Hand;
        stackerOverlayInfo.PreviewMouseDown += (s, e) =>
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
        cardOverlayConfig.Visibility = Visibility.Visible;
    }

    public void DisposeKeyboardHooks()
    {
        if (m_GlobalHook != null)
            m_GlobalHook.Dispose();
    }

    internal MousePositionOverlay mousePositionOverlay;
    private void SetRepositionMode(bool enabled)
    {
        gridOverlays.IsEnabled = !enabled;
        cardOverlayConfig.IsEnabled = !enabled;

        if (enabled)
        {
            mousePositionOverlay ??= new MousePositionOverlay(new System.Drawing.Rectangle(0, 0, 150, 150), "Mouse Position");
            mousePositionOverlay.Start(false);
        }
        else
        {
            if (mousePositionOverlay != null)
                mousePositionOverlay.Stop();
        }

        lock (OverlaysAcc.ActiveOverlays)
            foreach (AbstractOverlay overlay in OverlaysAcc.ActiveOverlays)
                overlay.EnableReposition(enabled);
    }

    private void BuildOverlayPanel()
    {
        OverlaysAcc.GenerateDictionary();
        CommonHuds.GenerateDictionary();

        BuildOverlayListView(listOverlays, OverlayType.Drive, (OverlayCategory)((ComboBoxItem)comboOverlays.SelectedItem).DataContext);
        BuildOverlayListView(listDebugOverlays, OverlayType.Pitwall, (OverlayCategory)((ComboBoxItem)comboOverlays.SelectedItem).DataContext);
    }

    private void PopulateCategoryCombobox(ComboBox box, ListView listView, OverlayType overlayType)
    {
        box.Items.Clear();

        foreach (OverlayCategory category in Enum.GetValues(typeof(OverlayCategory)))
        {
            ComboBoxItem item = new()
            {
                Content = string.Concat(category.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '),
                DataContext = category,
                FontWeight = FontWeights.Bold,
            };
            box.Items.Add(item);
        }

        box.SelectedIndex = 0;

        box.SelectionChanged += (s, e) =>
        {
            BuildOverlayListView(listView, overlayType, (OverlayCategory)((ComboBoxItem)box.SelectedItem).DataContext);
        };
    }

    private void BuildOverlayListView(ListView listView, OverlayType overlayType, OverlayCategory category)
    {
        listView.Items.Clear();

        SortedDictionary<string, Type> abstractOverlays = [];

        if (GameManager.CurrentGame != Game.AssettoCorsaCompetizione)
        {
            OverlaysAcc.CloseAll();
            abstractOverlays = CommonHuds.AbstractOverlays;
        }
        else
        {
            CommonHuds.CloseAll();
            OverlaysAcc.GenerateDictionary();
            abstractOverlays = OverlaysAcc.AbstractOverlays;
        }

        foreach (KeyValuePair<string, Type> x in abstractOverlays)
        {
            AbstractOverlay tempAbstractOverlay = (AbstractOverlay)Activator.CreateInstance(x.Value, DefaultOverlayArgs);
            var overlayAttribute = GetOverlayAttribute(x.Value);
            if (overlayAttribute.OverlayCategory != category && category != OverlayCategory.All)
                continue;

            OverlaySettingsJson tempOverlaySettings = OverlaySettings.LoadOverlaySettings(overlayAttribute.Name);
            tempAbstractOverlay.Dispose();

            if (overlayAttribute.OverlayType != overlayType)
                continue;

            double marginTopBottom = 6.5d;
            ListViewItem listViewItem = new()
            {
                Content = new TextBlock()
                {
                    Text = x.Key,
                    Style = Resources["MaterialDesignButtonTextBlock"] as Style,
                    Margin = new Thickness(14, 0, 0, 0),
                    FontSize = 13.8,
                },
                DataContext = x,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(0, marginTopBottom, 0, marginTopBottom),
                Margin = new Thickness(0, 0.5, 0, 0.5),
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(4, 0, 0, 0),
            };
            if (overlayAttribute.Description != string.Empty)
            {
                StringBuilder tooltipBuilder = new StringBuilder(overlayAttribute.Description);
                string toolTip = overlayAttribute.Description;
                if (overlayAttribute.Authors.Length > 0)
                {
                    tooltipBuilder.Append("\n\nAuthors: ");
                    for (int i = 0; i < overlayAttribute.Authors.Length; i++)
                    {
                        tooltipBuilder.Append(overlayAttribute.Authors[i]);

                        tooltipBuilder.Append($"{(i < overlayAttribute.Authors.Length - 1 ? ", " : ".")}");
                    }
                }

                listViewItem.ToolTip = tooltipBuilder.ToString();
                ToolTipService.SetPlacement(listViewItem, PlacementMode.Right);
                ToolTipService.SetInitialShowDelay(listViewItem, 0);
            }

            if (tempOverlaySettings != null)
                if (tempOverlaySettings.Enabled)
                {
                    listViewItem.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
                    listViewItem.BorderBrush = new SolidColorBrush(Colors.LimeGreen);

                    lock (OverlaysAcc.ActiveOverlays)
                    {
                        AbstractOverlay overlay = (AbstractOverlay)Activator.CreateInstance(x.Value, DefaultOverlayArgs);
                        if (OverlaysAcc.ActiveOverlays.FindIndex(o => o.Name == overlay.Name) == -1)
                        {
                            SaveOverlaySettings(overlay, true);
                            OverlaysAcc.ActiveOverlays.Add(overlay);
                            overlay.Start();
                        }
                        else
                        {
                            overlay.Dispose();
                        }
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
        StackPanel stacker = new()
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

        configFields ??= OverlayConfiguration.GetConfigFields(overlayConfig);


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
                //Debug.WriteLine($"{type.Name} -  {type.ReflectedType.FullName} - {type.PropertyType.Name}");

                ListView listView = new()
                {
                    Width = 550,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 5),
                    Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                };

                listView.Items.Add(new ListViewItem()
                {
                    Content = $" {cga.Title}",
                    Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    //FontWeight = FontWeights.Bold,
                    //FontStyle = FontStyles.Italic,
                    FontFamily = FindResource("Conthrax") as FontFamily,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    FontSize = 18,
                    ToolTip = $"{cga.Description}",
                    Focusable = false,
                    Margin = new Thickness(0),
                    Padding = new Thickness(4, 2, 0, 2),
                    BorderThickness = new Thickness(1, 0, 0, 0),
                    BorderBrush = Brushes.OrangeRed
                });



                // hide Rescale option when disabled in the HUD code itself.
                bool allowsRescaling = false;
                if (type.PropertyType == typeof(GenericConfig))
                {
                    var field = type.PropertyType.GetField("AllowRescale");
                    if (field != null)
                    {
                        var typeValue = type.GetValue(overlayConfig);
                        object value = field.GetValue(typeValue);
                        if (bool.TryParse(value.ToString(), out bool result))
                            allowsRescaling = result;
                    }
                }

                foreach (PropertyInfo subType in type.PropertyType.GetProperties())
                {
                    ConfigField configField = configFields.Where(x => x.Name == $"{type.Name}.{subType.Name}").FirstOrDefault();
                    if (configField == null)
                    {
                        var typeValue = type.GetValue(overlayConfig);
                        configField = new ConfigField() { Name = $"{type.Name}.{subType.Name}", Value = subType.GetValue(typeValue).ToString() };
                    }

                    // hide Rescale option when disabled in the HUD code itself. (null the configField so it won't be added)
                    if (!allowsRescaling && type.PropertyType.Name.Equals(typeof(GenericConfig).Name))
                        if (configField.Name == "GenericConfiguration.Scale")
                            configField = null;


                    if (configField != null)
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            listView.Items.Add(ControlFactory.Instance.GenerateOption($"{type.Name}", $"{subType.Name}", subType, configField));
                        }));
                }

                stacker.Children.Add(listView);
            }
        }

        return stacker;
    }

    private void SaveOverlaySettings(AbstractOverlay overlay, bool isEnabled)
    {
        OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(overlay.Name);
        settings ??= new OverlaySettingsJson() { X = overlay.X, Y = overlay.Y };

        settings.Enabled = isEnabled;

        OverlaySettings.SaveOverlaySettings(overlay.Name, settings);
    }

    private void SaveOverlayConfig(Type overlay, OverlayConfiguration overlayConfiguration)
    {
        object[] args = [new System.Drawing.Rectangle(0, 0, 300, 150)];
        AbstractOverlay tempOverlay = (AbstractOverlay)Activator.CreateInstance(overlay, args);
        SaveOverlayConfig(tempOverlay, overlayConfiguration);
        tempOverlay.Dispose();
    }

    private void SaveOverlayConfig(AbstractOverlay overlay, OverlayConfiguration overlayConfiguration)
    {
        OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(overlay.Name);
        settings ??= new OverlaySettingsJson() { X = overlay.X, Y = overlay.Y };

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
        object[] args = [new System.Drawing.Rectangle(0, 0, 300, 150)];
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
