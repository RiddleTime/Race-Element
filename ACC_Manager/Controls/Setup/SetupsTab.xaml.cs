using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for SetupsTab.xaml
    /// </summary>
    public partial class SetupsTab : UserControl
    {
        private static SetupsTab _instance;
        public static SetupsTab Instance { get { return _instance; } }

        public SetupsTab()
        {
            InitializeComponent();

            tabSetupTree.ContextMenu = GetBrowseTabContextMenu();
            _instance = this;
        }

        private ContextMenu GetBrowseTabContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu()
            {
                Style = Resources["MaterialDesignContextMenu"] as Style,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                GroupStyleSelector = null,
                UsesItemContainerTemplate = true,
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0))
            };

            Button refreshSetupTree = new Button()
            {
                Content = $"Refresh",
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            refreshSetupTree.Click += (s, e) => { SetupBrowser.Instance.FetchAllSetups(); ((s as Button).Parent as ContextMenu).IsOpen = false; };
            contextMenu.Items.Add(refreshSetupTree);

            return contextMenu;
        }
    }
}
