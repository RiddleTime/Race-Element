using ACCSetupApp.Controls.Setup;
using ACCSetupApp.SetupParser;
using MaterialDesignThemes.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for SetupBrowser.xaml
    /// </summary>
    public partial class SetupBrowser : UserControl
    {
        private readonly ConversionFactory conversionFactory;
        private readonly FlowDocSetupRenderer setupRenderer;

        public SetupBrowser()
        {
            InitializeComponent();

            this.conversionFactory = new ConversionFactory();
            this.setupRenderer = new FlowDocSetupRenderer();

            FetchAllSetups();

            setupsTreeView.SelectedItemChanged += SetupsTreeView_SelectedItemChanged;


        }

        private void SetupsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null)
            {
                if (e.NewValue.GetType() == typeof(TreeViewItem))
                {
                    TreeViewItem item = (TreeViewItem)e.NewValue;
                    if (item.DataContext != null)
                    {
                        if (item.DataContext.GetType() == typeof(FileInfo))
                        {
                            FileInfo file = (FileInfo)item.DataContext;
                            setupRenderer.LogSetup(ref flowDocument, file.FullName);
                        }
                    }
                }
            }
        }

        private void FetchAllSetups()
        {
            DirectoryInfo setupsDirectory = new DirectoryInfo(SetupsPath);

            setupsTreeView.Items.Clear();

            // Find car directories
            foreach (var carDir in setupsDirectory.GetDirectories())
            {
                if (carDir.GetDirectories().Any() && !carDir.Name.Contains(".git"))
                {
                    bool carHasSetups = false;

                    // Make Car Tree View Item
                    TextBlock carHeader = new TextBlock()
                    {
                        Text = conversionFactory.ParseCarName(carDir.Name),
                        Style = Resources["MaterialDesignSubtitle1TextBlock"] as Style,
                    };
                    TreeViewItem carTreeViewItem = new TreeViewItem() { Header = carHeader };

                    // find track directories in car dir
                    foreach (var trackDir in carDir.GetDirectories())
                    {
                        bool trackHasSetups = false;

                        string trackName = trackDir.Name;
                        trackName = Regex.Replace(trackName, "^[a-z]", m => m.Value.ToUpper());
                        trackName = trackName.Replace("_", " ");
                        TextBlock trackHeader = new TextBlock()
                        {
                            Text = conversionFactory.ParseCarName(trackName),
                            Style = Resources["MaterialDesignSubtitle2TextBlock"] as Style,
                        };
                        TreeViewItem trackTreeViewItem = new TreeViewItem() { Header = trackHeader, DataContext = trackDir };

                        // find setups in track dir
                        foreach (var trackFile in trackDir.GetFiles())
                        {
                            if (trackFile.Extension.Equals(".json"))
                            {
                                TextBlock setupHeader = new TextBlock()
                                {
                                    Text = conversionFactory.ParseCarName(trackFile.Name.Replace(".json", "")),
                                    Style = Resources["MaterialDesignDataGridTextColumnStyle"] as Style
                                };
                                TreeViewItem setupTreeViewItem = new TreeViewItem() { Header = setupHeader, DataContext = trackFile };

                                setupTreeViewItem.ContextMenu = GetCompareContextMenu(trackFile);

                                trackTreeViewItem.Items.Add(setupTreeViewItem);
                            }
                        }

                        // check for any setups so the tree view doesn't get cluttered with cars that have no setups
                        if (trackTreeViewItem.Items.Count > 0)
                        {
                            carHasSetups = true;
                            trackHasSetups = true;
                        }

                        if (trackHasSetups)
                            carTreeViewItem.Items.Add(trackTreeViewItem);
                    }

                    if (carHasSetups)
                        setupsTreeView.Items.Add(carTreeViewItem);
                }
            }

        }

        private ContextMenu GetCompareContextMenu(FileInfo file)
        {
            ContextMenu contextMenu = new ContextMenu()
            {
                Style = Resources["MaterialDesignContextMenu"] as Style,
                Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                UsesItemContainerTemplate = true,
            };


            Button addToCompare1 = new Button()
            {
                Content = "Add to compare 1",
                CommandParameter = file,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
            };
            addToCompare1.Click += AddToCompare1_Click;

            Button addToCompare2 = new Button()
            {
                Content = "Add to compare 2",
                CommandParameter = file,
                Style = Resources["MaterialDesignRaisedButton"] as Style
            };
            addToCompare2.Click += AddToCompare2_Click;

            contextMenu.Items.Add(addToCompare1);
            contextMenu.Items.Add(addToCompare2);
            return contextMenu;
        }

        private void AddToCompare2_Click(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(Button))
            {
                Button button = (Button)sender;

                SetupComparer.Instance.SetSetup2((FileInfo)button.CommandParameter);

                MainWindow.Instance.snackbar.MessageQueue.Clear();
                MainWindow.Instance.snackbar.MessageQueue.Enqueue("Added setup to compare 2");

                (button.Parent as ContextMenu).IsOpen = false;
            }
        }

        private void AddToCompare1_Click(object sender, RoutedEventArgs e)
        {

            if (sender.GetType() == typeof(Button))
            {
                Button button = (Button)sender;

                SetupComparer.Instance.SetSetup1((FileInfo)button.CommandParameter);

                MainWindow.Instance.snackbar.MessageQueue.Clear();
                MainWindow.Instance.snackbar.MessageQueue.Enqueue("Added setup to compare 1");

                (button.Parent as ContextMenu).IsOpen = false;
            }
        }

        private string AccPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Assetto Corsa Competizione\\";
        private string SetupsPath => AccPath + "Setups\\";

    }
}
