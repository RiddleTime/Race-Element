using ACCManager.Data.ACC.Core.Game;
using RaceElement.Controls;
using RaceElement.Controls.Util;
using RaceElement.HUD.ACC.Overlays.OverlayStartScreen;
using RaceElement.Util;
using RaceElement.Util.Settings;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        this.Startup += App_Startup;
        this.Exit += App_Exit;

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
        StringBuilder sb = new();
        foreach (var arg in e.Args) sb.Append(arg);
        LogWriter.WriteToLog(sb.ToString());

        for (int i = 0; i != e.Args.Length; ++i)
        {
            if (e.Args[i] == "/StartMinimized")
                StartMinimized = true;

            if (e.Args[i].StartsWith("raceelement://"))
                HandleCustomUriScheme(e.Args[i]);
        }

        DirectoryInfo internalPath = new(FileUtil.RaceElementInternalPath);
        if (!internalPath.Exists) internalPath.Create();
        ProfileOptimization.SetProfileRoot(FileUtil.RaceElementInternalPath);
        ProfileOptimization.StartProfile("RaceElementProfile");

        using Process current = Process.GetCurrentProcess();
        current.PriorityClass = ProcessPriorityClass.BelowNormal;
        RegisterProtocol();
    }

    private void RegisterProtocol()
    {
        CustomUriSchemeHelper.Initialize();
    }

    private void HandleCustomUriScheme(string arg)
    {
        if (string.IsNullOrEmpty(arg)) return;

        arg = arg.Replace(@"raceelement://", "");
        string command = arg.Split("=")[0];
        switch (command.ToLower())
        {
            case "setup":
                {
                    LogWriter.WriteToLog($"Received setup as custom uri parameter.");
                    LogWriter.WriteToLog($"{command}");
                    LogWriter.WriteToLog($"Trying to import setup through URI.");
                    arg = arg.Replace("setup=", "");
                    arg = arg.Replace("Setup=", "");
                    new Thread(x =>
                    {
                        while (SetupImporter.Instance == null)
                            Thread.Sleep(100);

                        SetupImporter.Instance.ImportFromUri(arg);
                        Dispatcher.BeginInvoke(() => { RaceElement.MainWindow.Instance.tabSetups.Focus(); });
                    }).Start();


                    break;
                }

        }
    }

    private void App_Exit(object sender, ExitEventArgs e)
    {
        AccScheduler.UnregisterJobs();
    }
}
