using RaceElement.Util;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static RaceElement.Controls.LiveryBrowser;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for LiveryExporter.xaml
    /// </summary>
    public partial class LiveryExporter : UserControl
    {
        public static LiveryExporter Instance { get; private set; }

        private List<LiveryTreeCar> exportItems = new();

        public LiveryExporter()
        {
            InitializeComponent();
            Instance = this;

            this.Visibility = Visibility.Hidden;

            buttonExport.Click += ButtonExport_Click;
            stackPanelToggleExportDDS.MouseUp += (s, e) => toggleExportDDS.IsChecked = !toggleExportDDS.IsChecked;
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool shouldExportDDS = toggleExportDDS.IsChecked.Value;

                ThreadPool.QueueUserWorkItem((WaitCallback)(x =>
                {
                    Microsoft.Win32.SaveFileDialog dlg = new();

                    // Set filter for file extension and default file extension 
                    dlg.DefaultExt = ".zip";
                    dlg.AddExtension = true;
                    dlg.CheckPathExists = true;
                    dlg.FileName = $"Rename_Me.zip";
                    dlg.DefaultExt = ".zip";
                    dlg.Filter = "Livery zip|*.zip";
                    bool? result = dlg.ShowDialog();


                    // Get the selected file name and display in a TextBox 
                    if (result == true)
                    {
                        // Open document 
                        string filename = dlg.FileName;

                        if (filename == null)
                        {
                            MainWindow.Instance.EnqueueSnackbarMessage("Please enter a file name");
                            return;
                        }
                        else
                        {
                            Instance.Dispatcher.Invoke(new Action(() =>
                            {
                                this.IsEnabled = false;
                                buttonExport.Content = "Exporting archive... please wait.";
                                MainWindow.Instance.EnqueueSnackbarMessage("Archiving selected skins...");
                            }));
                        }

                        using (ZipArchive zipArchive = ZipArchive.Create())
                        {
                            foreach (LiveryTreeCar liveryTreeCar in exportItems)
                            {
                                string liveriesFolder = $"Liveries\\{liveryTreeCar.CarsRoot.CustomSkinName}\\";
                                string carsFolder = "Cars\\";
                                zipArchive.AddEntry($"{carsFolder}{liveryTreeCar.CarsFile.Name}", liveryTreeCar.CarsFile);

                                DirectoryInfo customSkinDir = new(FileUtil.LiveriesPath + liveryTreeCar.CarsRoot.CustomSkinName);
                                if (customSkinDir.Exists)
                                {

                                    FileInfo decalsPng = new(customSkinDir.FullName + "\\" + "decals.png");
                                    FileInfo decalsJson = new(customSkinDir.FullName + "\\" + "decals.json");
                                    FileInfo sponsorsPng = new(customSkinDir.FullName + "\\" + "sponsors.png");
                                    FileInfo sponsorsJson = new(customSkinDir.FullName + "\\" + "sponsors.json"); ;

                                    if (decalsPng.Exists)
                                        zipArchive.AddEntry($"{liveriesFolder}{decalsPng.Name}", decalsPng);
                                    if (decalsJson.Exists)
                                        zipArchive.AddEntry($"{liveriesFolder}{decalsJson.Name}", decalsJson);
                                    if (sponsorsPng.Exists)
                                        zipArchive.AddEntry($"{liveriesFolder}{sponsorsPng.Name}", sponsorsPng);
                                    if (sponsorsJson.Exists)
                                        zipArchive.AddEntry($"{liveriesFolder}{sponsorsJson.Name}", sponsorsJson);


                                    if (shouldExportDDS)
                                    {
                                        //FileInfo decalsDds0 = new FileInfo(customSkinDir.FullName + "\\" + "decals_0.dds");
                                        FileInfo decalsDds1 = new(customSkinDir.FullName + "\\" + "decals_1.dds");
                                        //FileInfo sponsorsDds0 = new FileInfo(customSkinDir.FullName + "\\" + "sponsors_0.dds");
                                        FileInfo sponsorsDds1 = new(customSkinDir.FullName + "\\" + "sponsors_1.dds");

                                        //if (decalsDds0.Exists)
                                        //    zipArchive.AddEntry($"{liveriesFolder}{decalsDds0.Name}", decalsDds0);
                                        if (decalsDds1.Exists)
                                            zipArchive.AddEntry($"{liveriesFolder}{decalsDds1.Name}", decalsDds1);
                                        //if (sponsorsDds0.Exists)
                                        //    zipArchive.AddEntry($"{liveriesFolder}{sponsorsDds0.Name}", sponsorsDds0);
                                        if (sponsorsDds1.Exists)
                                            zipArchive.AddEntry($"{liveriesFolder}{sponsorsDds1.Name}", sponsorsDds1);
                                    }
                                }
                            }

                            using (FileStream outputStream = new(filename, FileMode.Create))
                            {
                                zipArchive.SaveTo(outputStream);
                                outputStream.Close();
                            }

                            Instance.Dispatcher.Invoke(() =>
                            {
                                MainWindow.Instance.ClearSnackbar();
                                MainWindow.Instance.EnqueueSnackbarMessage($"Skin pack saved as: {filename}", "Open in explorer", () =>
                                {
                                    Process.Start($"explorer", $"/select,{filename}");
                                });

                                exportItems.Clear();
                                RebuildListView();
                                this.IsEnabled = true;
                                buttonExport.Content = "Export as zip";
                            });
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }
            finally
            {

            }
        }

        internal bool AddExportItem(LiveryTreeCar skin)
        {

            if (!this.IsEnabled)
            {
                MainWindow.Instance.EnqueueSnackbarMessage($"Already exporting a skin pack, wait for it to complete archiving!");
                return false;
            }

            if (exportItems.Contains(skin))
            {
                MainWindow.Instance.EnqueueSnackbarMessage($"Skin pack already contains {skin.CarsRoot.TeamName}/{skin.CarsRoot.CustomSkinName}.");
                return false;
            }

            exportItems.Add(skin);

            RebuildListView();

            return true;
        }

        private void RebuildListView()
        {
            try
            {
                exportItems.Sort((a, b) =>
                {
                    return $"{a.CarsRoot.TeamName}{a.CarsRoot.CustomSkinName}".CompareTo($"{b.CarsRoot.TeamName}{b.CarsRoot.CustomSkinName}");
                });

                exportList.Items.Clear();
                exportItems.ForEach(x =>
                {
                    ListBoxItem listBoxItem = new()
                    {
                        AllowDrop = true,
                        Content = $"{x.CarsRoot.TeamName} / {x.CarsRoot.CustomSkinName}",
                        DataContext = x,
                        ToolTip = "Click to remove from skin pack"
                    };
                    listBoxItem.MouseLeftButtonUp += ListBoxItem_MouseLeftButtonUp;
                    exportList.Items.Add(listBoxItem);
                });
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog(e);
            }

            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            this.Visibility = exportItems.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        public void Cancel()
        {
            exportItems.Clear();
            UpdateVisibility();
        }

        private void ListBoxItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (sender is ListBoxItem listBoxItem)
            {
                exportList.Items.Remove(listBoxItem);

                if (listBoxItem.DataContext is LiveryTreeCar car)
                {
                    exportItems.Remove(car);
                }

                UpdateVisibility();
            }

        }
    }
}
