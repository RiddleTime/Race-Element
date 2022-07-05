using ACCManager.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
    /// Interaction logic for SetupsTab.xaml
    /// </summary>
    public partial class SetupsTab : UserControl
    {
        public SetupsTab()
        {
            InitializeComponent();

            this.Loaded += SetupsTab_Loaded;

            tabSetupTree.ContextMenu = GetBrowseTabContextMenu();
        }

        private void SetupsTab_Loaded(object sender, RoutedEventArgs e)
        {
            this.Drop += SetupsTab_Drop;
        }

        private void SetupsTab_Drop(object sender, DragEventArgs e)
        {
            if (e.Data is DataObject)
            {
                DataObject data = (DataObject)e.Data;

                StringCollection droppedItems = data.GetFileDropList();
                if (droppedItems.Count == 1)
                {
                    string droppedItem = droppedItems[0];

                    if (droppedItem.EndsWith(".json"))
                    {
                        SetupImporter.Instance.Open(droppedItem);
                    }
                }
            }
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
