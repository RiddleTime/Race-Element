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
    /// Interaction logic for SetupComparer.xaml
    /// </summary>
    public partial class SetupComparer : UserControl
    {
        private readonly FlowDocSetupRenderer setupRenderer = new FlowDocSetupRenderer();

        public SetupComparer()
        {
            InitializeComponent();
            Instance = this;
        }

        public void SetSetup1(FileInfo file)
        {
            setupRenderer.LogSetup(ref flowDoc1, file.FullName);
        }

        public void SetSetup2(FileInfo file)
        {
            setupRenderer.LogSetup(ref flowDoc2, file.FullName);
        }

        public static SetupComparer Instance { get; private set; }
    }
}
