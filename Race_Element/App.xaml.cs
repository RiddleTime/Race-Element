using ACCManager.Data.ACC.Core.Game;
using RaceElement.HUD.ACC.Overlays.OverlayStartScreen;
using RaceElement.Util;
using RaceElement.Util.Settings;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Windows;

namespace RaceElement;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    internal static App Instance { get; private set; }
    internal bool StartMinimized { get; private set; } = false;

    internal StartScreenOverlay _startScreenOverlay;

    public App()
    {

        this.Startup += App_Startup;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        Instance = this;

        var uiSettings = new UiSettings().Get();
        FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(System.Environment.ProcessPath);
        _startScreenOverlay = new StartScreenOverlay(new System.Drawing.Rectangle(uiSettings.X, uiSettings.Y, 150, 150)) { Version = fileVersion.FileVersion };
        _startScreenOverlay.Start(false);

        AccScheduler.RegisterJobs();
    }

    private void App_Startup(object sender, StartupEventArgs e)
    {
        for (int i = 0; i != e.Args.Length; ++i)
        {
            if (e.Args[i] == "/StartMinimized")
                StartMinimized = true;
        }

        DirectoryInfo internalPath = new(FileUtil.RaceElementInternalPath);
        if (!internalPath.Exists) internalPath.Create();
        ProfileOptimization.SetProfileRoot(FileUtil.RaceElementInternalPath);
        ProfileOptimization.StartProfile("RaceElementProfile");

        using Process current = Process.GetCurrentProcess();
        current.PriorityClass = ProcessPriorityClass.BelowNormal;
    }

}

