using Octokit;
using RaceElement.Controls.Util.Updater;
using RaceElement.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {
        internal static TitleBar Instance { get; private set; }

        private const string _AppName = "Race Element";

        public TitleBar()
        {
            InitializeComponent();
            SetAppTitle();
            buttonExit.Click += ButtonExit_Click;

            buttonMinimize.Click += (s, e) => { App.Current.MainWindow.WindowState = WindowState.Minimized; };
            buttonMaximize.Click += (s, e) =>
            {
                if (App.Current.MainWindow.WindowState == WindowState.Maximized)
                    App.Current.MainWindow.WindowState = WindowState.Normal;
                else
                    App.Current.MainWindow.WindowState = WindowState.Maximized;

            };
            buttonHelp.Click += (s, e) =>
            {
                if (MainWindow.Instance.tabControl.SelectedIndex == MainWindow.Instance.tabControl.Items.Count - 1)
                    MainWindow.Instance.EnqueueSnackbarMessage("The Info tab is already open.");
                else
                    MainWindow.Instance.tabControl.SelectedIndex = MainWindow.Instance.tabControl.Items.Count - 1;
            };

            this.iconSteeringLock.MouseRightButtonDown += (s, e) =>
            {
                MainWindow.Instance.tabSettings.Focus();
                SettingsTab.Instance.tabHardware.Focus();
                e.Handled = true;
            };

            this.iconSetupHider.MouseRightButtonDown += (s, e) =>
            {
                MainWindow.Instance.tabSettings.Focus();
                SettingsTab.Instance.tabStreaming.Focus();
                e.Handled = true;
            };

            this.iconAutoSaveReplay.MouseRightButtonDown += (s, e) =>
            {

                MainWindow.Instance.tabSettings.Focus();
                SettingsTab.Instance.tabAccSettings.Focus();
            };



            //// used for collecting corner numbers in data.acc.tracks
            //#if DEBUG
            //            copySpline.Click += (s, e) =>
            //            {
            //                try
            //                {
            //                    float spline = ACCSharedMemory.Instance.ReadGraphicsPageFile().NormalizedCarPosition;
            //                    MainWindow.Instance.EnqueueSnackbarMessage($"Copied spline position {spline}f");

            //                    Clipboard.SetText($"{spline}f");
            //                }
            //                catch (Exception)
            //                {// 
            //                }
            //            };
            //#else
            //            copySpline.Visibility = Visibility.Collapsed;
            //#endif
            this.MouseDoubleClick += TitleBar_MouseDoubleClick;
            Instance = this;
        }

        internal void SetAppTitle(string launchType = "")
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Title.Text = $"{_AppName} {GetAssemblyFileVersion()}";
                if (launchType != String.Empty)
                    this.Title.Text += $" - {launchType}";
            }));
        }

        private void TitleBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WindowState targetState = WindowState.Normal;
            switch (App.Current.MainWindow.WindowState)
            {
                case WindowState.Normal:
                    {
                        targetState = WindowState.Maximized;
                        break;
                    }
                case WindowState.Maximized:
                    {
                        targetState = WindowState.Normal;
                        break;
                    }
            }

            App.Current.MainWindow.WindowState = targetState;
        }

        public void SetIcons(ActivatedIcons icon, bool enabled)
        {
            switch (icon)
            {
                case ActivatedIcons.AutomaticSteeringHardLock:
                    {
                        iconSteeringLock.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    }
                case ActivatedIcons.SetupHider:
                    {
                        iconSetupHider.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    }
                case ActivatedIcons.AutomaticSaveReplay:
                    {
                        iconAutoSaveReplay.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    }
            }
        }

        public enum ActivatedIcons
        {
            AutomaticSteeringHardLock,
            SetupHider,
            AutomaticSaveReplay
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.EnqueueSnackbarMessage("Shutting down Race Element");
            MainWindow.Instance.SaveLocation();
            Environment.Exit(0);
        }

        public static string GetAssemblyFileVersion()
        {
            try
            {

                System.Reflection.Assembly assembly = typeof(App).Assembly;
                LogWriter.WriteToLog("assembly name: " + assembly.FullName);
                LogWriter.WriteToLog("assembly loc: " + assembly.Location);
                FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, assembly.GetName() + ".exe"));
                return fileVersion.ProductVersion;
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
                return String.Empty;
            }
        }

        public void SetUpdateButton(string version, string releaseNotes, ReleaseAsset asset)
        {
            updateButton.Visibility = Visibility.Visible;
            updateButton.Content = $"Update to {version}";
            updateButton.ToolTip = releaseNotes;
            updateButton.Click += (s, e) =>
            {
                updateButton.IsEnabled = false;
                MainWindow.Instance.EnqueueSnackbarMessage($"Updating to version... {version}, this may take a while..");
                new Thread(x =>
                {
                    new AppUpdater().Update(asset);
                }).Start();
            };
        }
    }
}
