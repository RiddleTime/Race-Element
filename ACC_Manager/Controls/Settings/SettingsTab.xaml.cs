using System.Windows.Controls;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for SettingsTab.xaml
    /// </summary>
    public partial class SettingsTab : UserControl
    {
        public static SettingsTab Instance { get; private set; }

        public SettingsTab()
        {
            InitializeComponent();
            Instance = this;
        }
    }
}
