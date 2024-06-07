using ACCManager.Data.ACC.Core.Game;
using RaceElement.Controls;
using RaceElement.Controls.Util;
using RaceElement.HUD.ACC.Overlays.OverlayStartScreen;
using RaceElement.Util;
using RaceElement.Util.Settings;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace RaceElement;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    internal static App Instance { get; private set; }
    internal bool StartMinimized { get; private set; } = false;

    internal StartScreenOverlay _startScreenOverlay;
    private readonly Mutex mutex = new(true, "8f81b9c5-284a-4458-98be-2387c9046562");

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

    private void RegisterNamedPipe(bool isnotherInstanceRunning, string[] args)
    {
        if (isnotherInstanceRunning)
        {
            Thread writerThread = new(x =>
            {
                NamedPipeClientStream client = new("8f81b9c5-284a-4458-98be-2387c9046562");
                try
                {
                    client.Connect(0);
                }
                catch (Exception)
                {
                    return;
                }
                using var writer = new BinaryWriter(client);
                if (args.Length > 0)
                {
                    writer.Write(args[0]);
                    Thread.Sleep(200);
                    if (args[0].ToLower().StartsWith("raceelement://"))
                    {
                        writer.Close();
                        _startScreenOverlay.Hide();
                        _startScreenOverlay.Stop();
                        _startScreenOverlay.Dispose();

                        App.Current.Shutdown();
                        Environment.Exit(0);
                        return;
                    }
                }
                else
                {

                    // might want to provide an initial message to already running race element client. for now nothing
                    //Debug.WriteLine("Saying hi");
                    //writer.Write("Just saying hi");
                }

                writer.Close();
            });
            writerThread.Start();
        }

        if (!isnotherInstanceRunning)
        {
            Thread readerThread = new(new ThreadStart(ReaderThread));
            readerThread.Start();
        }
    }
    private void ReaderThread()
    {
        try
        {
            while (true)
            {
                NamedPipeServerStream server = new("8f81b9c5-284a-4458-98be-2387c9046562", PipeDirection.InOut, 1, PipeTransmissionMode.Byte);
                server.WaitForConnection();
                using BinaryReader reader = new(server);
                string args = reader.ReadString();

                if (args.ToLower().StartsWith("raceelement://"))
                    HandleCustomUriScheme(args);

                Debug.WriteLine($"Received: {args}");
                Thread.Sleep(1000);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            // here's your explanation
        }
    }

    private bool IsAnotherInstanceRunning()
    {
        bool isAlreadyRunning = false;
        try
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
                mutex.ReleaseMutex();
            else
                isAlreadyRunning = true;
        }
        catch (AbandonedMutexException)
        {

        }

        return isAlreadyRunning;
    }


    private void App_Startup(object sender, StartupEventArgs e)
    {
        bool isAnotherInstanceRunning = IsAnotherInstanceRunning();
        Debug.WriteLine("is other running " + isAnotherInstanceRunning);
        RegisterNamedPipe(isAnotherInstanceRunning, e.Args);

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
                    Debug.WriteLine($"Received setup as custom uri parameter.");
                    Debug.WriteLine($"{command}");
                    Debug.WriteLine($"Trying to import setup through URI.");
                    arg = arg.Replace("setup=", "");
                    arg = arg.Replace("Setup=", "");
                    new Thread(x =>
                    {
                        while (SetupImporter.Instance == null)
                            Thread.Sleep(100);

                        SetupImporter.Instance.ImportFromUri(arg);
                        Dispatcher.BeginInvoke(() =>
                        {
                            RaceElement.MainWindow.Instance.Topmost = true;
                            RaceElement.MainWindow.Instance.Activate();
                            RaceElement.MainWindow.Instance.WindowState = WindowState.Minimized;
                            RaceElement.MainWindow.Instance.Show();
                            RaceElement.MainWindow.Instance.WindowState = WindowState.Normal;

                            Thread.Sleep(30);
                            RaceElement.MainWindow.Instance.tabSetups.Focus();
                            RaceElement.MainWindow.Instance.Topmost = false;
                        });
                    }).Start();


                    break;
                }

        }
    }

    private void App_Exit(object sender, ExitEventArgs e)
    {
        AccScheduler.UnregisterJobs();
        Environment.Exit(0);
    }
}
