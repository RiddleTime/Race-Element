using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Controls;
using ACCManager.Controls.Setup;
using ACCManager.Util;
using System.Diagnostics;

namespace ACCManager.Controls
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
