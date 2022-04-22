using ACCSetupApp.Controls.HUD.Overlay;
using ACCSetupApp.Controls.HUD.Overlay.Internal;
using ACCSetupApp.Controls.HUD.Overlay.OverlayStaticInfo;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for HudOptions.xaml
    /// </summary>
    public partial class HudOptions : UserControl
    {
        List<AbstractOverlay> overlays = new List<AbstractOverlay>();

        public HudOptions()
        {
            InitializeComponent();

            checkboxInputTrace.Checked += CheckboxInputTrace_Checked;
            checkboxInputTrace.Unchecked += CheckboxInputTrace_Unchecked;

            checkboxStaticData.Checked += CheckboxStaticData_Checked;
            checkboxStaticData.Unchecked += CheckboxStaticData_Unchecked;
        }

        private void CheckboxStaticData_Unchecked(object sender, RoutedEventArgs e)
        {
            AbstractOverlay overlay = overlays.Find(x => x.GetType() == typeof(StaticInfoOverlay));
            overlay?.Stop();
            overlays.Remove(overlay);
        }

        private void CheckboxStaticData_Checked(object sender, RoutedEventArgs e)
        {
            int width = 300;
            int x = 0;

            AbstractOverlay overlay = new StaticInfoOverlay(x, 0, width, 150);
            overlay.Start();
            overlays.Add(overlay);
        }

        private void CheckboxInputTrace_Unchecked(object sender, RoutedEventArgs e)
        {
            AbstractOverlay inputTrace = overlays.Find(x => x.GetType() == typeof(InputTraceOverlay));
            inputTrace?.Stop();
            overlays.Remove(inputTrace);
        }

        private void CheckboxInputTrace_Checked(object sender, RoutedEventArgs e)
        {
            int width = 300;
            int x = (int)(System.Windows.SystemParameters.FullPrimaryScreenWidth / 2 + (width * 1.5));

            AbstractOverlay inputTrace = new InputTraceOverlay(x, 0, width, 150);
            inputTrace.Start();
            overlays.Add(inputTrace);
        }
    }
}
