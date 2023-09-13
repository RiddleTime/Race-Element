using MaterialDesignThemes.Wpf;
using RaceElement.Controls.Util;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for SetupsTab.xaml
    /// </summary>
    public partial class SetupsTab : UserControl
    {
        public static SetupsTab Instance { get; private set; }

        public SetupsTab()
        {
            InitializeComponent();

            this.Loaded += SetupsTab_Loaded;
            Instance = this;
        }

        private void SetupsTab_Loaded(object sender, RoutedEventArgs e)
        {
            tabSetupTree.MouseUp += (s, e) =>
            {
                if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        SetupBrowser.Instance.FetchAllSetups();
                        MainWindow.Instance.EnqueueSnackbarMessage("Refreshed Setups");
                    });
            };
        }
    }
}
