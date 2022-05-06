using ACCManager.HUD.ACC;
using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ACCManager.HUD.Overlay.Internal.OverlayConfiguration;
using static ACCManager.HUD.Overlay.Internal.OverlayOptions;

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

                StackPanel stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                CheckBox checkBox = new CheckBox() { Content = x.Key };
                stackPanel.Children.Add(checkBox);
                StackPanel configStacker = GetConfigStacker(x.Value);
                stackPanel.Children.Add(configStacker);

                checkBox.Checked += (s, e) =>
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

                checkBox.Unchecked += (s, e) =>
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
                OverlaySettings settings = OverlayOptions.LoadOverlaySettings(tempOverlay.Name);
                if (settings != null)
                {
                    if (settings.Enabled)
                    {
                        checkBox.IsChecked = true;
                    }
                }
                tempOverlay.Dispose();

                stackPanelOverlayCheckboxes.Children.Add(stackPanel);
            }
        }

        private StackPanel GetConfigStacker(Type overlayType)
        {
            StackPanel stacker = new StackPanel();
            OverlayConfiguration overlayConfig = GetOverlayConfig(overlayType);
            if (overlayConfig == null) return stacker;

            List<ConfigField> configFields = overlayConfig.GetConfigFields();

            foreach (PropertyInfo pi in overlayConfig.GetProperties())
            {
                if (pi.PropertyType.Name == typeof(bool).Name)
                {
                    ConfigField configField = configFields.Where(cf => cf.Name == pi.Name).First();
                    CheckBox box = new CheckBox() { Content = configField.Name, IsChecked = (bool)configField.Value };
                    box.Checked += (sender, args) =>
                    {
                        configField.Value = true;
                        configFields.RemoveAt(configFields.IndexOf(configField));
                        configFields.Add(configField);


                        string overlayName = GetOverlayName(overlayType);
                        OverlaySettings settings = OverlayOptions.LoadOverlaySettings(overlayName);
                        settings.Config = configFields;
                        OverlayOptions.SaveOverlaySettings(overlayName, settings);
                    };
                    box.Unchecked += (sender, args) =>
                    {
                        configField.Value = false;
                        configFields.RemoveAt(configFields.IndexOf(configField));
                        configFields.Add(configField);
                        OverlayConfiguration config = GetOverlayConfig(overlayType);
                        config.SetConfigFields(configFields);


                        string overlayName = GetOverlayName(overlayType);
                        OverlaySettings settings = OverlayOptions.LoadOverlaySettings(overlayName);
                        settings.Config = configFields;
                        OverlayOptions.SaveOverlaySettings(overlayName, settings);
                    };
                    stacker.Children.Add(box);
                }
            };

            return stacker;
        }

        private void SaveOverlaySettings(AbstractOverlay overlay, bool isEnabled)
        {
            OverlaySettings settings = OverlayOptions.LoadOverlaySettings(overlay.Name);
            if (settings == null)
            {
                settings = new OverlaySettings() { X = overlay.X, Y = overlay.Y };
            }

            settings.Enabled = isEnabled;

            OverlayOptions.SaveOverlaySettings(overlay.Name, settings);
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
            OverlaySettings settings = OverlayOptions.LoadOverlaySettings(overlay.Name);
            if (settings == null)
            {
                settings = new OverlaySettings() { X = overlay.X, Y = overlay.Y };
            }

            settings.Config = overlayConfiguration.GetConfigFields();

            OverlayOptions.SaveOverlaySettings(overlay.Name, settings);
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
