using ACCSetupApp.LiveryParser;
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

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for LiveryBrowser.xaml
    /// </summary>
    public partial class LiveryBrowser : UserControl
    {
        private string AccPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Assetto Corsa Competizione\\";
        private string CustomsPath => AccPath + "Customs\\";
        private string CarsPath => CustomsPath + "Cars\\";
        private string LiveriesPath => CustomsPath + "Liveries\\";

        Regex carsJsonRegex = new Regex("^[0-9]*-[0-9]*-[0-9]*.json");

        public LiveryBrowser()
        {
            InitializeComponent();

            FetchAllCars();

            liveriesTreeView.SelectedItemChanged += LiveriesTreeView_SelectedItemChanged;

            buttonImportLiveries.Click += ButtonImportLiveries_Click;
        }

        private void ButtonImportLiveries_Click(object sender, RoutedEventArgs e)
        {
            ImportLiveryZips();
        }

        private void LiveriesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            decalsImage.Source = null;
            sponsorsImage.Source = null;

            ThreadPool.QueueUserWorkItem(x => { GC.Collect(); });

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
                            CarsJson.Root carsRoot = treeCar.carsRoot;
                            string customSkinName = carsRoot.customSkinName;

                            if (carsRoot != null)
                            {
                                stackPanelLiveryInfo.Children.Clear();
                                stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Team: {carsRoot.teamName}", HorizontalAlignment.Left, 18));
                                if (customSkinName != String.Empty)
                                    stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Skin: {customSkinName}", HorizontalAlignment.Left, 15));
                                stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Display Name: {carsRoot.displayName}"));
                                stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Race Number: {carsRoot.raceNumber}"));
                            }

                            if (customSkinName != null && customSkinName.Length > 0)
                            {
                                DirectoryInfo customSkinDir = new DirectoryInfo(LiveriesPath + customSkinName);
                                if (customSkinDir.Exists)
                                {
                                    FileInfo[] sponporsFiles = customSkinDir.GetFiles("sponsors.png");
                                    if (sponporsFiles != null && sponporsFiles.Length > 0)
                                    {
                                        FileInfo sponsorsFile = sponporsFiles[0];

                                        sponsorsImage.Source = new BitmapImage(new Uri(sponsorsFile.FullName, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
                                    }

                                    FileInfo[] decalFiles = customSkinDir.GetFiles("decals.png");
                                    if (decalFiles != null && decalFiles.Length > 0)
                                    {
                                        FileInfo decalsFile = decalFiles[0];

                                        decalsImage.Source = new BitmapImage(new Uri(decalsFile.FullName, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private Label GetInfoLabel(string text, HorizontalAlignment allignmment = HorizontalAlignment.Left, int size = 13)
        {
            Label label = new Label()
            {
                Content = text,
                Style = Resources["MaterialDesignLabel"] as Style,
                FontSize = size,
                HorizontalAlignment = allignmment
            };
            return label;
        }

        private class LiveryTreeCar
        {
            public FileInfo carsFile { get; set; }
            public CarsJson.Root carsRoot { get; set; }
        }

        private void FetchAllCars()
        {
            liveriesTreeView.Items.Clear();

            DirectoryInfo customsCarsDirectory = new DirectoryInfo(CarsPath);

            foreach (var carsFile in customsCarsDirectory.GetFiles())
            {
                if (carsFile.Extension.Equals(".json"))
                {
                    CarsJson.Root carsRoot = GetLivery(carsFile);

                    if (carsRoot != null)
                    {
                        TextBlock liveryHeader = new TextBlock()
                        {
                            Text = $"{carsRoot.teamName}",
                            Style = Resources["MaterialDesignSubtitle1TextBlock"] as Style,
                        };

                        LiveryTreeCar treeCar = new LiveryTreeCar() { carsFile = carsFile, carsRoot = carsRoot };
                        TreeViewItem teamItem = new TreeViewItem() { Header = liveryHeader, DataContext = treeCar };
                        teamItem.ContextMenu = GetTeamContextMenu(treeCar);

                        liveriesTreeView.Items.Add(teamItem);
                    }
                }
            }
        }

        private ContextMenu GetTeamContextMenu(LiveryTreeCar directory)
        {
            ContextMenu menu = new ContextMenu()
            {
                Style = Resources["MaterialDesignContextMenu"] as Style,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                UsesItemContainerTemplate = true,
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0))
            };


            Button createZipButton = new Button()
            {
                Content = "Save Livery.zip As",
                CommandParameter = directory,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center,
            };
            createZipButton.Click += CreateZipButton_Click;

            menu.Items.Add(createZipButton);

            return menu;
        }

        private void ImportLiveryZips()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.AddExtension = true;
            dlg.CheckPathExists = true;

            dlg.Filter = "Livery zip(s)|*.zip|Livery rar(s)|*.rar|Livery 7z(s)|*.7z";
            dlg.Multiselect = true;
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string[] fileNames = dlg.FileNames;

                if (fileNames != null && fileNames.Length > 0)
                {
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        FileInfo fi = new FileInfo(fileNames[i]);
                        if (fi.Exists)
                        {
                            IArchive archive = null;

                            switch (fi.Extension)
                            {
                                case ".7z":
                                case ".zip":
                                case ".rar": archive = ArchiveFactory.Open(fileNames[i]); break;
                            }

                            string decalsJson = string.Empty;
                            string decalsPng = string.Empty;
                            string sponsorsJson = string.Empty;
                            string sponsorsPng = string.Empty;
                            string carsJson = string.Empty;

                            archive.Entries.ToList().ForEach(x =>
                            {
                                //Debug.WriteLine(x.Key);
                                if (!x.Key.EndsWith("/") && !x.Key.EndsWith("\\"))
                                {
                                    string key = GetFileName(x.Key);
                                    switch (key.ToLower())
                                    {
                                        case "decals.json":
                                            {
                                                decalsJson = x.Key;
                                                Debug.WriteLine($"decals.json : " + x.Key);
                                                break;
                                            }
                                        case "decals.png":
                                            {
                                                decalsPng = x.Key;
                                                Debug.WriteLine($"decals.png : " + x.Key);
                                                break;
                                            }
                                        case "sponsors.json":
                                            {
                                                sponsorsJson = x.Key;
                                                Debug.WriteLine($"sponsors.json : " + x.Key);
                                                break;
                                            }
                                        case "sponsors.png":
                                            {
                                                sponsorsPng = x.Key;
                                                Debug.WriteLine($"sponsors.png : " + x.Key);
                                                break;
                                            }
                                        default:
                                            {
                                                if (carsJsonRegex.IsMatch(GetFileName(x.Key)))
                                                {
                                                    carsJson = x.Key;
                                                    Debug.WriteLine($"team/car json: {x.Key}");
                                                }
                                                else
                                                {
                                                    if (x.Key.EndsWith(".json"))
                                                    {
                                                        carsJson = x.Key;
                                                        Debug.WriteLine($"team/car json: {x.Key}");
                                                    }
                                                }
                                                break;
                                            }
                                    }
                                }
                            });

                            Debug.WriteLine("yeaaah we can unpack this archive in the correct folders!");

                            CarsJson.Root root = null;

                            archive.Entries.Where(e => e.Key == carsJson).ToList().ForEach(e =>
                            {
                                string carsJsonFileName = $"{CarsPath}{GetFileName(e.Key)}";

                                e.WriteToFile(carsJsonFileName);
                                root = GetLivery(new FileInfo(carsJsonFileName));
                            });

                            if (root != null)
                            {
                                string liveryFolder = $"{LiveriesPath}{root.customSkinName}\\";
                                Directory.CreateDirectory(liveryFolder);

                                archive.Entries.Where(e => e.Key == sponsorsJson).ToList().ForEach(e =>
                                {
                                    string sponsorsJsonFile = $"{liveryFolder}{GetFileName(e.Key)}";
                                    e.WriteToFile(sponsorsJsonFile);
                                });
                                archive.Entries.Where(e => e.Key == sponsorsPng).ToList().ForEach(e =>
                                {
                                    string sponsorsPngFile = $"{liveryFolder}{GetFileName(e.Key)}";
                                    e.WriteToFile(sponsorsPngFile);
                                });
                                archive.Entries.Where(e => e.Key == decalsJson).ToList().ForEach(e =>
                                {
                                    string decalsJsonFile = $"{liveryFolder}{GetFileName(e.Key)}";
                                    e.WriteToFile(decalsJsonFile);
                                });
                                archive.Entries.Where(e => e.Key == decalsPng).ToList().ForEach(e =>
                                {
                                    string decalsPngFile = $"{liveryFolder}{GetFileName(e.Key)}";
                                    e.WriteToFile(decalsPngFile);
                                });

                                Debug.WriteLine($"Installed custom livery: {root.teamName}");
                            }
                            else
                            {
                                Debug.WriteLine("No valid cars json found");
                            }

                        }
                    }

                }

                FetchAllCars();
            }
        }

        private string GetFileName(string fullName)
        {
            string[] split = fullName.Split('/');

            if (split.Length == 1 && split[0].Contains("\\"))
            {
                split = fullName.Split('\\');
            }
            return split[split.Length - 1].Replace("\\", "");
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
                        zipArchive.AddEntry(liveryTreeCar.carsFile.Name, liveryTreeCar.carsFile);

                        DirectoryInfo customSkinDir = new DirectoryInfo(LiveriesPath + liveryTreeCar.carsRoot.customSkinName);
                        if (customSkinDir.Exists)
                        {
                            FileInfo decalsPng = new FileInfo(customSkinDir.FullName + "\\" + "decals.png");
                            FileInfo decalsJson = new FileInfo(customSkinDir.FullName + "\\" + "decals.json");
                            FileInfo sponsorsPng = new FileInfo(customSkinDir.FullName + "\\" + "sponsors.png");
                            FileInfo sponsorsJson = new FileInfo(customSkinDir.FullName + "\\" + "sponsors.json"); ;

                            if (decalsPng.Exists)
                                zipArchive.AddEntry(decalsPng.Name, decalsPng);
                            if (decalsJson.Exists)
                                zipArchive.AddEntry(decalsJson.Name, decalsJson);
                            if (sponsorsPng.Exists)
                                zipArchive.AddEntry(sponsorsPng.Name, sponsorsPng);
                            if (sponsorsJson.Exists)
                                zipArchive.AddEntry(sponsorsJson.Name, sponsorsJson);

                            using (FileStream outputStream = new FileStream(filename, FileMode.Create))
                            {
                                zipArchive.SaveTo(outputStream);
                                MainWindow.Instance.snackbar.MessageQueue.Enqueue($"Livery Zip saved: {filename}");
                            }
                        }
                    }
                }

            closeMenu:
                (button.Parent as ContextMenu).IsOpen = false;
            }
        }

        public CarsJson.Root GetLivery(FileInfo file)
        {
            if (!file.Exists)
                return null;
            CarsJson.Root carLiveryRoot = null;
            string jsonString = string.Empty;
            try
            {
                using (FileStream fileStream = file.OpenRead())
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        jsonString = reader.ReadToEnd();
                        jsonString = jsonString.Replace("\0", "");
                        reader.Close();
                        fileStream.Close();
                    }
                }

                carLiveryRoot = JsonConvert.DeserializeObject<CarsJson.Root>(jsonString);
            }
            catch (Exception e)
            {
                //Debug.WriteLine(e);
            }

            return carLiveryRoot;
        }
    }
}
