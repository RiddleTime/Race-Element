using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using RaceElement.Data;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static RaceElement.Controls.LiveryBrowser;
using Quartz.Util;
using RaceElement.Util;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using Newtonsoft.Json.Serialization;
using RaceElement.LiveryParser;
using System.Threading;

namespace RaceElement.Controls.Liveries
{
    /// <summary>
    /// Interaction logic for LiveryCreator.xaml
    /// </summary>
    public partial class LiveryCreator : UserControl
    {
        private static LiveryCreator Instance { get; set; }

        public LiveryCreator()
        {
            InitializeComponent();
            buttonClose.Click += (s, e) =>
            {
                Instance.Visibility = Visibility.Collapsed;
                ClearInput();
            };
            buttonCreate.Click += (s, e) => Create();
            this.Loaded += (s, e) =>
            {
                SetupData();
                LiveryCreator.Instance = this;
            };

            textBoxCarNumber.PreviewTextInput += (s, e) => { };
        }

        public static void Show()
        {
            if (Instance == null)
                return;

            Instance.Visibility = Visibility.Visible;
        }

        private void SetupData()
        {
            if (comboCarModel.Items.Count == 0)
                foreach (var item in ConversionFactory.CarModelToCarName)
                {
                    if (item.Key == ConversionFactory.CarModels.None)
                        continue;

                    ComboBoxItem comboBoxItem = new ComboBoxItem
                    {
                        DataContext = item.Key,
                        Content = item.Value
                    };
                    comboCarModel.Items.Add(comboBoxItem);
                }
            comboCarModel.SelectedIndex = 0;

            if (comboNationality.Items.Count == 0)
                foreach (var item in LiveryDisplayer.Nationalities)
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem
                    {
                        DataContext = item.Key,
                        Content = item.Value
                    };
                    comboNationality.Items.Add(comboBoxItem);
                }
            comboNationality.SelectedIndex = 0;
        }

        private void Create()
        {
            if (!ValidateInput()) return;

            ComboBoxItem boxItemCarModel = (ComboBoxItem)comboCarModel.SelectedItem;
            ConversionFactory.CarModels selectedModel = (ConversionFactory.CarModels)boxItemCarModel.DataContext;

            ComboBoxItem boxItemNationality = (ComboBoxItem)comboNationality.SelectedItem;
            int selectedNationality = (int)boxItemNationality.DataContext;

            LiveryTreeCar liveryTreeCar = new LiveryTreeCar()
            {
                CarsFile = new FileInfo($"{RaceElement.Util.FileUtil.CarsPath}{Guid.NewGuid()}.json"),
                CarsRoot = new CarsJson.Root()
                {
                    CarModelType = ConversionFactory.IdsToCarModel.FirstOrDefault(x => x.Value == selectedModel).Key,
                    CustomSkinName = textBoxLiveryName.Text,

                    // TODO
                    RaceNumber = int.Parse(textBoxCarNumber.Text),
                    CompetitorName = textBoxDriverName.Text,
                    TeamName = textBoxTeamName.Text,
                    DisplayName = textBoxDisplayName.Text,

                    CompetitorNationality = selectedNationality,
                    Nationality = selectedNationality,

                    //   stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Body Base Layer: {GetBodyMaterialType(carsRoot.SkinMaterialType1)}"));
                    //   stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Body Accent: {GetBodyMaterialType(carsRoot.SkinMaterialType2)}"));
                    //   stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Body Trim: {GetBodyMaterialType(carsRoot.SkinMaterialType3)}"));
                    //   stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Rim Base: {GetRimMaterialType(carsRoot.RimMaterialType1)}"));
                    //   stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Rim Accent: {GetRimMaterialType(carsRoot.RimMaterialType2)}"));
                    SkinMaterialType1 = 0,
                    SkinMaterialType2 = 0,
                    SkinMaterialType3 = 0,
                    RimMaterialType1 = 0,
                    RimMaterialType2 = 0,
                }
            };

            Debug.WriteLine(JsonConvert.SerializeObject(liveryTreeCar, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() }));

            DirectoryInfo carFolder = new DirectoryInfo($"{RaceElement.Util.FileUtil.LiveriesPath}{liveryTreeCar.CarsRoot.CustomSkinName}");
            if (carFolder.Exists)
            {
                MainWindow.Instance.EnqueueSnackbarMessage($"Custom livery name already exists.");
                return;
            }

            // create cars file
            var jsonSettings = new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            string carsFileJsonString = JsonConvert.SerializeObject(liveryTreeCar.CarsRoot, Formatting.Indented, jsonSettings);
            File.WriteAllText(liveryTreeCar.CarsFile.FullName, carsFileJsonString);

            // create cars folder
            carFolder.Create();

            // create both default sponsors and decals files in cars folder
            string[] sponsorAndDecals = new string[] { "sponsors.json", "decals.json" };
            foreach (string fileName in sponsorAndDecals)
            {
                FileInfo sponsorsJson = new FileInfo($"{RaceElement.Util.FileUtil.LiveriesPath}{liveryTreeCar.CarsRoot.CustomSkinName}\\{fileName}");
                PaintDetailsJson.Root paintDetailsJson = new PaintDetailsJson.Root();
                string json = JsonConvert.SerializeObject(paintDetailsJson, Formatting.Indented, jsonSettings);
                File.WriteAllText(sponsorsJson.FullName, json);
            }

            this.Visibility = Visibility.Hidden;
            ClearInput();
            ThreadPool.QueueUserWorkItem(x => LiveryBrowser.Instance.FetchAllCars());
        }

        private void ClearInput()
        {
            textBoxCarNumber.Text = "1";
            textBoxDisplayName.Text = string.Empty;
            textBoxDriverName.Text = string.Empty;
            textBoxLiveryName.Text = string.Empty;
            textBoxTeamName.Text = string.Empty;
            comboCarModel.SelectedIndex = 0;
            comboNationality.SelectedIndex = 0;
        }

        private bool ValidateInput()
        {
            if (!ValidateCustomLiveryName())
                return false;

            if (!ValidateCarNumber())
                return false;

            if (!ValidateTeamName())
                return false;

            return true;
        }

        private bool ValidateCustomLiveryName()
        {
            string input = textBoxLiveryName.Text;

            if (input == string.Empty)
            {
                MainWindow.Instance.EnqueueSnackbarMessage("Custom Livery Name cannot be empty.");
                return false;
            }

            textBoxLiveryName.Text = textBoxLiveryName.Text.Trim();


            int illegalCharIndex = input.IndexOfAny(System.IO.Path.GetInvalidFileNameChars());
            if (illegalCharIndex >= 0)
            {
                MainWindow.Instance.EnqueueSnackbarMessage($"Custom Livery Name: {input[illegalCharIndex]} is not an allowed character.");
                return false;
            }

            return true;
        }

        private bool ValidateTeamName()
        {
            if (textBoxTeamName.Text == string.Empty)
            {
                MainWindow.Instance.EnqueueSnackbarMessage($"Team Name cannot be empty.");
                return false;
            }

            return true;
        }

        private bool ValidateCarNumber()
        {
            string input = textBoxCarNumber.Text;

            if (input == string.Empty)
            {
                MainWindow.Instance.EnqueueSnackbarMessage("Car Number cannot be empty.");
                return false;
            }

            if (!int.TryParse(input, out int _))
            {
                MainWindow.Instance.EnqueueSnackbarMessage($"Car Number can only be a number.");
                return false;
            }

            return true;
        }
    }
}
