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
using System.Windows.Controls.Primitives;
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
            buttonResetLiverySettings.Click += (s, e) =>
            {
                menu.ResetLiverySettings();
                LoadSettings();
            };

            AddToggleListener(toggleTexDDS);
            AddToggleListener(toggleTexCap);
        }

        private void AddToggleListener(ToggleButton button)
        {
            button.Checked += (s, e) => SaveSettings();
            button.Unchecked += (s, e) => SaveSettings();
        }

        private void LoadSettings()
        {
            var settings = menu.Settings().Get(false);

            toggleTexDDS.IsChecked = settings.TexCap == 1;
            toggleTexCap.IsChecked = settings.TexDDS == 0;
        }

        private void SaveSettings()
        {
            var settings = menu.Settings().Get(false);

            settings.TexCap = toggleTexDDS.IsChecked.Value ? 1 : 0;
            settings.TexDDS = toggleTexCap.IsChecked.Value ? 0 : 1;

            menu.Settings().Save(settings);
        }
    }
}
