using ACCManager.Controls.Setup;
using System.IO;
using System.Windows.Controls;

namespace ACCManager.Controls
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
