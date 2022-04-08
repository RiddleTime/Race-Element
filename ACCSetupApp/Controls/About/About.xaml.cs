using ACCSetupApp.SetupParser;
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

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        // https://qwerty.dev/whitespace/
        private string FourEmSpace = " ";

        public About()
        {
            InitializeComponent();

            SetCarConversionFeatures();
        }

        private void SetCarConversionFeatures()
        {
            List<string> gt3Names = new ConversionFactory().GetAllGT3Names();
            textBlockSetupViewerGT3.Text = $"{FourEmSpace}GT3:\n";
            for (int i = 0; i < gt3Names.Count; i++)
            {
                textBlockSetupViewerGT3.Text += $"{FourEmSpace}- {gt3Names[i]}";
                if (i < gt3Names.Count - 1)
                {
                    textBlockSetupViewerGT3.Text += "\n";
                }
            }
        }
    }
}
