using System.Diagnostics;
using System.Windows.Controls;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for SetupAffiliate.xaml
    /// </summary>
    public partial class SetupAffiliate : UserControl
    {
        public SetupAffiliate()
        {
            InitializeComponent();
            buttonLukeAddison.Click += (sender, e) => Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = $"/c start https://lukeaddison-racing.com?ref=319",
                WindowStyle = ProcessWindowStyle.Hidden,
            });
        }
    }
}
