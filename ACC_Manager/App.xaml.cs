using System.Globalization;
using System.Windows;

namespace ACCManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App Instance { get; private set; }
        public bool StartMinimized = false;

        public App()
        {
            this.Startup += App_Startup;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
           Instance = this;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "/StartMinimized")
                    StartMinimized = true;
            }
        }

    }
}
