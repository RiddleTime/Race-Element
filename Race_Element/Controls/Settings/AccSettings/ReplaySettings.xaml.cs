using RaceElement.Util.Settings;
using System.Windows.Controls;

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
