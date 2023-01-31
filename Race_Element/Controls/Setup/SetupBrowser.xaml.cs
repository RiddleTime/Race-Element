using RaceElement.Controls.Setup;
using RaceElement.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static RaceElement.Data.SetupJson;
using static RaceElement.Data.ConversionFactory;
using RaceElement.Util.SystemExtensions;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for SetupBrowser.xaml
    /// </summary>
    public partial class SetupBrowser : UserControl
    {
        public static SetupBrowser Instance { get; set; }

        private readonly string SetupsPath = FileUtil.AccPath + "Setups\\";

        private readonly FlowDocSetupRenderer _setupRenderer;
        private string _selectedSetup;

        private Dictionary<string, List<string>> _expandedHeaders = new Dictionary<string, List<string>>();

        public SetupBrowser()
        {
            InitializeComponent();

            _setupRenderer = new FlowDocSetupRenderer();
            ThreadPool.QueueUserWorkItem(x =>
            {
                FetchAllSetups();
            });
            setupsTreeView.SelectedItemChanged += SetupsTreeView_SelectedItemChanged;

            buttonEditSetup.Click += (o, e) =>
            {
                if (_selectedSetup != null)
                    SetupEditor.Instance.Open(_selectedSetup);
            };

            Instance = this;
        }


        private void SetupsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null && e.NewValue.GetType() == typeof(TreeViewItem))
            {
                TreeViewItem item = (TreeViewItem)e.NewValue;
                if (item.DataContext is FileInfo file)
                {
                    _selectedSetup = file.FullName;

                    Root root = GetSetupJsonRoot(file);
                    if (root == null)
                        return;

#if DEBUG
                    // make edit button visible depending on whether there is a setup changer avaiable for the car
                    if (GetChanger(ParseCarName(root.CarName)) == null)
                    {
                        buttonEditSetup.Visibility = Visibility.Hidden;
                    }
                    else
                    {

                        buttonEditSetup.Visibility = Visibility.Visible;
                    }
#endif

                    _setupRenderer.LogSetup(ref flowDocument, file.FullName);
                    e.Handled = true;
                }
            }
        }

        internal void FetchAllSetups()
        {
            try
            {
                DirectoryInfo setupsDirectory = new DirectoryInfo(SetupsPath);

                if (!setupsDirectory.Exists)
                    return;

                Dispatcher.BeginInvoke(new Action(() =>
                {
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
                            carTreeViewItem.MouseLeftButtonUp += (s, e) =>
                            {
                                carTreeViewItem.IsExpanded = !carTreeViewItem.IsExpanded;
                                if (s == carTreeViewItem)
                                    e.Handled = true;
                            };
                            carTreeViewItem.ContextMenu = GetCarContextMenu(carDir);
                            carTreeViewItem.Expanded += (s, e) =>
                            {
                                if (!_expandedHeaders.ContainsKey(carHeader.Text))
                                    _expandedHeaders.Add(carHeader.Text, new List<string>());
                            };
                            carTreeViewItem.Collapsed += (s, e) =>
                            {
                                if (s == carTreeViewItem)
                                    if (_expandedHeaders.ContainsKey(carHeader.Text))
                                        _expandedHeaders.Remove(carHeader.Text);
                            };

                            if (_expandedHeaders.ContainsKey(carHeader.Text)) carTreeViewItem.IsExpanded = true;

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
                                trackTreeViewItem.MouseLeftButtonUp += (s, e) =>
                                {
                                    trackTreeViewItem.IsExpanded = !trackTreeViewItem.IsExpanded;
                                    if (s == trackTreeViewItem)
                                        e.Handled = true;
                                };
                                trackTreeViewItem.Expanded += (s, e) =>
                                {
                                    if (_expandedHeaders.ContainsKey(carHeader.Text) && !_expandedHeaders[carHeader.Text].Contains(trackName))
                                        _expandedHeaders[carHeader.Text].Add(trackName);

                                    int targetItemInView = trackTreeViewItem.Items.Count;
                                    targetItemInView.ClipMax(18);
                                    if (targetItemInView > 0)
                                        ((TreeViewItem)trackTreeViewItem.Items.GetItemAt(targetItemInView - 1)).BringIntoView();
                                };
                                trackTreeViewItem.Collapsed += (s, e) =>
                                {
                                    if (_expandedHeaders.ContainsKey(carHeader.Text))
                                        _expandedHeaders[carHeader.Text].Remove(trackName);
                                    e.Handled = true;
                                };
                                trackTreeViewItem.ContextMenu = GetTrackContextMenu(trackDir);

                                if (_expandedHeaders.ContainsKey(carHeader.Text) && _expandedHeaders[carHeader.Text].Contains(trackName))
                                    trackTreeViewItem.IsExpanded = true;

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
                                        TreeViewItem setupTreeViewItem = new TreeViewItem()
                                        {
                                            Header = setupHeader,
                                            DataContext = trackFile,
                                        };
                                        setupTreeViewItem.MouseLeftButtonUp += (s, e) => e.Handled = true;

                                        setupTreeViewItem.ContextMenu = GetSetupContextMenu(trackFile);

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
                }));
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

        private ContextMenu GetSetupContextMenu(FileInfo file)
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

            Button copyToClipBoard = new Button()
            {
                Content = "Copy to Clipboard",
                CommandParameter = file,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            copyToClipBoard.Click += CopyToClipBoard_Click;
            contextMenu.Items.Add(copyToClipBoard);

            Button copyToOtherTrack = new Button()
            {
                Content = "Copy to other track",
                CommandParameter = file,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            copyToOtherTrack.Click += CopyToOtherTrack_Click;
            contextMenu.Items.Add(copyToOtherTrack);


            contextMenu.HorizontalOffset = 0;
            contextMenu.VerticalOffset = 0;

            return contextMenu;
        }

        private void CopyToClipBoard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                FileInfo file = (FileInfo)button.DataContext;
                Thread thread = new Thread(() =>
                {
                    Clipboard.SetFileDropList(new StringCollection
                    {
                        file.FullName
                    });

                    Dispatcher.Invoke(new Action(() =>
                    {
                        MainWindow.Instance.EnqueueSnackbarMessage($"Copied setup \'{file.Name}\' to the clipboard.");
                    }));
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                (button.Parent as ContextMenu).IsOpen = false;
            }
        }

        private void CopyToOtherTrack_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                FileInfo file = (FileInfo)button.DataContext;

                SetupImporter.Instance.Open(file.FullName, true);

                (button.Parent as ContextMenu).IsOpen = false;
            }
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

    }
}
