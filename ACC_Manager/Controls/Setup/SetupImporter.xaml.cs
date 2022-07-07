using ACCManager.Data;
using ACCManager.Data.ACC.Tracks;
using ACCManager.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using static ACCManager.Data.ConversionFactory;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for SetupImporter.xaml
    /// </summary>
    public partial class SetupImporter : UserControl
    {
        private static SetupImporter _instance;
        internal static SetupImporter Instance { get { return _instance; } }

        private string _originalSetupFile;
        private string _setupName;
        private SetupJson.Root _currentSetup;

        public SetupImporter()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden;

            buttonCancel.Click += (s, e) => Close();
            _instance = this;
        }

        private void BuildTrackList()
        {
            this.listViewTracks.Items.Clear();
            foreach (KeyValuePair<string, string> kv in TrackNames.Tracks)
            {
                ListViewItem trackItem = new ListViewItem()
                {
                    FontWeight = FontWeights.Bold,
                    Content = kv.Value,
                    DataContext = kv.Key
                };

                trackItem.MouseLeftButtonUp += (s, e) =>
                {
                    CarModels model = ConversionFactory.ParseCarName(_currentSetup.carName);
                    string modelName = ConversionFactory.GetNameFromCarModel(model);

                    FileInfo targetFile = new FileInfo(FileUtil.AccPath + "Setups\\" + _currentSetup.carName + "\\" + kv.Key + "\\" + _setupName + ".json");

                    if (targetFile.Exists)
                    {
                        MainWindow.Instance.EnqueueSnackbarMessage($"Setup already exists: {targetFile.FullName}");
                        Close();
                        return;
                    }

                    FileInfo originalFile = new FileInfo(_originalSetupFile);
                    if (originalFile.Exists)
                        originalFile.CopyTo(targetFile.FullName);

                    MainWindow.Instance.EnqueueSnackbarMessage($"Imported {_setupName} for {modelName} at {kv.Value}");

                    SetupBrowser.Instance.FetchAllSetups();
                    Close();
                };
                this.listViewTracks.Items.Add(trackItem);
            }
        }

        public void Open(string setupFile)
        {
            FileInfo file = new FileInfo(setupFile);
            if (!file.Exists)
                return;

            SetupJson.Root setupRoot = ConversionFactory.GetSetupJsonRoot(file);
            if (setupRoot == null)
                return;

            Debug.WriteLine($"Importing {file.FullName}");
            CarModels model = ConversionFactory.ParseCarName(setupRoot.carName);
            string modelName = ConversionFactory.GetNameFromCarModel(model);
            Debug.WriteLine($"Trying to import a setup for {modelName}");
            _currentSetup = setupRoot;
            _setupName = file.Name;
            _originalSetupFile = setupFile;

            BuildTrackList();
            this.textBlockSetupName.Text = $"{modelName} - {file.Name}";

            this.Visibility = Visibility.Visible;
            SetupsTab.Instance.tabControl.IsEnabled = false;
        }

        public void Close()
        {
            _setupName = String.Empty;
            _currentSetup = null;
            this.Visibility = Visibility.Hidden;
            this.listViewTracks.Items.Clear();
            SetupsTab.Instance.tabControl.IsEnabled = true;
        }
    }
}
