using ACCManager.HUD.ACC;
using ACCManager.Util;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ACCManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

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
                LogWriter.WriteToLog("Rounded corners are not supported for this machine, using square corners for the main window.");
            }


            this.Title = $"ACC Manager {GetAssemblyFileVersion()}";

            this.titleBar.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;

            this.buttonPlayACC.Click += (sender, e) => System.Diagnostics.Process.Start("steam://rungameid/805550");

            this.StateChanged += MainWindow_StateChanged;


            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            this.Closing += MainWindow_Closing;

            Instance = this;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OverlaysACC.CloseAll();
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            OverlaysACC.CloseAll();
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
                        break;
                    }
                case WindowState.Maximized:
                    {
                        rowTitleBar.Height = new GridLength(35);
                        break;
                    }
            }
        }

        public void EnqueueSnackbarMessage(string message)
        {
            Instance.snackbar.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                   delegate ()
                   {
                       Instance.snackbar.MessageQueue.Enqueue(message);
                   }));
        }

        public void EnqueueSnackbarWarning(string message)
        {
            Instance.snackbar.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                   delegate ()
                   {
                       Instance.snackbar.MessageQueue.Enqueue(message);
                   }));
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
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
