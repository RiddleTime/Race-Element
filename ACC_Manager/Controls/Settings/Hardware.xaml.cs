using ACCManager.Hardware.ACC.SteeringLock;
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

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for Hardware.xaml
    /// </summary>
    public partial class Hardware : UserControl
    {
        public Hardware()
        {
            InitializeComponent();
            toggleSteeringHardwareLock.Checked += ToggleSteeringHardwareLock_Checked;
            toggleSteeringHardwareLock.Unchecked += ToggleSteeringHardwareLock_Unchecked;
        }

        private void ToggleSteeringHardwareLock_Unchecked(object sender, RoutedEventArgs e)
        {
            SteeringLockTracker.Instance.Dispose();
        }

        private void ToggleSteeringHardwareLock_Checked(object sender, RoutedEventArgs e)
        {
            SteeringLockTracker.Instance.StartTracking();
        }
    }
}
