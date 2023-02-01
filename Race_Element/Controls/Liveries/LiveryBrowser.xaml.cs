using RaceElement.Util.SystemExtensions;
using RaceElement.Data;
using RaceElement.LiveryParser;
using RaceElement.Util;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static RaceElement.Controls.LiveryTagging;
using RaceElement.Controls.Util;
using MaterialDesignThemes.Wpf;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for LiveryBrowser.xaml
    /// </summary>
    public partial class LiveryBrowser : UserControl
    {
        public static LiveryBrowser Instance;

        private List<string> expandedCarHeaders = new List<string>();
        private List<string> expandedTeamHeaders = new List<string>();
        private List<string> expandedTagHeaders = new List<string>();

        public LiveryBrowser()
        {
            InitializeComponent();

            Instance = this;
            this.Loaded += (s, e) => ThreadPool.QueueUserWorkItem(x => FetchAllCars());

            liveriesTreeViewTeams.SelectedItemChanged += LiveriesTreeView_SelectedItemChanged;
            liveriesTreeViewCars.SelectedItemChanged += LiveriesTreeView_SelectedItemChanged;
            liveriesTreeViewTags.SelectedItemChanged += LiveriesTreeView_SelectedItemChanged;

            liveriesTreeViewTeams.KeyUp += (s, e) => LiveryTreeViewKeyUp(liveriesTreeViewTeams, s, e);
            liveriesTreeViewCars.KeyUp += (s, e) => LiveryTreeViewKeyUp(liveriesTreeViewCars, s, e);
            liveriesTreeViewTags.KeyUp += (s, e) => LiveryTreeViewKeyUp(liveriesTreeViewTags, s, e);


            buttonImportLiveries.Click += ButtonImportLiveries_Click;

            buttonRefreshLiveries.Click += (s, e) => ThreadPool.QueueUserWorkItem(x => FetchAllCars()); ;

            buttonGenerateAllDDS.Click += ButtonGenerateAllDDS_Click;


            buttonNewTag.Click += (sender, args) =>
            {
                LiveryTagCreator.Instance.Open();
            };

            this.IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue)
                    LiveryDisplayer.Instance.ReloadLivery();
                else
                    LiveryDisplayer.Instance.Cache();

                ThreadPool.QueueUserWorkItem(x =>
                {
                    Thread.Sleep(10 * 1000);
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                });
            };
        }

        private void LiveryTreeViewKeyUp(TreeView treeView, object sender, KeyEventArgs e)
        {
            if (!treeView.IsVisible)
                return;

            switch (e.Key)
            {
                case Key.Delete:
                    {
                        var selectedItem = treeView.SelectedItem;

                        if (selectedItem == null)
                            return;

                        TreeViewItem item = (TreeViewItem)selectedItem;
                        if (item.DataContext != null)
                        {
                            if (item.DataContext.GetType() == typeof(LiveryTreeCar))
                            {
                                LiveryTreeCar treeCar = (LiveryTreeCar)item.DataContext;
                                LiveryDeleter.Instance.Open(treeCar);

                            }
                        }
                        break;
                    }
                default: break;
            }
        }

        private void ButtonGenerateAllDDS_Click(object sender, RoutedEventArgs e)
        {
            DDSgenerator.Instance.Visibility = Visibility.Visible;
        }

        private void ButtonImportLiveries_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(x => LiveryImporter.ImportLiveryZips());
        }

        private void LiveriesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null)
            {
                if (e.NewValue.GetType() == typeof(TreeViewItem))
                {
                    TreeViewItem item = (TreeViewItem)e.NewValue;
                    if (item.DataContext != null)
                    {
                        if (item.DataContext.GetType() == typeof(LiveryTreeCar))
                        {
                            LiveryTreeCar treeCar = (LiveryTreeCar)item.DataContext;


                            LiveryDisplayer.Instance.SetLivery(treeCar);
                            LiveryDisplayer.Instance.ReloadLivery();
                        }
                    }
                }
            }
        }

        internal class LiveryTreeCar
        {
            public FileInfo CarsFile { get; set; }
            public CarsJson.Root CarsRoot { get; set; }

            public override bool Equals(object obj)
            {
                return obj is LiveryTreeCar car &&
                       EqualityComparer<FileInfo>.Default.Equals(CarsFile, car.CarsFile) &&
                       EqualityComparer<CarsJson.Root>.Default.Equals(CarsRoot, car.CarsRoot);
            }
        }

        internal void FetchAllCars(bool tagsOnly = false)
        {
            Instance.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (!tagsOnly)
                    {
                        liveriesTreeViewTeams.Items.Clear();
                        liveriesTreeViewCars.Items.Clear();
                    }
                    liveriesTreeViewTags.Items.Clear();

                    DirectoryInfo customsCarsDirectory = new DirectoryInfo(FileUtil.CarsPath);

                    List<LiveryTreeCar> liveryTreeCars = new List<LiveryTreeCar>();

                    foreach (var carsFile in customsCarsDirectory.GetFiles())
                    {
                        if (carsFile.Extension != null && carsFile.Extension.Equals(".json"))
                        {
                            CarsJson.Root carsRoot = LiveryImporter.GetLivery(carsFile);

                            if (carsRoot != null)
                            {
                                LiveryTreeCar treeCar = new LiveryTreeCar() { CarsFile = carsFile, CarsRoot = carsRoot };

                                if (treeCar.CarsRoot.CustomSkinName != null /*&& treeCar.carsRoot.teamName != null*/)
                                    if (!treeCar.CarsRoot.CustomSkinName.Equals(string.Empty)
                                            //&& !treeCar.carsRoot.teamName.Equals(string.Empty)
                                            )
                                        liveryTreeCars.Add(treeCar);
                            }
                        }
                    }


                    var liveriesGroupedByCar = liveryTreeCars.GroupBy(g => ConversionFactory.GetCarName(g.CarsRoot.CarModelType));
                    var liveriesGroupedByTeam = liveryTreeCars.GroupBy(g => g.CarsRoot.TeamName);

                    if (!tagsOnly)
                    {
                        FillTreeViewTeams(liveriesGroupedByTeam);
                        FillTreeViewModels(liveriesGroupedByCar);
                    }
                    FillTreeViewTags(liveryTreeCars);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    LogWriter.WriteToLog(ex);
                }
            }));
        }

        private void FillTreeViewModels(IEnumerable<IGrouping<string, LiveryTreeCar>> liveriesGroupedByModel)
        {
            List<TreeViewItem> carsTreeViews = new List<TreeViewItem>();

            foreach (IGrouping<string, LiveryTreeCar> tItem in liveriesGroupedByModel)
            {
                string treeViewTitle = $"{tItem.Key}";
                int groupCount = tItem.Count();
                if (groupCount > 1)
                {
                    treeViewTitle += $"  ({groupCount})";
                }
                TextBlock teamHeader = new TextBlock()
                {
                    Text = treeViewTitle,
                    Style = Resources["MaterialDesignSubtitle1TextBlock"] as Style,
                    TextTrimming = TextTrimming.WordEllipsis,
                    Width = liveriesTreeViewTeams.Width - 5,
                    DataContext = tItem.Key
                };
                TreeViewItem modelItem = new TreeViewItem()
                {
                    Header = teamHeader,
                    Background = new SolidColorBrush(Color.FromArgb(38, 10, 0, 0)),
                };
                modelItem.MouseLeftButtonUp += (s, e) =>
                {
                    modelItem.IsExpanded = !modelItem.IsExpanded;
                    e.Handled = true;
                };
                if (expandedCarHeaders.Contains(tItem.Key)) modelItem.ExpandSubtree();
                modelItem.Expanded += (s, e) =>
                {
                    int targetItemInView = modelItem.Items.Count;
                    targetItemInView.ClipMax(18);
                    ((TreeViewItem)modelItem.Items.GetItemAt(targetItemInView - 1)).BringIntoView();
                    expandedCarHeaders.Add(tItem.Key);
                };
                modelItem.Collapsed += (s, e) => expandedCarHeaders.Remove(tItem.Key);

                var cars = tItem.ToList();
                cars.Sort((a, b) =>
                {
                    return $"{a.CarsRoot.CustomSkinName.ToLower()}".CompareTo($"{b.CarsRoot.CustomSkinName.ToLower()}");
                });
                foreach (LiveryTreeCar car in cars)
                {
                    TextBlock skinHeader = new TextBlock()
                    {
                        Text = $"{car.CarsRoot.CustomSkinName}",
                        Style = Resources["MaterialDesignDataGridTextColumnStyle"] as Style,
                        TextTrimming = TextTrimming.WordEllipsis,
                        Width = liveriesTreeViewTeams.Width - 5
                    };
                    TreeViewItem skinItem = new TreeViewItem() { Header = skinHeader, DataContext = car };
                    skinItem.ContextMenu = GetSkinContextMenu(car, null);
                    skinItem.MouseLeftButtonUp += (s, e) => e.Handled = true;

                    modelItem.Items.Add(skinItem);
                }

                modelItem.ContextMenu = GetTeamContextMenu(modelItem);
                carsTreeViews.Add(modelItem);
            }

            carsTreeViews.Sort((a, b) =>
            {
                TextBlock aCar = a.Header as TextBlock;
                TextBlock bCar = b.Header as TextBlock;
                return $"{aCar.DataContext}".CompareTo($"{bCar.DataContext}");
            });

            carsTreeViews.ForEach(x => liveriesTreeViewCars.Items.Add(x));
        }


        private void FillTreeViewTeams(IEnumerable<IGrouping<string, LiveryTreeCar>> liveriesGroupedByTeam)
        {
            List<TreeViewItem> carsTreeViews = new List<TreeViewItem>();

            foreach (IGrouping<string, LiveryTreeCar> tItem in liveriesGroupedByTeam)
            {
                string treeViewTitle = $"{tItem.Key}";
                int groupCount = tItem.Count();
                if (groupCount > 1)
                {
                    treeViewTitle += $"  ({groupCount})";
                }
                TextBlock teamHeader = new TextBlock()
                {
                    Text = treeViewTitle,
                    Style = Resources["MaterialDesignSubtitle1TextBlock"] as Style,
                    TextTrimming = TextTrimming.WordEllipsis,
                    Width = liveriesTreeViewTeams.Width - 5,
                    DataContext = tItem.Key
                };
                TreeViewItem teamItem = new TreeViewItem()
                {
                    Header = teamHeader,
                    Background = new SolidColorBrush(Color.FromArgb(38, 10, 0, 0)),
                };
                teamItem.MouseLeftButtonUp += (s, e) =>
                {
                    teamItem.IsExpanded = !teamItem.IsExpanded;
                    e.Handled = true;
                };
                if (expandedTeamHeaders.Contains(tItem.Key)) teamItem.ExpandSubtree();
                teamItem.Expanded += (s, e) =>
                {
                    int targetItemInView = teamItem.Items.Count;
                    targetItemInView.ClipMax(18);
                    ((TreeViewItem)teamItem.Items.GetItemAt(targetItemInView - 1)).BringIntoView();
                    expandedTeamHeaders.Add(tItem.Key);
                };
                teamItem.Collapsed += (s, e) => expandedTeamHeaders.Remove(tItem.Key);

                var cars = tItem.ToList();
                cars.Sort((a, b) =>
                {
                    return $"{a.CarsRoot.CustomSkinName.ToLower()}".CompareTo($"{b.CarsRoot.CustomSkinName.ToLower()}");
                });

                foreach (LiveryTreeCar car in cars)
                {
                    TextBlock skinHeader = new TextBlock()
                    {
                        Text = $"{car.CarsRoot.CustomSkinName}",
                        Style = Resources["MaterialDesignDataGridTextColumnStyle"] as Style,
                        TextTrimming = TextTrimming.WordEllipsis,
                        Width = liveriesTreeViewTeams.Width - 5
                    };
                    TreeViewItem skinItem = new TreeViewItem() { Header = skinHeader, DataContext = car };
                    skinItem.ContextMenu = GetSkinContextMenu(car, null);
                    skinItem.MouseLeftButtonUp += (s, e) => e.Handled = true;

                    teamItem.Items.Add(skinItem);
                }

                teamItem.ContextMenu = GetTeamContextMenu(teamItem);
                carsTreeViews.Add(teamItem);
            }

            carsTreeViews.Sort((a, b) =>
            {
                TextBlock aCar = a.Header as TextBlock;
                TextBlock bCar = b.Header as TextBlock;
                return $"{aCar.DataContext}".CompareTo($"{bCar.DataContext}");
            });

            carsTreeViews.ForEach(x => liveriesTreeViewTeams.Items.Add(x));
        }

        private void FillTreeViewTags(List<LiveryTreeCar> allLiveries)
        {
            List<TreeViewItem> tagTreeItems = new List<TreeViewItem>();

            List<LiveryTreeCar> liveriesWithTags = new List<LiveryTreeCar>();

            LiveryTagging.GetAllTags().ForEach(liveryTag =>
            {
                List<LiveryTreeCar> tagCars = new List<LiveryTreeCar>();
                foreach (LiveryTreeCar car in allLiveries)
                {
                    if (LiveryTagging.TagContainsCar(liveryTag, car))
                    {
                        tagCars.Add(car);
                    }
                }

                string treeViewTitle = $"{liveryTag.Name}";
                int groupCount = tagCars.Count();
                if (groupCount > 1)
                {
                    treeViewTitle += $"  ({groupCount})";
                }

                TextBlock tagHeader = new TextBlock()
                {
                    Text = treeViewTitle,
                    Style = Resources["MaterialDesignSubtitle1TextBlock"] as Style,
                    TextTrimming = TextTrimming.WordEllipsis,
                    Width = liveriesTreeViewTeams.Width - 5,
                    DataContext = liveryTag.Name
                };
                TreeViewItem tagItem = new TreeViewItem()
                {
                    Header = tagHeader,
                    Background = new SolidColorBrush(Color.FromArgb(38, 10, 0, 0)),
                };
                tagItem.MouseLeftButtonUp += (s, e) =>
                {
                    tagItem.IsExpanded = !tagItem.IsExpanded;
                    e.Handled = true;
                };
                if (expandedTagHeaders.Contains(liveryTag.Name)) tagItem.ExpandSubtree();
                tagItem.Expanded += (s, e) =>
                {
                    int targetItemInView = tagItem.Items.Count;
                    targetItemInView.ClipMax(18);
                    if (tagItem.Items.Count > 0)
                        ((TreeViewItem)tagItem.Items.GetItemAt(targetItemInView - 1)).BringIntoView();
                    expandedTagHeaders.Add(liveryTag.Name);
                };
                tagItem.Collapsed += (s, e) => expandedTagHeaders.Remove(liveryTag.Name);

                tagItem.ContextMenu = GetTagContextMenu(tagItem, liveryTag);
                tagCars.Sort((a, b) =>
                {
                    return $"{a.CarsRoot.CustomSkinName.ToLower()}".CompareTo($"{b.CarsRoot.CustomSkinName.ToLower()}");
                });

                foreach (LiveryTreeCar car in tagCars)
                {
                    TextBlock skinHeader = new TextBlock()
                    {
                        Text = $"{car.CarsRoot.CustomSkinName}",
                        Style = Resources["MaterialDesignDataGridTextColumnStyle"] as Style,
                        TextTrimming = TextTrimming.WordEllipsis,
                        Width = liveriesTreeViewTeams.Width - 5
                    };
                    TreeViewItem skinItem = new TreeViewItem() { Header = skinHeader, DataContext = car };
                    skinItem.ContextMenu = GetSkinContextMenu(car, liveryTag);
                    skinItem.MouseLeftButtonUp += (s, e) => e.Handled = true;
                    if (!liveriesWithTags.Contains(car))
                        liveriesWithTags.Add(car);

                    tagItem.Items.Add(skinItem);
                }

                tagTreeItems.Add(tagItem);
            });

            tagTreeItems.Sort((a, b) =>
            {
                TextBlock textA = a.Header as TextBlock;
                TextBlock textB = b.Header as TextBlock;
                return $"{textA.DataContext}".CompareTo($"{textB.DataContext}");
            });


            if (liveriesWithTags.Count > 0)
            {
                List<LiveryTreeCar> liveriesWithoutTags = allLiveries.Except(liveriesWithTags).ToList();

                string treeViewTitle = $"No Tags ({liveriesWithoutTags.Count})";
                TextBlock tagHeader = new TextBlock()
                {
                    Text = treeViewTitle,
                    Style = Resources["MaterialDesignSubtitle1TextBlock"] as Style,
                    TextTrimming = TextTrimming.WordEllipsis,
                    Width = liveriesTreeViewTeams.Width - 5,
                    Background = new SolidColorBrush(Colors.OrangeRed)
                };
                if (liveriesWithoutTags.Count == 0)
                    tagHeader.Background = new SolidColorBrush(Colors.DarkOliveGreen);

                TreeViewItem tagItem = new TreeViewItem()
                {
                    Header = tagHeader,
                    Background = new SolidColorBrush(Color.FromArgb(38, 10, 0, 0)),
                };
                tagItem.PreviewMouseLeftButtonDown += (s, e) => { tagItem.IsExpanded = !tagItem.IsExpanded; };
                if (expandedTagHeaders.Contains("No Tags")) tagItem.ExpandSubtree();
                tagItem.Expanded += (s, e) =>
                {
                    int targetItemInView = tagItem.Items.Count;
                    targetItemInView.ClipMax(18);
                    if (tagItem.Items.Count > 0)
                        ((TreeViewItem)tagItem.Items.GetItemAt(targetItemInView - 1)).BringIntoView();
                    expandedTagHeaders.Add("No Tags");
                };
                tagItem.Collapsed += (s, e) => expandedTagHeaders.Remove("No Tags");

                liveriesWithoutTags.Sort((a, b) =>
                {
                    return $"{a.CarsRoot.TeamName} / {a.CarsRoot.CustomSkinName}".CompareTo($"{b.CarsRoot.TeamName} / {b.CarsRoot.CustomSkinName}");
                });

                foreach (LiveryTreeCar car in liveriesWithoutTags)
                {
                    TextBlock skinHeader = new TextBlock()
                    {
                        Text = $"{car.CarsRoot.TeamName} / {car.CarsRoot.CustomSkinName}",
                        Style = Resources["MaterialDesignDataGridTextColumnStyle"] as Style,
                        TextTrimming = TextTrimming.WordEllipsis,
                        Width = liveriesTreeViewTeams.Width - 5
                    };
                    TreeViewItem skinItem = new TreeViewItem() { Header = skinHeader, DataContext = car };
                    skinItem.ContextMenu = GetSkinContextMenu(car, null);

                    if (!liveriesWithTags.Contains(car))
                        liveriesWithTags.Add(car);

                    tagItem.Items.Add(skinItem);
                }

                tagTreeItems.Insert(0, tagItem);

            }

            tagTreeItems.ForEach(x => liveriesTreeViewTags.Items.Add(x));

        }


        private ContextMenu GetSkinContextMenu(LiveryTreeCar liveryTreeCar, LiveryTag tag)
        {
            ContextMenu menu = ContextMenuHelper.DefaultContextMenu();

            MenuItem openLiveryDirectory = ContextMenuHelper.DefaultMenuItem("Open Livery Directory", PackIconKind.FolderOpen);
            openLiveryDirectory.CommandParameter = liveryTreeCar;
            openLiveryDirectory.Click += OpenLiveryDirectory_Click;
            menu.Items.Add(openLiveryDirectory);

            MenuItem viewInExplorer = ContextMenuHelper.DefaultMenuItem("View Livery Json in Explorer", PackIconKind.FolderEye);
            viewInExplorer.CommandParameter = liveryTreeCar;
            viewInExplorer.Click += OpenLiveryJson_Click;
            menu.Items.Add(viewInExplorer);

            MenuItem createZipButton = ContextMenuHelper.DefaultMenuItem("Save as Zip Archive", PackIconKind.ArchiveOutline);
            createZipButton.CommandParameter = liveryTreeCar;
            createZipButton.Click += CreateZipButton_Click;
            menu.Items.Add(createZipButton);

            MenuItem addSkinToSkinPack = ContextMenuHelper.DefaultMenuItem("Add to Skin Pack", PackIconKind.ArchiveAddOutline);
            addSkinToSkinPack.CommandParameter = liveryTreeCar;
            addSkinToSkinPack.Click += AddSkinToSkinPack_Click;
            menu.Items.Add(addSkinToSkinPack);

            if (tag == null)
            {
                MenuItem addSkinToTag = ContextMenuHelper.DefaultMenuItem("Add to Tag", PackIconKind.TagPlusOutline);
                addSkinToTag.CommandParameter = liveryTreeCar;
                addSkinToTag.Click += AddSkinToTag_Click;
                menu.Items.Add(addSkinToTag);
            }
            else
            {
                MenuItem removeSkinFromTag = ContextMenuHelper.DefaultMenuItem("Remove from Tag", PackIconKind.TagMinusOutline);
                removeSkinFromTag.Click += (e, s) =>
                {
                    LiveryTagging.RemoveFromTag(tag, liveryTreeCar);
                    FetchAllCars();
                };
                menu.Items.Add(removeSkinFromTag);
            }

            MenuItem deleteLivery = ContextMenuHelper.DefaultMenuItem("Delete", PackIconKind.Delete);
            deleteLivery.CommandParameter = liveryTreeCar;
            deleteLivery.Click += DeleteLivery_Click;
            menu.Items.Add(deleteLivery);

            return menu;
        }

        private void AddSkinToTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(MenuItem))
                {
                    MenuItem button = (MenuItem)sender;

                    LiveryTreeCar liveryTreeCar = (LiveryTreeCar)button.CommandParameter;

                    LiveryTagger.Instance.Open(liveryTreeCar);
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }
        }

        private void DeleteLivery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(MenuItem))
                {
                    MenuItem button = (MenuItem)sender;

                    LiveryTreeCar liveryTreeCar = (LiveryTreeCar)button.CommandParameter;
                    LiveryDeleter.Instance.Open(liveryTreeCar);
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }
        }

        private void AddSkinToSkinPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(MenuItem))
                {
                    MenuItem button = (MenuItem)sender;

                    LiveryTreeCar liveryTreeCar = (LiveryTreeCar)button.CommandParameter;
                    if (LiveryExporter.Instance.AddExportItem(liveryTreeCar))
                        MainWindow.Instance.EnqueueSnackbarMessage($"Added {liveryTreeCar.CarsRoot.TeamName}/{liveryTreeCar.CarsRoot.CustomSkinName} to skin pack.");
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }
        }

        private void OpenLiveryJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(MenuItem))
                {
                    MenuItem button = (MenuItem)sender;
                    LiveryTreeCar liveryTreeCar = (LiveryTreeCar)button.CommandParameter;

                    if (liveryTreeCar.CarsRoot.CustomSkinName == null || liveryTreeCar.CarsRoot.CustomSkinName.Length == 0)
                        return;

                    FileInfo carsJsonFile = new FileInfo($"{liveryTreeCar.CarsFile}");
                    Process.Start($"explorer", $"/select,{FileUtil.CarsPath}{carsJsonFile.Name}");
                }
            }
            catch (Exception ex) { LogWriter.WriteToLog(ex); }
        }

        private void OpenLiveryDirectory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(MenuItem))
                {
                    MenuItem button = (MenuItem)sender;
                    LiveryTreeCar liveryTreeCar = (LiveryTreeCar)button.CommandParameter;

                    if (liveryTreeCar.CarsRoot.CustomSkinName == null || liveryTreeCar.CarsRoot.CustomSkinName.Length == 0)
                        return;

                    DirectoryInfo directory = new DirectoryInfo($"{FileUtil.LiveriesPath}{liveryTreeCar.CarsRoot.CustomSkinName}");
                    Process.Start(directory.FullName);
                }
            }
            catch (Exception ex) { LogWriter.WriteToLog(ex); }
        }

        private ContextMenu GetTagContextMenu(TreeViewItem teamItem, LiveryTag tag)
        {
            ContextMenu menu = ContextMenuHelper.DefaultContextMenu();

            MenuItem addTagToSkinPack = ContextMenuHelper.DefaultMenuItem("Add to Skin Pack", PackIconKind.ArchiveAddOutline);
            addTagToSkinPack.CommandParameter = teamItem;
            addTagToSkinPack.Click += AddTeamToSkinPack_Click;
            menu.Items.Add(addTagToSkinPack);

            MenuItem deleteTag = ContextMenuHelper.DefaultMenuItem("Delete Tag", PackIconKind.TagRemoveOutline);
            deleteTag.ToolTip = "Warning! This permanently deletes this tag!";
            deleteTag.Click += (e, s) => LiveryTagging.DeleteTag(tag);
            menu.Items.Add(deleteTag);

            return menu;
        }

        private ContextMenu GetTeamContextMenu(TreeViewItem teamItem)
        {
            ContextMenu menu = ContextMenuHelper.DefaultContextMenu();

            MenuItem addTeamToSkinPack = ContextMenuHelper.DefaultMenuItem("Add to Skin Pack", PackIconKind.ArchivePlusOutline);
            addTeamToSkinPack.CommandParameter = teamItem;
            addTeamToSkinPack.Click += AddTeamToSkinPack_Click;
            menu.Items.Add(addTeamToSkinPack);

            MenuItem addTeamToTag = ContextMenuHelper.DefaultMenuItem("Add to Tag", PackIconKind.TagPlusOutline);
            addTeamToTag.CommandParameter = teamItem;
            addTeamToTag.Click += AddTeamToTag_Click;
            menu.Items.Add(addTeamToTag);

            return menu;
        }

        private void AddTeamToTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(MenuItem))
                {
                    MenuItem button = (MenuItem)sender;

                    TreeViewItem treeItem = (TreeViewItem)button.CommandParameter;
                    List<LiveryTreeCar> treeCars = new List<LiveryTreeCar>();

                    treeItem.Items.OfType<TreeViewItem>().ToList().ForEach(x =>
                    {
                        treeCars.Add((LiveryTreeCar)x.DataContext);
                    });

                    LiveryTagger.Instance.Open(treeCars);
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }
        }

        private void AddTeamToSkinPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(MenuItem))
                {
                    MenuItem button = (MenuItem)sender;

                    TreeViewItem treeItem = (TreeViewItem)button.CommandParameter;
                    List<LiveryTreeCar> treeCars = new List<LiveryTreeCar>();

                    treeItem.Items.OfType<TreeViewItem>().ToList().ForEach(x =>
                    {
                        LiveryExporter.Instance.AddExportItem((LiveryTreeCar)x.DataContext);
                    });
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }
        }

        private void CreateZipButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(MenuItem))
            {
                MenuItem button = (MenuItem)sender;

                LiveryTreeCar liveryTreeCar = (LiveryTreeCar)button.CommandParameter;

                if (liveryTreeCar.CarsRoot.CustomSkinName == null || liveryTreeCar.CarsRoot.CustomSkinName.Length == 0)
                    return;

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

                // Set filter for file extension and default file extension 
                dlg.DefaultExt = ".zip";
                dlg.AddExtension = true;
                dlg.CheckPathExists = true;
                dlg.FileName = $"{liveryTreeCar.CarsRoot.CustomSkinName}";
                dlg.DefaultExt = ".zip";
                dlg.Filter = "Livery zip|*.zip";
                Nullable<bool> result = dlg.ShowDialog();


                // Get the selected file name and display in a TextBox 
                if (result == true)
                {
                    // Open document 
                    string filename = dlg.FileName;

                    if (filename == null)
                        return;

                    using (ZipArchive zipArchive = ZipArchive.Create())
                    {
                        string liveriesFolder = $"Liveries\\{liveryTreeCar.CarsRoot.CustomSkinName}\\";
                        string carsFolder = "Cars\\";
                        zipArchive.AddEntry($"{carsFolder}{liveryTreeCar.CarsFile.Name}", liveryTreeCar.CarsFile);

                        DirectoryInfo customSkinDir = new DirectoryInfo(FileUtil.LiveriesPath + liveryTreeCar.CarsRoot.CustomSkinName);
                        if (customSkinDir.Exists)
                        {

                            FileInfo decalsPng = new FileInfo(customSkinDir.FullName + "\\" + "decals.png");
                            FileInfo decalsJson = new FileInfo(customSkinDir.FullName + "\\" + "decals.json");
                            FileInfo sponsorsPng = new FileInfo(customSkinDir.FullName + "\\" + "sponsors.png");
                            FileInfo sponsorsJson = new FileInfo(customSkinDir.FullName + "\\" + "sponsors.json");

                            if (decalsPng.Exists)
                                zipArchive.AddEntry($"{liveriesFolder}{decalsPng.Name}", decalsPng);
                            if (decalsJson.Exists)
                                zipArchive.AddEntry($"{liveriesFolder}{decalsJson.Name}", decalsJson);
                            if (sponsorsPng.Exists)
                                zipArchive.AddEntry($"{liveriesFolder}{sponsorsPng.Name}", sponsorsPng);
                            if (sponsorsJson.Exists)
                                zipArchive.AddEntry($"{liveriesFolder}{sponsorsJson.Name}", sponsorsJson);

                            FileInfo decalsDds0 = new FileInfo(customSkinDir.FullName + "\\" + "decals_0.dds");
                            //FileInfo decalsDds1 = new FileInfo(customSkinDir.FullName + "\\" + "decals_1.dds");
                            FileInfo sponsorsDds0 = new FileInfo(customSkinDir.FullName + "\\" + "sponsors_0.dds");
                            //FileInfo sponsorsDds1 = new FileInfo(customSkinDir.FullName + "\\" + "sponsors_1.dds");

                            if (decalsDds0.Exists)
                                zipArchive.AddEntry($"{liveriesFolder}{decalsDds0.Name}", decalsDds0);
                            //if (decalsDds1.Exists)
                            //    zipArchive.AddEntry($"{liveriesFolder}{decalsDds1.Name}", decalsDds1);
                            if (sponsorsDds0.Exists)
                                zipArchive.AddEntry($"{liveriesFolder}{sponsorsDds0.Name}", sponsorsDds0);
                            //if (sponsorsDds1.Exists)
                            //    zipArchive.AddEntry($"{liveriesFolder}{sponsorsDds1.Name}", sponsorsDds1);


                            using (FileStream outputStream = new FileStream(filename, FileMode.Create))
                            {
                                zipArchive.SaveTo(outputStream);
                                MainWindow.Instance.snackbar.MessageQueue.Enqueue($"Livery \"{liveryTreeCar.CarsRoot.TeamName}\" saved as: {filename}");
                            }
                        }
                    }
                }


            }
        }
    }
}
