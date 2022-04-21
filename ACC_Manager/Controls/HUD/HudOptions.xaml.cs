using ACCSetupApp.Controls.HUD.Overlay;
using ACCSetupApp.Controls.HUD.Overlay.Internal;
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
        AbstractOverlay overlayInputTrace;

        public HudOptions()
        {
            InitializeComponent();

            checkboxInputTrace.Checked += CheckboxInputTrace_Checked;
            checkboxInputTrace.Unchecked += CheckboxInputTrace_Unchecked;
        }

        private void CheckboxInputTrace_Unchecked(object sender, RoutedEventArgs e)
        {
            overlayInputTrace.Stop();
        }

        private void CheckboxInputTrace_Checked(object sender, RoutedEventArgs e)
        {
            int width = 300;
            int x = (int)(System.Windows.SystemParameters.FullPrimaryScreenWidth / 2 + (width * 1.5));

            overlayInputTrace = new InputTraceOverlay(x, 0, width, 150);
            overlayInputTrace.Start();
        }
    }
}
