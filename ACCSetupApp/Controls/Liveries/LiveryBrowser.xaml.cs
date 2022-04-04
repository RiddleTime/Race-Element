using ACCSetupApp.LiveryParser;
using Newtonsoft.Json;
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
                        if (item.DataContext.GetType() == typeof(CarsJson.Root))
                        {
                            CarsJson.Root carsRoot = (CarsJson.Root)item.DataContext;
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
                    TreeViewItem teamItem = new TreeViewItem() { Header = liveryHeader, DataContext = carsRoot };

                    liveriesTreeView.Items.Add(teamItem);

                }
            }
        }

        private ContextMenu GetTeamContextMenu(DirectoryInfo directory)
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
