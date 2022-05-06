using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using static ACCManager.ACCSharedMemory;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for TelemetryTab.xaml
    /// </summary>
    public partial class TelemetryTab : UserControl
    {
        public TelemetryTab()
        {
            InitializeComponent();

#if DEBUG
            tbDataChartsInProgress.Visibility = Visibility.Hidden;
            dataRecorderControl.Visibility = Visibility.Visible;
#endif
        }
    }
}
