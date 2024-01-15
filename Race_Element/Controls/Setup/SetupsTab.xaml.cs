using MaterialDesignThemes.Wpf;
using RaceElement.Controls.Util;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
			SetupBrowser.Instance.FetchAllSetups();

			
			e.Handled = true;
		};

		Instance = this;
	}

}
