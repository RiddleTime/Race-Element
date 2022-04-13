using Newtonsoft.Json;
using ACCSetupApp.SetupParser.Cars.GT3;
using System;
using System.Windows;
using System.Windows.Controls;
using static SetupParser.SetupJson;
using static ACCSetupApp.SetupParser.SetupConverter;
using ACCSetupApp.SetupParser;
using ACCSetupApp.Controls.Setup;
using ACCSetupApp.Util;
using System.Diagnostics;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for SetupRenderer.xaml
    /// 
    /// </summary>
    public partial class SetupRenderer : UserControl
    {
        public SetupRenderer()
        {
            InitializeComponent();

            openFile.Click += OpenFile_Click;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension and default file extension 
                dlg.DefaultExt = ".json";
                dlg.Filter = "ACC Setup files|*.json";
                Nullable<bool> result = dlg.ShowDialog();


                // Get the selected file name and display in a TextBox 
                if (result == true)
                {
                    flowDocument.Blocks.Clear();

                    // Open document 
                    string filename = dlg.FileName;
                    new FlowDocSetupRenderer().LogSetup(ref flowDocument, filename);
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
                Debug.WriteLine(ex);
            }
        }


    }
}
