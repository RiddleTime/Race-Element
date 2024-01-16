using MaterialDesignThemes.Wpf;
using RaceElement.Controls.Setup;
using RaceElement.Controls.Util;
using RaceElement.Data;
using RaceElement.Data.ACC.Core;
using RaceElement.Data.ACC.Tracks;
using RaceElement.Util;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static RaceElement.Data.ACC.Tracks.TrackData;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupJson;

namespace RaceElement.Controls;

/// <summary>
/// Interaction logic for SetupBrowser.xaml
/// </summary>
public partial class SetupBrowser : UserControl
{
    public static SetupBrowser Instance { get; set; }

    private readonly FlowDocSetupRenderer _setupRenderer;
    private string _selectedSetup;

    private Task _refreshTask = Task.CompletedTask;

    // car -> track
    private readonly Dictionary<string, List<string>> _expandedHeaders = [];

    public SetupBrowser()
    {
        InitializeComponent();

        _setupRenderer = new FlowDocSetupRenderer();

        setupsTreeView.SelectedItemChanged += SetupsTreeView_SelectedItemChanged;

        buttonEditSetup.Click += (o, e) =>
        {
            if (_selectedSetup != null)
                SetupEditor.Instance.Open(_selectedSetup);
        };

        this.IsVisibleChanged += (o, e) =>
        {
            if ((bool)e.NewValue)
                RefreshTree(false);
            else
                ClearSetups();
        };

        Instance = this;
    }

    private void ExpandCombination(string track, string carParseName)
    {
        if (track is null || track == string.Empty || carParseName == null || carParseName == string.Empty)
            return;

        CarModels carModel = ConversionFactory.ParseCarName(carParseName);
        if (carModel == CarModels.None) return;

        ConversionFactory.CarModelToCarName.TryGetValue(carModel, out string carName);
        AbstractTrackData trackData = TrackData.Tracks.Find(x => track == x.GameName);

        if (!_expandedHeaders.TryGetValue(carName, out List<string> list))
            _expandedHeaders.Add(carName, [trackData.GameName]);
        else
            _expandedHeaders[carName].Add(Regex.Replace(trackData.GameName, "^[a-z]", m => m.Value.ToUpper()));
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
                    buttonEditSetup.Visibility = Visibility.Hidden;
                else
                    buttonEditSetup.Visibility = Visibility.Visible;
#endif

                _setupRenderer.LogSetup(ref flowDocument, file.FullName);
                e.Handled = true;
            }
        }
    }

    private void ClearSetups()
    {
        while (setupsTreeView.Items.Count > 0)
        {
            TreeViewItem item = (TreeViewItem)setupsTreeView.Items[0];

            foreach (var cItem in item.ContextMenu.Items)
                if (cItem is MenuItem mItem)
                {
                    mItem.Click -= AddToCompare1_Click;
                    mItem.Click -= AddToCompare2_Click;
                    mItem.Click -= CopyToClipBoard_Click;
                    mItem.Click -= CopyToOtherTrack_Click;
                    mItem.Click -= OpenFolder_Click;
                    mItem.Visibility = Visibility.Collapsed;
                }
            item.ContextMenu?.Items.Clear();

            item.Visibility = Visibility.Collapsed;

            setupsTreeView.Items.RemoveAt(0);
        }

        Task.Run(() => GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true));
    }

    internal void RefreshTree(bool showMessage = true)
    {
        try
        {
            if (_refreshTask == Task.CompletedTask)
            {
                _refreshTask = Task.Run(() =>
                {
                    DirectoryInfo setupsDirectory = new(FileUtil.SetupsPath);

                    if (!setupsDirectory.Exists)
                        return;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ClearSetups();

                        // Pre-expand the current car and track leafs
                        if (AccProcess.IsRunning)
                        {
                            var staticPage = ACCSharedMemory.Instance.ReadStaticPageFile();
                            if (staticPage != null)
                            {
                                string track = staticPage.Track;
                                string carModel = staticPage.CarModel;
                                ExpandCombination(track, carModel);
                            }
                        }

                        // Find car directories
                        foreach (var carDir in (Span<DirectoryInfo>)setupsDirectory.GetDirectories())
                        {
                            if (carDir.GetDirectories().Length > 0 && !carDir.Name.Contains(".git"))
                            {
                                bool carHasSetups = false;

                                // check whether any setups exist for this car
                                foreach (var trackDir in (Span<DirectoryInfo>)carDir.GetDirectories())
                                    foreach (var carDirFile in (Span<FileInfo>)trackDir.GetFiles())
                                        if (carDirFile.Extension == ".json")
                                            carHasSetups = true;

                                if (!carHasSetups)
                                    continue;

                                // Make Car Tree View Item
                                TextBlock carHeader = new()
                                {
                                    Text = CarModelToCarName[ParseCarName(carDir.Name)],
                                    Style = Resources["MaterialDesignSubtitle1TextBlock"] as Style,
                                };
                                TreeViewItem carTreeViewItem = new()
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
                                    if (!carTreeViewItem.HasItems)
                                    {
                                        Debug.WriteLine($"Loading tracks for {carHeader.Text} ");
                                        foreach (var trackDir in (Span<DirectoryInfo>)carDir.GetDirectories())
                                        {
                                            bool trackHasSetups = false;
                                            foreach (var carDirFile in (Span<FileInfo>)trackDir.GetFiles())
                                                if (carDirFile.Extension == ".json")
                                                    trackHasSetups = true;

                                            if (!trackHasSetups) continue;


                                            string trackName = trackDir.Name;
                                            trackName = Regex.Replace(trackName, "^[a-z]", m => m.Value.ToUpper());
                                            trackName = trackName.Replace("_", " ");
                                            TextBlock trackHeader = new()
                                            {
                                                Text = trackName,
                                                Style = Resources["MaterialDesignSubtitle2TextBlock"] as Style,
                                            };
                                            TreeViewItem trackTreeViewItem = new()
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

                                            trackTreeViewItem.Expanded += (s, e) =>
                                            {
                                                if (!trackTreeViewItem.HasItems)
                                                {
                                                    // find setups in track dir
                                                    foreach (var trackFile in (Span<FileInfo>)trackDir.GetFiles())
                                                        if (trackFile.Extension.Equals(".json"))
                                                        {
                                                            TextBlock setupHeader = new()
                                                            {
                                                                Text = trackFile.Name.Replace(".json", ""),
                                                                Style = Resources["MaterialDesignDataGridTextColumnStyle"] as Style
                                                            };
                                                            TreeViewItem setupTreeViewItem = new()
                                                            {
                                                                Header = setupHeader,
                                                                DataContext = trackFile,
                                                            };
                                                            setupTreeViewItem.MouseLeftButtonUp += (s, e) => e.Handled = true;
                                                            setupTreeViewItem.ContextMenu = GetSetupContextMenu(trackFile);
                                                            trackTreeViewItem.Items.Add(setupTreeViewItem);
                                                        }
                                                }

                                                if (_expandedHeaders.ContainsKey(carHeader.Text) && !_expandedHeaders[carHeader.Text].Contains(trackName))
                                                    _expandedHeaders[carHeader.Text].Add(trackName);

                                                int targetItemInView = trackTreeViewItem.Items.Count;
                                                targetItemInView.ClipMax(18);
                                                if (targetItemInView > 0)
                                                    ((TreeViewItem)trackTreeViewItem.Items.GetItemAt(targetItemInView - 1)).BringIntoView();
                                            };

                                            if (_expandedHeaders.ContainsKey(carHeader.Text) && _expandedHeaders[carHeader.Text].Contains(trackName))
                                                trackTreeViewItem.IsExpanded = true;

                                            carTreeViewItem.Items.Add(trackTreeViewItem);
                                        }
                                    }
                                    if (!_expandedHeaders.ContainsKey(carHeader.Text))
                                        _expandedHeaders.Add(carHeader.Text, []);
                                };
                                carTreeViewItem.Collapsed += (s, e) =>
                                {
                                    if (s == carTreeViewItem)
                                        if (_expandedHeaders.ContainsKey(carHeader.Text))
                                            _expandedHeaders.Remove(carHeader.Text);
                                };

                                if (_expandedHeaders.ContainsKey(carHeader.Text)) carTreeViewItem.IsExpanded = true;

                                setupsTreeView.Items.Add(carTreeViewItem);
                            }
                        }
                        //Task.Run(() =>
                        //{
                        //	Thread.Sleep(2000);
                        //	GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false, false);
                        //});
                    }));

                    if (showMessage)
                    {
                        MainWindow.Instance.ClearSnackbar();
                        MainWindow.Instance.EnqueueSnackbarMessage("Refreshed Setups");
                    }

                    // debounce
                    Thread.Sleep(1000);
                    _refreshTask = Task.CompletedTask;
                });
            }
        }
        catch (Exception e)
        {
            LogWriter.WriteToLog(e);
        }
    }
    private ContextMenu GetCarContextMenu(DirectoryInfo directory)
    {
        ContextMenu menu = ContextMenuHelper.DefaultContextMenu();

        MenuItem folder = ContextMenuHelper.DefaultMenuItem("Open in explorer", PackIconKind.FolderOpen);
        folder.CommandParameter = directory;
        folder.Click += OpenFolder_Click;

        menu.Items.Add(folder);
        return menu;
    }

    private ContextMenu GetTrackContextMenu(DirectoryInfo directory)
    {
        ContextMenu menu = ContextMenuHelper.DefaultContextMenu();

        MenuItem folder = ContextMenuHelper.DefaultMenuItem("Open in explorer", PackIconKind.FolderOpen);
        folder.CommandParameter = directory;
        folder.Click += OpenFolder_Click;

        menu.Items.Add(folder);

        return menu;
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender.GetType() == typeof(MenuItem))
        {
            MenuItem button = (MenuItem)sender;

            DirectoryInfo directory = (DirectoryInfo)button.CommandParameter;
            Process.Start("explorer", directory.FullName);
        }
    }

    private ContextMenu GetSetupContextMenu(FileInfo file)
    {
        ContextMenu contextMenu = ContextMenuHelper.DefaultContextMenu();

        MenuItem copy = ContextMenuHelper.DefaultMenuItem("Copy to clipboard", PackIconKind.ContentCopy);
        copy.CommandParameter = file;
        copy.Click += CopyToClipBoard_Click;
        contextMenu.Items.Add(copy);

        MenuItem addCompare1 = ContextMenuHelper.DefaultMenuItem("Add to compare 1", PackIconKind.Compare);
        addCompare1.CommandParameter = file;
        addCompare1.Click += AddToCompare1_Click;
        contextMenu.Items.Add(addCompare1);

        MenuItem addCompare2 = ContextMenuHelper.DefaultMenuItem("Add to compare 2", PackIconKind.Compare);
        addCompare2.CommandParameter = file;
        addCompare2.Click += AddToCompare2_Click;
        contextMenu.Items.Add(addCompare2);

        MenuItem copyToOtherTrack = ContextMenuHelper.DefaultMenuItem("Copy to other track", PackIconKind.SwapVertical);
        copyToOtherTrack.CommandParameter = file;
        copyToOtherTrack.Click += CopyToOtherTrack_Click;
        contextMenu.Items.Add(copyToOtherTrack);

        return contextMenu;
    }

    private void CopyToClipBoard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem button)
        {
            FileInfo file = (FileInfo)button.CommandParameter;
            Thread thread = new(() =>
            {
                Clipboard.SetFileDropList(
                [
                    file.FullName
                ]);

                Dispatcher.Invoke(new Action(() =>
                {
                    MainWindow.Instance.EnqueueSnackbarMessage($"Copied setup \'{file.Name}\' to the clipboard.");
                }));
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }

    private void CopyToOtherTrack_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem button)
        {
            FileInfo file = (FileInfo)button.DataContext;

            SetupImporter.Instance.Open(file.FullName, true, true);

            (button.Parent as ContextMenu).IsOpen = false;
        }
    }

    private void AddToCompare2_Click(object sender, RoutedEventArgs e)
    {
        if (sender.GetType() == typeof(MenuItem))
        {
            MenuItem button = (MenuItem)sender;

            SetupComparer.Instance.SetSetup2((FileInfo)button.CommandParameter);

            MainWindow.Instance.snackbar.MessageQueue.Clear();
            MainWindow.Instance.snackbar.MessageQueue.Enqueue("Added setup to compare 2");

            (button.Parent as ContextMenu).IsOpen = false;
        }
    }

    private void AddToCompare1_Click(object sender, RoutedEventArgs e)
    {
        if (sender.GetType() == typeof(MenuItem))
        {
            MenuItem button = (MenuItem)sender;

            SetupComparer.Instance.SetSetup1((FileInfo)button.CommandParameter);

            MainWindow.Instance.snackbar.MessageQueue.Clear();
            MainWindow.Instance.snackbar.MessageQueue.Enqueue("Added setup to compare 1");

            (button.Parent as ContextMenu).IsOpen = false;
        }
    }

}
