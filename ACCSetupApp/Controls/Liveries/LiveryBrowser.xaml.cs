using ACCSetupApp.LiveryParser;
using Newtonsoft.Json;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Cache;
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

        public LiveryBrowser()
        {
            InitializeComponent();

            FetchAllCars();

            liveriesTreeView.SelectedItemChanged += LiveriesTreeView_SelectedItemChanged;
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
                            CarsJson.Root carsRoot = treeCar.carsRoot;
                            string customSkinName = carsRoot.customSkinName;

                            if (customSkinName != null && customSkinName.Length > 0)
                            {
                                DirectoryInfo customSkinDir = new DirectoryInfo(LiveriesPath + customSkinName);
                                if (customSkinDir.Exists)
                                {
                                    FileInfo[] foundFiles = customSkinDir.GetFiles("decals.png");
                                    if (foundFiles != null && foundFiles.Length > 0)
                                    {
                                        FileInfo decalsFile = foundFiles[0];

                                        liveryImage.Source = new BitmapImage(new Uri(decalsFile.FullName, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
                                    }
                                    else
                                    {
                                        liveryImage.Source = null;
                                    }
                                }
                            }
                            else
                            {
                                liveryImage.Source = null;
                            }
                        }
                    }
                }
            }
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

                    ZipArchive zipArchive = ZipArchive.Create();
                    zipArchive.AddEntry(liveryTreeCar.carsFile.Name, liveryTreeCar.carsFile);

                    DirectoryInfo customSkinDir = new DirectoryInfo(LiveriesPath + liveryTreeCar.carsRoot.customSkinName);
                    if (customSkinDir.Exists)
                    {
                        FileInfo decalsPng = new FileInfo(customSkinDir.FullName + "\\" + "decals.png");
                        FileInfo decalsJson = new FileInfo(customSkinDir.FullName + "\\" + "decals.json");
                        FileInfo sponsorsPng = new FileInfo(customSkinDir.FullName + "\\" + "sponsors.png");
                        FileInfo sponsorsJson = new FileInfo(customSkinDir.FullName + "\\" + "sponsors.json"); ;

                        if (!decalsJson.Exists || !decalsJson.Exists || !sponsorsJson.Exists || !sponsorsPng.Exists)
                            goto closeMenu;

                        zipArchive.AddEntry(decalsPng.Name, decalsPng);
                        zipArchive.AddEntry(decalsJson.Name, decalsJson);
                        zipArchive.AddEntry(sponsorsPng.Name, sponsorsPng);
                        zipArchive.AddEntry(sponsorsJson.Name, sponsorsJson);

                        using (FileStream outputStream = new FileStream(filename, FileMode.Create))
                        {
                            zipArchive.SaveTo(outputStream);
                            MainWindow.Instance.snackbar.MessageQueue.Enqueue($"Livery Zip saved: {filename}");
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
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            CarsJson.Root carLiveryRoot = JsonConvert.DeserializeObject<CarsJson.Root>(jsonString);
            return carLiveryRoot;
        }
    }
}
