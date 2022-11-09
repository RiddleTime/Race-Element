using ACC_Manager.Broadcast;
using ACC_Manager.Util.Settings;
using ACC_Manager.Util.SystemExtensions;
using ACCManager.Controls;
using ACCManager.Data.ACC.Tracker;
using ACCManager.Hardware.ACC.SteeringLock;
using ACCManager.HUD.ACC;
using ACCManager.HUD.ACC.Data.Tracker;
using ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayDebugOutput;
using ACCManager.Util;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ACCManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static MainWindow Instance { get; private set; }
        private readonly UiSettings _uiSettings;
        private readonly AccManagerSettings _accManagerSettings;

        public MainWindow()
        {
            DateTime startTime = DateTime.Now;
            InitializeComponent();
            Debug.WriteLine($"Startup time(ms): {DateTime.Now.Subtract(startTime).TotalMilliseconds}");

            try
            {
                IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
                var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
                var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
            }
            catch (Exception)
            {
                // LogWriter.WriteToLog("Rounded corners are not supported for this machine, using square corners for the main window.");
            }

            _uiSettings = new UiSettings();
            _accManagerSettings = new AccManagerSettings();


            this.Title = $"ACC Manager {GetAssemblyFileVersion()}";

            this.titleBar.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;
            this.titleBar.MouseLeftButtonUp += TitleBar_MouseLeftButtonUp;
            this.titleBar.MouseLeave += (s, e) => { _stopDecreaseOpacty = true; e.Handled = true; this.Opacity = 1; };
            this.titleBar.DragLeave += (s, e) => { _stopDecreaseOpacty = true; e.Handled = true; this.Opacity = 1; };
            this.titleBar.MouseDoubleClick += (s, e) => { _stopDecreaseOpacty = true; e.Handled = true; this.Opacity = 1; };

            this.buttonPlayACC.Click += (sender, e) => Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = $"/c start steam://rungameid/805550",
                WindowStyle = ProcessWindowStyle.Hidden,
            });

            this.StateChanged += MainWindow_StateChanged;


            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            this.Closing += MainWindow_Closing;

            _ = TraceOutputListener.Instance;

            this.Loaded += MainWindow_Loaded;

            tabControl.SelectionChanged += (s, se) =>
            {
                UiSettingsJson tempSettings = _uiSettings.Get();
                tempSettings.SelectedTabIndex = tabControl.SelectedIndex;
                _uiSettings.Save(tempSettings);
            };

            tabControl.SelectedIndex = _uiSettings.Get().SelectedTabIndex.Clip(0, tabControl.Items.Count - 1);

            UiSettingsJson uiSettings = _uiSettings.Get();
            this.Left = uiSettings.X.Clip(0, (int)SystemParameters.PrimaryScreenWidth);
            this.Top = uiSettings.Y.Clip(0, (int)SystemParameters.PrimaryScreenHeight);

            _uiSettings.Save(uiSettings);


            this.Drop += MainWindow_Drop;

            InitializeSystemTrayIcon();

            Instance = this;

            // --- TODO refactor
            ACCTrackerStarter.StartACCTrackers();

            // warm up the broad cast config json.. make sure it's set up before we join any session
            _ = BroadcastConfig.GetConfiguration();
            // ---
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data is DataObject)
            {
                DataObject data = (DataObject)e.Data;

                StringCollection droppedItems = data.GetFileDropList();
                if (droppedItems.Count == 1)
                {
                    string droppedItem = droppedItems[0];

                    if (droppedItem.EndsWith(".json"))
                    {
                        if (SetupImporter.Instance.Open(droppedItem))
                        {
                            tabSetups.Focus();
                            e.Handled = true;
                            return;
                        }
                    }

                    if (droppedItem.EndsWith(".rwdb"))
                    {
                        RaceSessionBrowser.Instance.OpenRaceWeekendDatabase(droppedItem);
                        tabTelemetry.Focus();
                        e.Handled = true;
                        return;
                    }
                }
            }

        }

        public void SaveLocation()
        {
            UiSettingsJson uiSettings = _uiSettings.Get();
            uiSettings.X = (int)this.Left;
            uiSettings.Y = (int)this.Top;
            _uiSettings.Save(uiSettings);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(x =>
            {
                Thread.Sleep(2000);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                string loadString = $"Loaded ACC Manager {GetAssemblyFileVersion()}";
                string fileHash = FileUtil.GetBase64Hash(FileUtil.AppFullName);

#if DEBUG
                loadString += " - Debug";
#endif
                Trace.WriteLine(loadString);
                Trace.WriteLine($"Application Hash: {fileHash}");
                LogWriter.WriteToLog($"Application Hash: {fileHash}");
                LogWriter.WriteToLog(loadString);

                UpdateUsage();
            });

            if (!App.Instance.StartMinimized)
                this.WindowState = WindowState.Normal;
        }

        private void UpdateUsage()
        {
            try
            {
                string hitCounter = "https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2FRiddleTime%2FACC-Manager";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(hitCounter);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (Exception) { }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveLocation();

            OverlaysACC.CloseAll();
            HudTrackers.StopAll();
            ACCTrackerDispose.Dispose();
            HudOptions.Instance.DisposeKeyboardHooks();
            SteeringLockTracker.Instance.Dispose();

            Environment.Exit(0);
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            OverlaysACC.CloseAll();
            HudTrackers.StopAll();
            ACCTrackerDispose.Dispose();
            HudOptions.Instance.DisposeKeyboardHooks();
            SteeringLockTracker.Instance.Dispose();

            Environment.Exit(0);
        }


        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private void InitializeSystemTrayIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon()
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Visible = false,
                ContextMenuStrip = CreateContextMenu(),
                Text = "ACC Manager"
            };

            _notifyIcon.DoubleClick += (s, e) => Instance.WindowState = WindowState.Normal;

        }
        private System.Windows.Forms.ContextMenuStrip CreateContextMenu()
        {
            var openItem = new System.Windows.Forms.ToolStripMenuItem("Open");
            openItem.Click += (s, e) => Instance.WindowState = WindowState.Normal;
            var exitItem = new System.Windows.Forms.ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => Environment.Exit(0);
            var contextMenu = new System.Windows.Forms.ContextMenuStrip { Items = { openItem, exitItem } };
            return contextMenu;
        }


        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Minimized:
                    {
                        if (_accManagerSettings.Get().MinimizeToSystemTray)
                        {
                            _notifyIcon.Visible = true;
                            ShowInTaskbar = false;
                        }

                        break;
                    }
                case WindowState.Normal:
                    {
                        this.Activate();
                        mainGrid.Margin = new Thickness(0);
                        //rowTitleBar.Height = new GridLength(30);
                        _stopDecreaseOpacty = true;
                        ShowInTaskbar = true;

                        _notifyIcon.Visible = false;
                        break;
                    }
                case WindowState.Maximized:
                    {
                        mainGrid.Margin = new Thickness(8);
                        //rowTitleBar.Height = new GridLength(35);
                        _stopDecreaseOpacty = true;
                        ShowInTaskbar = true;

                        _notifyIcon.Visible = false;
                        break;
                    }
            }
        }

        internal void EnqueueSnackbarMessage(string message)
        {
            Instance.snackbar.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
               delegate ()
               {
                   if (Instance.WindowState != WindowState.Minimized)
                       Instance.snackbar.MessageQueue.Enqueue(message);
               }));
        }

        internal void ClearSnackbar()
        {
            Instance.snackbar.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
               delegate ()
               {
                   if (Instance.WindowState != WindowState.Minimized)
                       Instance.snackbar.MessageQueue.Clear();
               }));
        }

        internal void EnqueueSnackbarMessage(string message, string action, Action actionDelegate)
        {
            Instance.snackbar.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
               delegate ()
               {
                   if (Instance.WindowState != WindowState.Minimized)
                       Instance.snackbar.MessageQueue.Enqueue(message, action, actionDelegate);
               }));
        }


        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DecreaseOpacity(0.925, 0.0025);
            DragMove();
            e.Handled = true;
        }

        private void TitleBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _stopDecreaseOpacty = true;
            this.Opacity = 1.0;
            e.Handled = true;
        }

        private bool _stopDecreaseOpacty;
        private void DecreaseOpacity(double target, double steps)
        {
            _stopDecreaseOpacty = false;

            new Thread(() =>
            {
                bool finalValueReached = false;

                while (!finalValueReached)
                {
                    if (_stopDecreaseOpacty)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            this.Opacity = 1;
                        }));
                        break;
                    }

                    Thread.Sleep(3);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        this.Opacity -= steps;

                        if (this.Opacity < target)
                            finalValueReached = true;
                    }));

                }
            }).Start();
        }

        public static string GetAssemblyFileVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersion.FileVersion;
        }

        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }

        // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
        // what value of the enum to set.
        // Copied from dwmapi.h
        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        // Import dwmapi.dll and define DwmSetWindowAttribute in C# corresponding to the native function.
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void DwmSetWindowAttribute(IntPtr hwnd,
                                                         DWMWINDOWATTRIBUTE attribute,
                                                         ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                         uint cbAttribute);
    }
}
