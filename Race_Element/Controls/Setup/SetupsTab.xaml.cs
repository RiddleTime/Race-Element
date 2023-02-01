using MaterialDesignThemes.Wpf;
using RaceElement.Controls.Util;
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
            tabSetupTree.ContextMenu = GetBrowseTabContextMenu();
        }

        private ContextMenu GetBrowseTabContextMenu()
        {
            ContextMenu contextMenu = ContextMenuHelper.DefaultContextMenu();

            MenuItem refresh = ContextMenuHelper.DefaultMenuItem("Refresh", PackIconKind.Refresh);
            refresh.Click += (s, e) => SetupBrowser.Instance.FetchAllSetups();
            contextMenu.Items.Add(refresh);

            return contextMenu;
        }
    }
}
