using RaceElement.Data.ACC.Config;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RaceElement.Controls.AccHudSettingsNS;

/// <summary>
/// Interaction logic for AccHudSettings.xaml
/// </summary>
public partial class AccHudSettings : UserControl
{
    private readonly HudSettingsService hudSettings = new();


    public AccHudSettings()
    {
        InitializeComponent();
        this.Loaded += (s, e) => LoadSettings();

        AddToggleListener(toggleRatingWidget);
        AddToggleListener(toggleServerStats);
        AddToggleListener(toggleFps);
    }

    private void AddToggleListener(ToggleButton button)
    {
        button.Checked += (s, e) => SaveSettings();
        button.Unchecked += (s, e) => SaveSettings();
    }

    private void LoadSettings()
    {
        var json = hudSettings.Settings().Get(false);

        toggleRatingWidget.IsChecked = json.RatingWidgetVisible == 1;
        toggleServerStats.IsChecked = json.ServerStatsVisible == 1;
        toggleFps.IsChecked = json.FPSVisible == 1;
    }

    private void SaveSettings()
    {
        var json = hudSettings.Settings().Get(false);

        json.RatingWidgetVisible = toggleRatingWidget.IsChecked.Value ? 1 : 0;
        json.ServerStatsVisible = toggleServerStats.IsChecked.Value ? 1 : 0;
        json.FPSVisible = toggleFps.IsChecked.Value ? 1 : 0;

        hudSettings.Settings().Save(json);
    }
}
