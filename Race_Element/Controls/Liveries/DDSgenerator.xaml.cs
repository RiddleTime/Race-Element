using RaceElement.Controls.Liveries;
using RaceElement.LiveryParser;
using RaceElement.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static RaceElement.Controls.LiveryBrowser;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for DDSgenerator.xaml
    /// </summary>
    public partial class DDSgenerator : UserControl
    {
        internal static DDSgenerator Instance;

        private List<LiveryTreeCar> liveriesWithoutDDS = new();
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
            Cancel();
        }

        public void Cancel()
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
                    LiveryDisplayer.Instance.buttonGenerateDDS.IsEnabled = false;
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

                    Instance.Dispatcher.Invoke((Action)(() =>
                    {
                        todoList.Items.RemoveAt(0);
                        liveriesWithoutDDS.RemoveAt(0);
                    }));
                }

             
                Instance.Dispatcher.Invoke(() =>
                {
                    MainWindow.Instance.ClearSnackbar();
                    MainWindow.Instance.EnqueueSnackbarMessage($"Finished generating DDS Files.");

                    buttonGenerate.IsEnabled = true;
                    buttonGenerate.Content = "Start generating";
                    LiveryBrowser.Instance.liveriesTreeViewTeams.IsEnabled = true;
                    LiveryBrowser.Instance.liveriesTreeViewCars.IsEnabled = true;
                    LiveryBrowser.Instance.buttonImportLiveries.IsEnabled = true;
                    LiveryBrowser.Instance.buttonGenerateAllDDS.IsEnabled = true;
                    LiveryDisplayer.Instance.buttonGenerateDDS.IsEnabled = true;
                    LiveryDisplayer.Instance.ReloadLivery();
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

            DirectoryInfo customsCarsDirectory = new(FileUtil.CarsPath);

            List<LiveryTreeCar> liveryTreeCars = new();

            foreach (var carsFile in customsCarsDirectory.GetFiles())
            {
                if (carsFile.Extension != null && carsFile.Extension.Equals(".json"))
                {
                    CarsJson.Root carsRoot = LiveryImporter.GetLivery(carsFile);

                    if (carsRoot != null)
                    {
                        LiveryTreeCar treeCar = new() { CarsFile = carsFile, CarsRoot = carsRoot };

                        if (treeCar.CarsRoot.CustomSkinName != null && treeCar.CarsRoot.TeamName != null)
                            if (!treeCar.CarsRoot.CustomSkinName.Equals(string.Empty)
                                   && !treeCar.CarsRoot.TeamName.Equals(string.Empty)
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
                return $"{a.CarsRoot.TeamName}{a.CarsRoot.CustomSkinName}".CompareTo($"{b.CarsRoot.TeamName}{b.CarsRoot.CustomSkinName}");
            });

            liveryTreeCars.ForEach(x =>
            {
                ListBoxItem listBoxItem = new()
                {
                    AllowDrop = true,
                    Content = $"{x.CarsRoot.TeamName} / {x.CarsRoot.CustomSkinName}",
                    DataContext = x,
                };

                todoList.Items.Add(listBoxItem);
            });

            liveriesWithoutDDS = liveryTreeCars;
        }


    }
}
