using System.Windows.Controls;

namespace RaceElement.Controls;

/// <summary>
/// Interaction logic for SetupsTab.xaml
/// </summary>
public partial class SetupsTab : UserControl
{
    public static SetupsTab Instance { get; private set; }

    public SetupsTab()
    {
        InitializeComponent();

        headerSetupTree.MouseRightButtonUp += (s, e) =>
        {
            SetupBrowser.Instance.RefreshTree();


            e.Handled = true;
        };

        Instance = this;
    }

}
