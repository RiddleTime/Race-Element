using RaceElement.Util.Settings;
using System;
using System.Collections.Generic;
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

namespace RaceElement.Controls;

/// <summary>
/// Interaction logic for ReplaySettings.xaml
/// </summary>
public partial class ReplaySettings : UserControl
{
    private AccSettingsJson _accSettings;

    public ReplaySettings()
    {
        InitializeComponent();

        this.Loaded += (s, e) => LoadSettings();

        this.toggleAutoSaveReplay.Checked += (s, e) => SaveSettings();
        this.toggleAutoSaveReplay.Unchecked += (s, e) => SaveSettings();
    }

    private void SaveSettings()
    {
        _accSettings.AutoRecordReplay = toggleAutoSaveReplay.IsChecked.Value;
        TitleBar.Instance.SetIcons(TitleBar.ActivatedIcons.AutomaticSaveReplay, _accSettings.AutoRecordReplay);

        new AccSettings().Save(_accSettings);
    }

    private void LoadSettings()
    {
        _accSettings = new AccSettings().Get();

        toggleAutoSaveReplay.IsChecked = _accSettings.AutoRecordReplay;
        TitleBar.Instance.SetIcons(TitleBar.ActivatedIcons.AutomaticSaveReplay, _accSettings.AutoRecordReplay);
    }
}
