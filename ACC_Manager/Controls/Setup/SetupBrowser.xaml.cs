using ACCManager.Controls.Setup;
using ACCManager.Util;
using MaterialDesignThemes.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ACCManager.Data.SetupJson;
using static ACCManager.Data.ConversionFactory;
using ACC_Manager.Util.SystemExtensions;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for SetupBrowser.xaml
    /// </summary>
    public partial class SetupBrowser : UserControl
    {
        public static SetupBrowser Instance { get; set; }

        private readonly FlowDocSetupRenderer setupRenderer;
        private string selectedSetup;

        public SetupBrowser()
        {
            InitializeComponent();

            this.setupRenderer = new FlowDocSetupRenderer();

            FetchAllSetups();

            setupsTreeView.SelectedItemChanged += SetupsTreeView_SelectedItemChanged;

            buttonEditSetup.Click += (o, e) =>
            {
                if (selectedSetup != null)
                    SetupEditor.Instance.Open(selectedSetup);
            };

            Instance = this;
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
                            selectedSetup = file.FullName;

                            Root root = GetSetupJsonRoot(file);
                            if (root == null)
                                return;

                            // make edit button visible depending on whether there is a setup changer avaiable for the car
                            if (GetChanger(ParseCarName(root.carName)) == null)
                            {
                                buttonEditSetup.Visibility = Visibility.Hidden;
                            }
                            else
                            {
#if DEBUG
                                buttonEditSetup.Visibility = Visibility.Visible;
#endif
                            }

                            setupRenderer.LogSetup(ref flowDocument, file.FullName);
                        }
                    }
                }
            }
        }

        internal void FetchAllSetups()
        {
            try
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
                            Text = CarModelToCarName[ParseCarName(carDir.Name)],
                            Style = Resources["MaterialDesignSubtitle1TextBlock"] as Style,
                        };
                        TreeViewItem carTreeViewItem = new TreeViewItem()
                        {
                            Header = carHeader,
                            Background = new SolidColorBrush(Color.FromArgb(38, 10, 0, 0)),
                        };
                        carTreeViewItem.PreviewMouseLeftButtonDown += (s, e) => { carTreeViewItem.IsExpanded = !carTreeViewItem.IsExpanded; };
                        carTreeViewItem.ContextMenu = GetCarContextMenu(carDir);

                        // find track directories in car dir
                        foreach (var trackDir in carDir.GetDirectories())
                        {
                            bool trackHasSetups = false;

                            string trackName = trackDir.Name;
                            trackName = Regex.Replace(trackName, "^[a-z]", m => m.Value.ToUpper());
                            trackName = trackName.Replace("_", " ");
                            TextBlock trackHeader = new TextBlock()
                            {
                                Text = trackName,
                                Style = Resources["MaterialDesignSubtitle2TextBlock"] as Style,
                            };
                            TreeViewItem trackTreeViewItem = new TreeViewItem()
                            {
                                Header = trackHeader,
                                DataContext = trackDir,
                                Background = new SolidColorBrush(Color.FromArgb(19, 0, 0, 0)),
                            };
                            trackTreeViewItem.PreviewMouseLeftButtonDown += (s, e) => { trackTreeViewItem.IsExpanded = !trackTreeViewItem.IsExpanded; };
                            trackTreeViewItem.Expanded += (s, e) =>
                            {
                                int targetItemInView = trackTreeViewItem.Items.Count;
                                targetItemInView.ClipMax(18);
                                ((TreeViewItem)trackTreeViewItem.Items.GetItemAt(targetItemInView - 1)).BringIntoView();
                            };
                            trackTreeViewItem.ContextMenu = GetTrackContextMenu(trackDir);

                            // find setups in track dir
                            foreach (var trackFile in trackDir.GetFiles())
                            {
                                if (trackFile.Extension.Equals(".json"))
                                {
                                    TextBlock setupHeader = new TextBlock()
                                    {
                                        Text = trackFile.Name.Replace(".json", ""),
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
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
                Debug.WriteLine(ex);
            }
        }

        private ContextMenu GetCarContextMenu(DirectoryInfo directory)
        {
            ContextMenu menu = new ContextMenu()
            {
                Style = Resources["MaterialDesignContextMenu"] as Style,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                UsesItemContainerTemplate = true,
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0))
            };


            Button openFolder = new Button()
            {
                Content = "Open in explorer",
                CommandParameter = directory,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            openFolder.Click += OpenFolder_Click;

            menu.Items.Add(openFolder);

            return menu;
        }

        private ContextMenu GetTrackContextMenu(DirectoryInfo directory)
        {
            ContextMenu menu = new ContextMenu()
            {
                Style = Resources["MaterialDesignContextMenu"] as Style,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                UsesItemContainerTemplate = true,
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0))
            };


            Button openFolder = new Button()
            {
                Content = "Open in explorer",
                CommandParameter = directory,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            openFolder.Click += OpenFolder_Click;

            menu.Items.Add(openFolder);

            return menu;
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(Button))
            {
                Button button = (Button)sender;

                DirectoryInfo directory = (DirectoryInfo)button.CommandParameter;
                Process.Start(directory.FullName);

                (button.Parent as ContextMenu).IsOpen = false;
            }
        }

        private ContextMenu GetCompareContextMenu(FileInfo file)
        {
            ContextMenu contextMenu = new ContextMenu()
            {
                Style = Resources["MaterialDesignContextMenu"] as Style,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                UsesItemContainerTemplate = true,
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0)),
            };

            Button addToCompare1 = new Button()
            {
                Content = "Add to compare 1",
                CommandParameter = file,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            addToCompare1.Click += AddToCompare1_Click;

            Button addToCompare2 = new Button()
            {
                Content = "Add to compare 2",
                CommandParameter = file,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            addToCompare2.Click += AddToCompare2_Click;

            contextMenu.Items.Add(addToCompare1);
            contextMenu.Items.Add(addToCompare2);
            contextMenu.HorizontalOffset = 0;
            contextMenu.VerticalOffset = 0;

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
