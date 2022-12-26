using ACC_Manager.Util.Settings;
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
    /// Interaction logic for ReplaySettings.xaml
    /// </summary>
    public partial class ReplaySettings : UserControl
    {
        public ReplaySettings()
        {
            InitializeComponent();

            AccSettingsJson accSettings = new AccSettings().Get();
            accSettings.AutoRecordReplay = true;
            if (accSettings.AutoRecordReplay)
            {
                TitleBar.Instance.SetIcons(TitleBar.ActivatedIcons.AutomaticSaveReplay, true);
            }
        }
    }
}
