using Newtonsoft.Json;
using RaceElement.Data.ACC.Config;
using RaceElement.Util.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for AccLiverySettings.xaml
    /// </summary>
    public partial class AccLiverySettings : UserControl
    {
        private readonly MenuSettingsService menu = new MenuSettingsService();

        public AccLiverySettings()
        {
            InitializeComponent();
            this.Loaded += (s, e) => LoadSettings();
            buttonResetLiverySettings.Click += (s, e) => menu.ResetLiverySettings();
        }

        private void LoadSettings()
        {
            var settings = menu.Settings().Get(false);
            if (settings != null)
                Debug.WriteLine(JsonConvert.SerializeObject(settings, Formatting.Indented));

            int texCap = settings.TexCap;
            int texDDS = settings.TexDDS;
        }

        private void SaveSettings()
        {

        }
    }
}
