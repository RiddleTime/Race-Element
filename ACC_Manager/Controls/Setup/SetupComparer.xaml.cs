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
        private readonly FlowDocCompareSetupRenderer setupRenderer = new FlowDocCompareSetupRenderer();
        private FileInfo Setup1 { get; set; }
        private FileInfo Setup2 { get; set; }


        public SetupComparer()
        {
            InitializeComponent();
            Instance = this;
        }

        public void SetSetup1(FileInfo file)
        {
            this.Setup1 = file;

            UpdateComparison();
            //setupRenderer.LogSetup(ref flowDoc1, file.FullName);
        }

        public void SetSetup2(FileInfo file)
        {
            this.Setup2 = file;
            UpdateComparison();
            //setupRenderer.LogSetup(ref flowDoc2, file.FullName);
        }

        private void UpdateComparison()
        {
            if (this.Setup1 != null && this.Setup2 != null)
            {
                this.setupRenderer.LogComparison(ref flowDocCompare, Setup1, Setup2);
            }
        }

        public static SetupComparer Instance { get; private set; }
    }
}
