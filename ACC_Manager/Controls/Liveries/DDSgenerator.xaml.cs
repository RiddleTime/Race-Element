using ACCSetupApp.Controls.Liveries;
using ACCSetupApp.LiveryParser;
using ACCSetupApp.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using static ACCSetupApp.Controls.LiveryBrowser;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for DDSgenerator.xaml
    /// </summary>
    public partial class DDSgenerator : UserControl
    {
        internal static DDSgenerator Instance;

        private List<LiveryTreeCar> liveriesWithoutDDS = new List<LiveryTreeCar>();
        private bool Generating = false;

        public DDSgenerator()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden;

            this.IsVisibleChanged += DDSgenerator_IsVisibleChanged;

            buttonGenerate.Click += ButtonGenerate_Click;
            buttonCancel.Click += ButtonCancel_Click;

            Instance = this;
        }

        private void DDSgenerator_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                LoadLiveriesWithoutDDS();
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!this.Generating)
            {
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                buttonCancel.IsEnabled = false;
                buttonCancel.Content = "Cancelling...";
                Generating = false;
            }
        }

        private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            new Thread(x =>
            {
                Generating = true;

                Instance.Dispatcher.Invoke(() =>
                {
                    buttonGenerate.IsEnabled = false;
                    buttonGenerate.Content = "Generating dds files... this may take a while";
                    buttonCancel.IsEnabled = true;
                    LiveryBrowser.Instance.liveriesTreeViewTeams.IsEnabled = false;
                    LiveryBrowser.Instance.liveriesTreeViewCars.IsEnabled = false;
                    LiveryBrowser.Instance.buttonImportLiveries.IsEnabled = false;
                    LiveryBrowser.Instance.buttonGenerateAllDDS.IsEnabled = false;

                });

                while (liveriesWithoutDDS.Count > 0 && Generating)
                {
                    Instance.Dispatcher.Invoke(() =>
                    {
                        todoList.SelectedIndex = 0;
                        (todoList.SelectedItem as ListBoxItem).Background = Brushes.Orange;
                    });

                    LiveryTreeCar livery = liveriesWithoutDDS.First();
                    DDSutil.GenerateDDS(livery);

                    Instance.Dispatcher.Invoke(() =>
                    {
                        MainWindow.Instance.EnqueueSnackbarMessage($"Generated DDS files for {livery.carsRoot.teamName}{livery.carsRoot.customSkinName}");
                        todoList.Items.RemoveAt(0);
                        liveriesWithoutDDS.RemoveAt(0);
                    });
                }

                Instance.Dispatcher.Invoke(() =>
                {
                    buttonGenerate.IsEnabled = true;
                    buttonGenerate.Content = "Start generating";
                    LiveryBrowser.Instance.liveriesTreeViewTeams.IsEnabled = true;
                    LiveryBrowser.Instance.liveriesTreeViewCars.IsEnabled = true;
                    LiveryBrowser.Instance.buttonImportLiveries.IsEnabled = true;
                    LiveryBrowser.Instance.buttonGenerateAllDDS.IsEnabled = true;

                    if (!Generating)
                    {
                        buttonCancel.IsEnabled = true;
                        buttonCancel.Content = "Cancel";
                    }

                    Generating = false;
                    LoadLiveriesWithoutDDS();
                });


            }).Start();
        }

        private void LoadLiveriesWithoutDDS()
        {
            todoList.Items.Clear();
            liveriesWithoutDDS.Clear();

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

                        if (treeCar.carsRoot.customSkinName != null && treeCar.carsRoot.teamName != null)
                            if (!treeCar.carsRoot.customSkinName.Equals(string.Empty)
                                   && !treeCar.carsRoot.teamName.Equals(string.Empty)
                                    )
                            {
                                if (!DDSutil.HasDdsFiles(treeCar))
                                    liveryTreeCars.Add(treeCar);

                            }
                    }
                }
            }

            if (liveryTreeCars.Count == 0)
            {
                buttonGenerate.Content = "All DDS files available";
                buttonGenerate.IsEnabled = false;
            }
            else
            {
                buttonGenerate.Content = "Start generating";
                buttonGenerate.IsEnabled = true;
            }

            liveryTreeCars.Sort((a, b) =>
            {
                return $"{a.carsRoot.teamName}{a.carsRoot.customSkinName}".CompareTo($"{b.carsRoot.teamName}{b.carsRoot.customSkinName}");
            });

            liveryTreeCars.ForEach(x =>
            {
                ListBoxItem listBoxItem = new ListBoxItem()
                {
                    AllowDrop = true,
                    Content = $"{x.carsRoot.teamName} / {x.carsRoot.customSkinName}",
                    DataContext = x,
                };

                todoList.Items.Add(listBoxItem);
            });

            liveriesWithoutDDS = liveryTreeCars;
        }


    }
}
