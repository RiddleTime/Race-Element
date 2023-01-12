using System.Windows.Controls;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for LiveriesTab.xaml
    /// </summary>
    public partial class LiveriesTab : UserControl
    {

        public static LiveriesTab Instance;
        public LiveriesTab()
        {
            InitializeComponent();
            Instance = this;
        }
    }
}
