using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {
        private static TitleBar _instance;
        internal static TitleBar Instance { get { return _instance; } }

        private const string _AppName = "ACC Manager";

        public TitleBar()
        {
            InitializeComponent();
            SetAppTitle();
            buttonExit.Click += ButtonExit_Click;

            buttonMinimize.Click += (e, s) => { App.Current.MainWindow.WindowState = WindowState.Minimized; };
            buttonMaximize.Click += (e, s) =>
            {
                if (App.Current.MainWindow.WindowState == WindowState.Maximized)
                    App.Current.MainWindow.WindowState = WindowState.Normal;
                else
                    App.Current.MainWindow.WindowState = WindowState.Maximized;
            };

            this.MouseDoubleClick += TitleBar_MouseDoubleClick;
            _instance = this;
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

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.SaveLocation();
            Environment.Exit(0);
        }

        public static string GetAssemblyFileVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersion.FileVersion;
        }
    }
}
