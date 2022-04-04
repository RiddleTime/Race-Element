using ACCSetupApp.LiveryParser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for LiveryBrowser.xaml
    /// </summary>
    public partial class LiveryBrowser : UserControl
    {
        private string AccPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Assetto Corsa Competizione\\";
        private string CustomsPath => AccPath + "Customs\\";
        private string CarsPath => CustomsPath + "Cars\\";

        public LiveryBrowser()
        {
            InitializeComponent();

            FetchAllCars();
        }

        private void FetchAllCars()
        {
            liveriesTreeView.Items.Clear();

            DirectoryInfo customsCarsDirectory = new DirectoryInfo(CarsPath);

            foreach (var carsFile in customsCarsDirectory.GetFiles())
            {
                if (carsFile.Extension.Equals(".json"))
                {

                    CarsJson.Root carsRoot = GetLivery(carsFile);

                    TextBlock liveryHeader = new TextBlock()
                    {
                        Text = $"{carsRoot.teamName}",
                        Style = Resources["MaterialDesignSubtitle1TextBlock"] as Style,
                    };
                    TreeViewItem teamItem = new TreeViewItem() { Header = liveryHeader, DataContext = carsRoot };

                    liveriesTreeView.Items.Add(teamItem);

                }
            }
        }

        public CarsJson.Root GetLivery(FileInfo file)
        {
            if (!file.Exists)
                return null;

            string jsonString = string.Empty;
            try
            {
                using (FileStream fileStream = file.OpenRead())
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        jsonString = reader.ReadToEnd();
                        jsonString = jsonString.Replace("\0", "");
                        reader.Close();
                        fileStream.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            CarsJson.Root carLiveryRoot = JsonConvert.DeserializeObject<CarsJson.Root>(jsonString);
            return carLiveryRoot;
        }
    }
}
