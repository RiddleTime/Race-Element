using RaceElement.Data.ACC.Config;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RaceElement.Controls;

/// <summary>
/// Interaction logic for AccLiverySettings.xaml
/// </summary>
public partial class AccLiverySettings : UserControl
{
    private readonly MenuSettingsService menu = new();

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
