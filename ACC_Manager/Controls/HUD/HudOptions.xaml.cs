using ACCManager.HUD.ACC;
using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
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

                CheckBox checkBox = new CheckBox() { Content = x.Key };

                checkBox.Checked += (s, e) =>
                {
                    lock (OverlaysACC.ActiveOverlays)
                    {
                        AbstractOverlay overlay = (AbstractOverlay)Activator.CreateInstance(x.Value, args);
                        overlay.Start();

                        SaveOverlaySettings(overlay, true);

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

                stackPanelOverlayCheckboxes.Children.Add(checkBox);
            }
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
    }
}
