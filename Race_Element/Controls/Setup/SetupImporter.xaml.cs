using RaceElement.Controls.Setup;
using RaceElement.Data;
using RaceElement.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using static RaceElement.Data.ACC.Tracks.TrackData;
using static RaceElement.Data.ConversionFactory;

namespace RaceElement.Controls;

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
        buttonImportMulti.Click += (s, e) =>
        {
            MainWindow.Instance.EnqueueSnackbarMessage($"Importing setups, please wait...");
            if (listViewTracks.SelectedItems.Count > 0 && listViewTracks.SelectionMode == SelectionMode.Multiple)
            {
                StringBuilder sb = new();

                foreach (ListViewItem selectedItem in listViewTracks.SelectedItems)
                {
                    Import((string)selectedItem.DataContext, false);
                    var trackData = Tracks.FirstOrDefault(x => x.GameName == (string)selectedItem.DataContext);
                    sb.Append($", {trackData.FullName}");
                }

                CarModels model = ConversionFactory.ParseCarName(_currentSetup.CarName);
                string modelName = ConversionFactory.GetNameFromCarModel(model);
                MainWindow.Instance.EnqueueSnackbarMessage($"Imported setup \"{_setupName}\" for {modelName} at{sb}");

                SetupBrowser.Instance.RefreshTree();

                Close();
            }
        };

        _renderer = new FlowDocSetupRenderer();

        Instance = this;
    }

    private void BuildTrackList()
    {
        this.listViewTracks.Items.Clear();
        foreach (AbstractTrackData trackData in Tracks)
        {
            ListViewItem trackItem = new()
            {
                FontWeight = FontWeights.Bold,
                Content = trackData.FullName,
                DataContext = trackData.GameName
            };

            if (listViewTracks.SelectionMode == SelectionMode.Single)
                trackItem.MouseLeftButtonUp += (s, e) =>
                {
                    Import(trackData.GameName, true);

                    SetupBrowser.Instance.RefreshTree();

                    Close();
                };

            this.listViewTracks.Items.Add(trackItem);
        }
    }


    private void Import(string track, bool displayMessage)
    {
        try
        {
            CarModels model = ConversionFactory.ParseCarName(_currentSetup.CarName);
            string modelName = ConversionFactory.GetNameFromCarModel(model);


            FileInfo targetFile = new(FileUtil.AccPath + "Setups\\" + _currentSetup.CarName + "\\" + track + "\\" + _setupName + ".json");

            if (targetFile.Exists)
            {
                MainWindow.Instance.EnqueueSnackbarMessage($"Setup already exists: {targetFile.FullName}");
                return;
            }

            if (!targetFile.Directory.Exists)
                targetFile.Directory.Create();

            FileInfo originalFile = new(_originalSetupFile);
            if (originalFile.Exists)
                originalFile.CopyTo(targetFile.FullName);

            if (displayMessage)
            {
                AbstractTrackData trackData = Tracks.FirstOrDefault(x => x.GameName == track);
                MainWindow.Instance.EnqueueSnackbarMessage($"Imported setup \"{_setupName}\" for {modelName} at {trackData.FullName}");
            }
        }
        catch (Exception e)
        {
            LogWriter.WriteToLog(e);
        }
    }

    public bool Open(string setupFile, bool selectMultipleTracks, bool showTrack = false)
    {
        listViewTracks.SelectionMode = selectMultipleTracks ? SelectionMode.Multiple : SelectionMode.Single;
        buttonImportMulti.Visibility = selectMultipleTracks ? Visibility.Visible : Visibility.Collapsed;

        FileInfo file = null;

        if (setupFile.StartsWith("https"))
        {
            try
            {
                using (var client = new WebClient())
                {
                    string fileName;
                    string[] splits = setupFile.Split(['/']);
                    fileName = splits[splits.Length - 1];

                    DirectoryInfo downloadCache = new(FileUtil.RaceElementDownloadCachePath);

                    if (!downloadCache.Exists) downloadCache.Create();

                    string fullName = FileUtil.RaceElementDownloadCachePath + fileName;
                    client.DownloadFile(setupFile, fullName);
                    setupFile = fullName;
                    file = new FileInfo(fullName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        else
        {
            file = new FileInfo(setupFile);
        }

        if (file == null || !file.Exists)
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

        try
        {
            BuildTrackList();
            this.textBlockSetupName.Text = $"{modelName} - {file.Name}";

            _renderer.LogSetup(ref this.flowDocument, setupFile, showTrack);

            this.Visibility = Visibility.Visible;
            SetupsTab.Instance.tabControl.IsEnabled = false;
        }
        catch (Exception ex) { Debug.WriteLine(ex); }

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
