using ACCSetupApp.LiveryParser;
using ACCSetupApp.SetupParser;
using ACCSetupApp.Util;
using Newtonsoft.Json;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using static ACCSetupApp.Controls.LiveryTagging;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for LiveryBrowser.xaml
    /// </summary>
    public partial class LiveryBrowser : UserControl
    {
        public static LiveryBrowser Instance;

        public LiveryBrowser()
        {
            InitializeComponent();

            Instance = this;
            ThreadPool.QueueUserWorkItem(x => FetchAllCars());

            liveriesTreeViewTeams.SelectedItemChanged += LiveriesTreeView_SelectedItemChanged;
            liveriesTreeViewCars.SelectedItemChanged += LiveriesTreeView_SelectedItemChanged;
            liveriesTreeViewTags.SelectedItemChanged += LiveriesTreeView_SelectedItemChanged;

            buttonImportLiveries.Click += ButtonImportLiveries_Click;
            buttonGenerateAllDDS.Click += ButtonGenerateAllDDS_Click;


            buttonNewTag.Click += (sender, args) =>
            {
                LiveryTagCreator.Instance.Open();
            };
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
                        }
                    }
                }
            }
        }

        internal class LiveryTreeCar
        {
            public FileInfo carsFile { get; set; }
            public CarsJson.Root carsRoot { get; set; }

            public override bool Equals(object obj)
            {
                return obj is LiveryTreeCar car &&
                       EqualityComparer<FileInfo>.Default.Equals(carsFile, car.carsFile) &&
                       EqualityComparer<CarsJson.Root>.Default.Equals(carsRoot, car.carsRoot);
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
                                LiveryTreeCar treeCar = new LiveryTreeCar() { carsFile = carsFile, carsRoot = carsRoot };

                                if (treeCar.carsRoot.customSkinName != null /*&& treeCar.carsRoot.teamName != null*/)
                                    if (!treeCar.carsRoot.customSkinName.Equals(string.Empty)
                                            //&& !treeCar.carsRoot.teamName.Equals(string.Empty)
                                            )
                                        liveryTreeCars.Add(treeCar);
                            }
                        }
                    }


                    var liveriesGroupedByCar = liveryTreeCars.GroupBy(g => ConversionFactory.GetCarName(g.carsRoot.carModelType));
                    var liveriesGroupedByTeam = liveryTreeCars.GroupBy(g => g.carsRoot.teamName);

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
                TreeViewItem modelItem = new TreeViewItem() { Header = teamHeader };
                modelItem.Expanded += (s, e) =>
                {
                    int targetItemInView = modelItem.Items.Count;
                    if (targetItemInView > 18)    // magic number :D (no just counted minimum size and made sure it will still show the track)
                        targetItemInView = 18;
                    ((TreeViewItem)modelItem.Items.GetItemAt(targetItemInView - 1)).BringIntoView();
                };

                var cars = tItem.ToList();
                cars.Sort((a, b) =>
                {
                    return $"{a.carsRoot.customSkinName.ToLower()}".CompareTo($"{b.carsRoot.customSkinName.ToLower()}");
                });
                foreach (LiveryTreeCar car in cars)
                {
                    TextBlock skinHeader = new TextBlock()
                    {
                        Text = $"{car.carsRoot.customSkinName}",
                        Style = Resources["MaterialDesignDataGridTextColumnStyle"] as Style,
                        TextTrimming = TextTrimming.WordEllipsis,
                        Width = liveriesTreeViewTeams.Width - 5
                    };
                    TreeViewItem skinItem = new TreeViewItem() { Header = skinHeader, DataContext = car };
                    skinItem.ContextMenu = GetSkinContextMenu(car, null);

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
                TreeViewItem teamItem = new TreeViewItem() { Header = teamHeader };
                teamItem.Expanded += (s, e) =>
                {
                    int targetItemInView = teamItem.Items.Count;
                    if (targetItemInView > 18)    // magic number :D (no just counted minimum size and made sure it will still show the track)
                        targetItemInView = 18;
                    ((TreeViewItem)teamItem.Items.GetItemAt(targetItemInView - 1)).BringIntoView();
                };

                var cars = tItem.ToList();
                cars.Sort((a, b) =>
                {
                    return $"{a.carsRoot.customSkinName.ToLower()}".CompareTo($"{b.carsRoot.customSkinName.ToLower()}");
                });

                foreach (LiveryTreeCar car in cars)
                {
                    TextBlock skinHeader = new TextBlock()
                    {
                        Text = $"{car.carsRoot.customSkinName}",
                        Style = Resources["MaterialDesignDataGridTextColumnStyle"] as Style,
                        TextTrimming = TextTrimming.WordEllipsis,
                        Width = liveriesTreeViewTeams.Width - 5
                    };
                    TreeViewItem skinItem = new TreeViewItem() { Header = skinHeader, DataContext = car };
                    skinItem.ContextMenu = GetSkinContextMenu(car, null);

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
            //LiveryTag tag = LiveryTagging.CreateNewTag("oNiD");

            List<TreeViewItem> tagTreeItems = new List<TreeViewItem>();

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
                TreeViewItem tagItem = new TreeViewItem() { Header = tagHeader };
                tagItem.Expanded += (s, e) =>
                {
                    int targetItemInView = tagItem.Items.Count;
                    if (targetItemInView > 18)    // magic number :D (no just counted minimum size and made sure it will still show the track)
                        targetItemInView = 18;
                    ((TreeViewItem)tagItem.Items.GetItemAt(targetItemInView - 1)).BringIntoView();
                };

                tagItem.ContextMenu = GetTagContextMenu(tagItem, liveryTag);
                tagCars.Sort((a, b) =>
                {
                    return $"{a.carsRoot.customSkinName.ToLower()}".CompareTo($"{b.carsRoot.customSkinName.ToLower()}");
                });

                foreach (LiveryTreeCar car in tagCars)
                {
                    TextBlock skinHeader = new TextBlock()
                    {
                        Text = $"{car.carsRoot.customSkinName}",
                        Style = Resources["MaterialDesignDataGridTextColumnStyle"] as Style,
                        TextTrimming = TextTrimming.WordEllipsis,
                        Width = liveriesTreeViewTeams.Width - 5
                    };
                    TreeViewItem skinItem = new TreeViewItem() { Header = skinHeader, DataContext = car };
                    skinItem.ContextMenu = GetSkinContextMenu(car, liveryTag);

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

            tagTreeItems.ForEach(x => liveriesTreeViewTags.Items.Add(x));

        }


        private ContextMenu GetSkinContextMenu(LiveryTreeCar liveryTreeCar, LiveryTag tag)
        {
            ContextMenu menu = new ContextMenu()
            {
                Style = Resources["MaterialDesignContextMenu"] as Style,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                UsesItemContainerTemplate = true,
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0))
            };

            Button openLiveryDirectory = new Button()
            {
                Content = $"Open Livery Directory",
                CommandParameter = liveryTreeCar,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            openLiveryDirectory.Click += OpenLiveryDirectory_Click;
            menu.Items.Add(openLiveryDirectory);

            Button openLiveryJson = new Button()
            {
                Content = $"Open Livery Json",
                CommandParameter = liveryTreeCar,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            openLiveryJson.Click += OpenLiveryJson_Click;
            menu.Items.Add(openLiveryJson);

            Button createZipButton = new Button()
            {
                Content = $"Save Skin as zip archive",
                CommandParameter = liveryTreeCar,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            createZipButton.Click += CreateZipButton_Click;
            menu.Items.Add(createZipButton);

            Button addSkinToSkinPack = new Button()
            {
                Content = $"Add to Skin Pack",
                CommandParameter = liveryTreeCar,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            addSkinToSkinPack.Click += AddSkinToSkinPack_Click;
            menu.Items.Add(addSkinToSkinPack);

            if (tag == null)
            {
                Button addSkinToTag = new Button()
                {
                    Content = $"Add to Tag",
                    CommandParameter = liveryTreeCar,
                    Style = Resources["MaterialDesignRaisedButton"] as Style,
                    Margin = new Thickness(0),
                    Height = 30,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                addSkinToTag.Click += AddSkinToTag_Click;
                menu.Items.Add(addSkinToTag);
            }
            else
            {
                Button removeSkinFromTag = new Button()
                {
                    Content = $"Remove from tag",
                    CommandParameter = liveryTreeCar,
                    Style = Resources["MaterialDesignRaisedButton"] as Style,
                    Margin = new Thickness(0),
                    Height = 30,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                removeSkinFromTag.Click += (e, s) =>
                {
                    LiveryTagging.RemoveFromTag(tag, liveryTreeCar);
                    menu.IsOpen = false;
                    FetchAllCars();
                };
                menu.Items.Add(removeSkinFromTag);
            }

            Button deleteLivery = new Button()
            {
                Content = $"Delete",
                CommandParameter = liveryTreeCar,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            deleteLivery.Click += DeleteLivery_Click;
            menu.Items.Add(deleteLivery);

            return menu;
        }

        private void AddSkinToTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(Button))
                {
                    Button button = (Button)sender;
                    (button.Parent as ContextMenu).IsOpen = false;

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
                if (sender.GetType() == typeof(Button))
                {
                    Button button = (Button)sender;
                    (button.Parent as ContextMenu).IsOpen = false;

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
                if (sender.GetType() == typeof(Button))
                {
                    Button button = (Button)sender;

                    LiveryTreeCar liveryTreeCar = (LiveryTreeCar)button.CommandParameter;
                    if (LiveryExporter.Instance.AddExportItem(liveryTreeCar))
                    {
                        MainWindow.Instance.EnqueueSnackbarMessage($"Added {liveryTreeCar.carsRoot.teamName}/{liveryTreeCar.carsRoot.customSkinName} to skin pack.");
                    }

                    (button.Parent as ContextMenu).IsOpen = false;
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
                if (sender.GetType() == typeof(Button))
                {

                    Button button = (Button)sender;

                    LiveryTreeCar liveryTreeCar = (LiveryTreeCar)button.CommandParameter;

                    if (liveryTreeCar.carsRoot.customSkinName == null || liveryTreeCar.carsRoot.customSkinName.Length == 0)
                    {
                        goto closeMenu;
                    }

                    FileInfo carsJsonFile = new FileInfo($"{liveryTreeCar.carsFile}");
                    Process.Start($"{FileUtil.CarsPath}{carsJsonFile.Name}");

                closeMenu:
                    (button.Parent as ContextMenu).IsOpen = false;
                }
            }
            catch (Exception ex) { LogWriter.WriteToLog(ex); }
        }

        private void OpenLiveryDirectory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(Button))
                {
                    Button button = (Button)sender;

                    LiveryTreeCar liveryTreeCar = (LiveryTreeCar)button.CommandParameter;

                    if (liveryTreeCar.carsRoot.customSkinName == null || liveryTreeCar.carsRoot.customSkinName.Length == 0)
                    {
                        goto closeMenu;
                    }

                    DirectoryInfo directory = new DirectoryInfo($"{FileUtil.LiveriesPath}{liveryTreeCar.carsRoot.customSkinName}");
                    Process.Start(directory.FullName);

                closeMenu:
                    (button.Parent as ContextMenu).IsOpen = false;
                }
            }
            catch (Exception ex) { LogWriter.WriteToLog(ex); }
        }

        private ContextMenu GetTagContextMenu(TreeViewItem teamItem, LiveryTag tag)
        {
            ContextMenu menu = new ContextMenu()
            {
                Style = Resources["MaterialDesignContextMenu"] as Style,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                UsesItemContainerTemplate = true,
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0))
            };

            Button addTeamToSkinPack = new Button()
            {
                Content = $"Add to Skin Pack",
                CommandParameter = teamItem,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            addTeamToSkinPack.Click += AddTeamToSkinPack_Click;
            menu.Items.Add(addTeamToSkinPack);

            Button deleteTagButton = new Button()
            {
                Content = $"Delete Tag",
                CommandParameter = teamItem,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                ToolTip = "Warning! This permantely deletes this tag!",
                VerticalAlignment = VerticalAlignment.Center,
            };
            deleteTagButton.Click += (e, s) =>
            {
                LiveryTagging.DeleteTag(tag);
                menu.IsOpen = false;
            };
            menu.Items.Add(deleteTagButton);

            return menu;
        }

        private ContextMenu GetTeamContextMenu(TreeViewItem teamItem)
        {
            ContextMenu menu = new ContextMenu()
            {
                Style = Resources["MaterialDesignContextMenu"] as Style,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                UsesItemContainerTemplate = true,
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0))
            };

            Button addTeamToSkinPack = new Button()
            {
                Content = $"Add to Skin Pack",
                CommandParameter = teamItem,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            addTeamToSkinPack.Click += AddTeamToSkinPack_Click;
            menu.Items.Add(addTeamToSkinPack);

            Button addTeamToTag = new Button()
            {
                Content = $"Add to Tag",
                CommandParameter = teamItem,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            addTeamToTag.Click += AddTeamToTag_Click;
            menu.Items.Add(addTeamToTag);

            return menu;
        }

        private void AddTeamToTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(Button))
                {
                    Button button = (Button)sender;

                    TreeViewItem treeItem = (TreeViewItem)button.CommandParameter;
                    List<LiveryTreeCar> treeCars = new List<LiveryTreeCar>();

                    treeItem.Items.OfType<TreeViewItem>().ToList().ForEach(x =>
                    {
                        treeCars.Add((LiveryTreeCar)x.DataContext);
                    });

                    LiveryTagger.Instance.Open(treeCars);

                    (button.Parent as ContextMenu).IsOpen = false;
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
                if (sender.GetType() == typeof(Button))
                {
                    Button button = (Button)sender;

                    TreeViewItem treeItem = (TreeViewItem)button.CommandParameter;
                    List<LiveryTreeCar> treeCars = new List<LiveryTreeCar>();

                    treeItem.Items.OfType<TreeViewItem>().ToList().ForEach(x =>
                    {
                        LiveryExporter.Instance.AddExportItem((LiveryTreeCar)x.DataContext);
                    });

                    (button.Parent as ContextMenu).IsOpen = false;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }
        }

        private void CreateZipButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(Button))
            {
                Button button = (Button)sender;

                LiveryTreeCar liveryTreeCar = (LiveryTreeCar)button.CommandParameter;

                if (liveryTreeCar.carsRoot.customSkinName == null || liveryTreeCar.carsRoot.customSkinName.Length == 0)
                {
                    goto closeMenu;
                }

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

                // Set filter for file extension and default file extension 
                dlg.DefaultExt = ".zip";
                dlg.AddExtension = true;
                dlg.CheckPathExists = true;
                dlg.FileName = $"{liveryTreeCar.carsRoot.customSkinName}";
                dlg.DefaultExt = ".zip";
                dlg.Filter = "Livery zip|*.zip";
                Nullable<bool> result = dlg.ShowDialog();


                // Get the selected file name and display in a TextBox 
                if (result == true)
                {
                    // Open document 
                    string filename = dlg.FileName;

                    if (filename == null)
                        goto closeMenu;

                    using (ZipArchive zipArchive = ZipArchive.Create())
                    {
                        string liveriesFolder = $"Liveries\\{liveryTreeCar.carsRoot.customSkinName}\\";
                        string carsFolder = "Cars\\";
                        zipArchive.AddEntry($"{carsFolder}{liveryTreeCar.carsFile.Name}", liveryTreeCar.carsFile);

                        DirectoryInfo customSkinDir = new DirectoryInfo(FileUtil.LiveriesPath + liveryTreeCar.carsRoot.customSkinName);
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
                            FileInfo decalsDds1 = new FileInfo(customSkinDir.FullName + "\\" + "decals_1.dds");
                            FileInfo sponsorsDds0 = new FileInfo(customSkinDir.FullName + "\\" + "sponsors_0.dds");
                            FileInfo sponsorsDds1 = new FileInfo(customSkinDir.FullName + "\\" + "sponsors_1.dds");

                            if (decalsDds0.Exists)
                                zipArchive.AddEntry($"{liveriesFolder}{decalsDds0.Name}", decalsDds0);
                            if (decalsDds1.Exists)
                                zipArchive.AddEntry($"{liveriesFolder}{decalsDds1.Name}", decalsDds1);
                            if (sponsorsDds0.Exists)
                                zipArchive.AddEntry($"{liveriesFolder}{sponsorsDds0.Name}", sponsorsDds0);
                            if (sponsorsDds1.Exists)
                                zipArchive.AddEntry($"{liveriesFolder}{sponsorsDds1.Name}", sponsorsDds1);


                            using (FileStream outputStream = new FileStream(filename, FileMode.Create))
                            {
                                zipArchive.SaveTo(outputStream);
                                MainWindow.Instance.snackbar.MessageQueue.Enqueue($"Livery \"{liveryTreeCar.carsRoot.teamName}\" saved as: {filename}");
                            }
                        }
                    }
                }

            closeMenu:
                (button.Parent as ContextMenu).IsOpen = false;
            }
        }
    }
}
