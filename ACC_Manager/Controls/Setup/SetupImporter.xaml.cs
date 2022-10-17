using ACCManager.Controls.Setup;
using ACCManager.Data;
using ACCManager.Data.ACC.Tracks;
using ACCManager.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static ACCManager.Data.ACC.Tracks.TrackNames;
using static ACCManager.Data.ConversionFactory;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for SetupImporter.xaml
    /// </summary>
    public partial class SetupImporter : UserControl
    {
        internal static SetupImporter Instance { get; private set; }

        private string _originalSetupFile;
        private string _setupName;
        private SetupJson.Root _currentSetup;
        private FlowDocSetupRenderer _renderer;

        public SetupImporter()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden;

            buttonCancel.Click += (s, e) => Close();

            _renderer = new FlowDocSetupRenderer();

            Instance = this;
        }

        private void BuildTrackList()
        {
            this.listViewTracks.Items.Clear();
            foreach (KeyValuePair<string, TrackData> kv in TrackNames.Tracks)
            {
                ListViewItem trackItem = new ListViewItem()
                {
                    FontWeight = FontWeights.Bold,
                    Content = kv.Value.FullName,
                    DataContext = kv.Key
                };

                trackItem.MouseLeftButtonUp += (s, e) =>
                {
                    CarModels model = ConversionFactory.ParseCarName(_currentSetup.CarName);
                    string modelName = ConversionFactory.GetNameFromCarModel(model);

                    FileInfo targetFile = new FileInfo(FileUtil.AccPath + "Setups\\" + _currentSetup.CarName + "\\" + kv.Key + "\\" + _setupName + ".json");

                    if (targetFile.Exists)
                    {
                        MainWindow.Instance.EnqueueSnackbarMessage($"Setup already exists: {targetFile.FullName}");
                        Close();
                        return;
                    }

                    if (!targetFile.Directory.Exists)
                        targetFile.Directory.Create();

                    FileInfo originalFile = new FileInfo(_originalSetupFile);
                    if (originalFile.Exists)
                        originalFile.CopyTo(targetFile.FullName);

                    MainWindow.Instance.EnqueueSnackbarMessage($"Imported {_setupName} for {modelName} at {kv.Value.FullName}");

                    SetupBrowser.Instance.FetchAllSetups();
                    Close();
                };
                this.listViewTracks.Items.Add(trackItem);
            }
        }

        public bool Open(string setupFile, bool showTrack = false)
        {
            FileInfo file = new FileInfo(setupFile);
            if (!file.Exists)
                return false;

            SetupJson.Root setupRoot = ConversionFactory.GetSetupJsonRoot(file);
            if (setupRoot == null)
                return false;

            Debug.WriteLine($"Importing {file.FullName}");
            CarModels model = ConversionFactory.ParseCarName(setupRoot.CarName);
            string modelName = ConversionFactory.GetNameFromCarModel(model);
            Debug.WriteLine($"Trying to import a setup for {modelName}");
            _currentSetup = setupRoot;
            _setupName = file.Name.Replace(".json", "");
            _originalSetupFile = setupFile;

            BuildTrackList();
            this.textBlockSetupName.Text = $"{modelName} - {file.Name}";

            _renderer.LogSetup(ref this.flowDocument, setupFile, showTrack);

            this.Visibility = Visibility.Visible;
            SetupsTab.Instance.tabControl.IsEnabled = false;

            return true;
        }

        public void Close()
        {
            _setupName = String.Empty;
            _currentSetup = null;
            this.Visibility = Visibility.Hidden;
            this.listViewTracks.Items.Clear();
            this.flowDocument.Blocks.Clear();
            SetupsTab.Instance.tabControl.IsEnabled = true;
        }
    }
}
