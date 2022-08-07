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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
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

        public MainWindow()
        {
            InitializeComponent();

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


            this.Title = $"ACC Manager {GetAssemblyFileVersion()}";

            this.titleBar.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;
            this.titleBar.MouseLeftButtonUp += TitleBar_MouseLeftButtonUp;
            this.titleBar.MouseLeave += (s, e) => { _stopDecreaseOpacty = true; e.Handled = true; this.Opacity = 1; };
            this.titleBar.DragLeave += (s, e) => { _stopDecreaseOpacty = true; e.Handled = true; this.Opacity = 1; };
            this.titleBar.MouseDoubleClick += (s, e) => { _stopDecreaseOpacty = true; e.Handled = true; this.Opacity = 1; };

            this.buttonPlayACC.Click += (sender, e) => System.Diagnostics.Process.Start("steam://rungameid/805550");

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

            Instance = this;

            // TODO refactor
            ACCTrackerStarter.StartACCTrackers();
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
            string loadString = $"Loaded ACC Manager {GetAssemblyFileVersion()}";
#if DEBUG
            loadString += " - Debug";
#endif
            Trace.WriteLine(loadString);
            LogWriter.WriteToLog(loadString);

            if (!App.Instance.StartMinimized)
                this.WindowState = WindowState.Normal;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveLocation();

            OverlaysACC.CloseAll();
            HudTrackers.StopAll();
            ACCTrackerDispose.Dispose();
            HudOptions.Instance.DisposeKeyboardHooks();
            SteeringLockTracker.Instance.Dispose();
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            OverlaysACC.CloseAll();
            HudTrackers.StopAll();
            ACCTrackerDispose.Dispose();
            HudOptions.Instance.DisposeKeyboardHooks();
            SteeringLockTracker.Instance.Dispose();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Minimized:
                    {
                        break;
                    }
                case WindowState.Normal:
                    {
                        this.Activate();
                        rowTitleBar.Height = new GridLength(30);
                        _stopDecreaseOpacty = true;
                        break;
                    }
                case WindowState.Maximized:
                    {
                        rowTitleBar.Height = new GridLength(35);
                        _stopDecreaseOpacty = true;
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
