using ACCSetupApp.Controls.Setup;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for SetupBrowser.xaml
    /// </summary>
    public partial class SetupBrowser : UserControl
    {
        public SetupBrowser()
        {
            InitializeComponent();

            new FlowDocSetupRenderer().LogSetup(ref flowDocument, @"C:\Users\Reinier\Documents\Assetto Corsa Competizione\Setups\honda_nsx_gt3_evo\misano\Aggressive_Preset.json");
        }

        private void FetchAllSetups()
        {


        }

        private string AccPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Assetto Corsa Competizione\\";
        private string SetupsPath => AccPath + "Setups\\";
    }
}
