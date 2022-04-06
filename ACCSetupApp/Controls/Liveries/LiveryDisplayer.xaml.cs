using ACCSetupApp.LiveryParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading;
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
using static ACCSetupApp.Controls.LiveryBrowser;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for LiveryDisplayer.xaml
    /// </summary>
    public partial class LiveryDisplayer : UserControl
    {
        private string AccPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Assetto Corsa Competizione\\";
        private string CustomsPath => AccPath + "Customs\\";
        private string CarsPath => CustomsPath + "Cars\\";
        private string LiveriesPath => CustomsPath + "Liveries\\";

        internal static LiveryDisplayer Instance;

        private LiveryTreeCar Livery { get; set; }

        public LiveryDisplayer()
        {
            InitializeComponent();

            buttonOpenLiveryCarsJson.Click += ButtonOpenLiveryCarsJson_Click;
            buttonOpenLiveryDirectory.Click += ButtonOpenLiveryDirectory_Click;

            Instance = this;
        }

        private void ButtonOpenLiveryDirectory_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo directory = new DirectoryInfo($"{LiveriesPath}{Livery.carsRoot.customSkinName}");
            Process.Start(directory.FullName);
        }

        private void ButtonOpenLiveryCarsJson_Click(object sender, RoutedEventArgs e)
        {
            FileInfo carsJsonFile = new FileInfo($"{Livery.carsFile}");
            Process.Start($"{CarsPath}{carsJsonFile.Name}");
        }

        internal void SetLivery(LiveryTreeCar livery)
        {
            ThreadPool.QueueUserWorkItem(x => { GC.Collect(); });

            if (livery == null)
                return;

            Livery = livery;

            decalsLabel.Content = string.Empty;
            decalsImage.Source = null;
            sponsorsLabel.Content = string.Empty;
            sponsorsImage.Source = null;
            stackPanelLiveryInfo.Children.Clear();
            buttonsPanel.Visibility = Visibility.Hidden;

            ThreadPool.QueueUserWorkItem(x => { GC.Collect(); });


            ThreadPool.QueueUserWorkItem(x =>
            {
                Instance.Dispatcher.BeginInvoke(new Action(() =>
                {

                    CarsJson.Root carsRoot = livery.carsRoot;
                    string customSkinName = carsRoot.customSkinName;

                    if (customSkinName != null && customSkinName.Length > 0)
                    {
                        buttonsPanel.Visibility = Visibility.Visible;

                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"{carsRoot.teamName}", HorizontalAlignment.Center, 25));
                        if (customSkinName != String.Empty)
                            stackPanelLiveryInfo.Children.Add(GetInfoLabel($"{carsRoot.customSkinName}", HorizontalAlignment.Center, 18));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Display Name: {carsRoot.displayName}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Race Number: {carsRoot.raceNumber}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Nationality: {GetNationality(carsRoot.nationality)}"));

                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Rim Base: {GetRimMaterialType(carsRoot.rimMaterialType1)}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Rim Accent: {GetRimMaterialType(carsRoot.rimMaterialType2)}"));

                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Body Base Layer: {GetBodyMaterialType(carsRoot.skinMaterialType1)}"));

                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Body Accent: {GetBodyMaterialType(carsRoot.skinMaterialType2)}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Body Trim: {GetBodyMaterialType(carsRoot.skinMaterialType3)}"));

                        DirectoryInfo customSkinDir = new DirectoryInfo(LiveriesPath + customSkinName);
                        if (customSkinDir.Exists)
                        {
                            FileInfo[] sponporsFiles = customSkinDir.GetFiles("sponsors.png");
                            if (sponporsFiles != null && sponporsFiles.Length > 0)
                            {
                                FileInfo sponsorsFile = sponporsFiles[0];
                                sponsorsLabel.Content = "Sponsors";
                                sponsorsImage.Source = new BitmapImage(new Uri(sponsorsFile.FullName, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
                            }

                            FileInfo[] decalFiles = customSkinDir.GetFiles("decals.png");
                            if (decalFiles != null && decalFiles.Length > 0)
                            {
                                FileInfo decalsFile = decalFiles[0];
                                decalsLabel.Content = "Decals";
                                decalsImage.Source = new BitmapImage(new Uri(decalsFile.FullName, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
                            }
                        }
                    }
                }));
            });
        }

        private Label GetInfoLabel(string text, HorizontalAlignment allignmment = HorizontalAlignment.Left, int size = 13)
        {
            Label label = new Label()
            {
                Content = text,
                Style = Resources["MaterialDesignLabel"] as Style,
                FontSize = size,
                HorizontalAlignment = allignmment
            };
            return label;
        }

        private Dictionary<int, string> Nationalities = new Dictionary<int, string>()
            {
                {0,"Other" },
                {49,"Andorra" },
                {14,"Argentina" },
                {28,"Armenia" },
                {41,"Australia" },
                {9,"Austria" },
                {50,"Azerbaijan" },
                {79,"Bahrain" },
                {7,"Belgium" },
                {17,"Brazil" },
                {51,"Bulgaria" },
                {34,"Canada" },
                {82,"Chile" },
                {35,"China" },
                {81,"Chinese Taipei" },
                {47,"Colombia" },
                {33,"Croatia" },
                {52,"Cuba" },
                {53,"Czech Republic" },
                {32,"Denmark" },
                {86,"England" },
                {54,"Estonia" },
                {31,"Finland" },
                {3,"France" },
                {55,"Georgia" },
                {2,"Germany" },
                {5,"Great Britain" },
                {22,"Greece" },
                {46,"Hong Kong (S.A.R. of China)" },
                {6,"Hungary" },
                {56,"India" },
                {38,"Indonesia" },
                {78,"Iran" },
                {16,"Ireland" },
                {57,"Israel" },
                {1,"Italy" },
                {58,"Jamaica" },
                {48,"Japan" },
                {45,"Kuwait" },
                {59,"Latvia" },
                {27,"Lebanon" },
                {60,"Lithuania" },
                {44,"Luxembourg" },
                {61,"Macao (S.A.R. of China)" },
                {84,"Madagascar" },
                {62,"Malaysia" },
                {85,"Malta" },
                {29,"Mexico" },
                {15,"Monaco" },
                {63,"Nepal" },
                {12,"Netherlands" },
                {64,"New Caledonia" },
                {40,"New Zealand" },
                {65,"Nigeria" },
                {66,"Northern Ireland" },
                {24,"Norway" },
                {21,"Oman" },
                {67,"Papua New Guinea" },
                {68,"Phillipines" },
                {13,"Poland" },
                {36,"Portugal" },
                {19,"Puerto Rico" },
                {69,"Qatar" },
                {70,"Romania" },
                {10,"Russia" },
                {42,"San Marino" },
                {23,"Saudi Arabia" },
                {71,"Scotland" },
                {72,"Serbia" },
                {37,"Singapore" },
                {20,"Slovakia" },
                {73,"Slovenia" },
                {18,"South Africa" },
                {26,"South Korea" },
                {4,"Spain" },
                {30,"Sweden" },
                {8,"Switzerland" },
                {74,"Taiwan (China)" },
                {11,"Thailand" },
                {25,"Turkey" },
                {75,"Ukraine" },
                {43,"United Arab Emirates" },
                {83,"Uruguay" },
                {39,"USA" },
                {76,"Venezuela" },
                {77,"Wales" },
                {80,"Zimbabwe" },
            };
        private string GetNationality(int jsonValue)
        {

            string nationality = "Other";
            try { nationality = Nationalities[jsonValue]; } catch (Exception e) { }
            return nationality;
        }


        private Dictionary<int, string> RimMaterialTypes = new Dictionary<int, string>()
        {
            {1,"Glossy" },
            {2,"Matte" },
            {3,"Satin Metallic" },
            {4,"Metallic" },
            {5,"Chrome" }
        };
        private string GetRimMaterialType(int jsonValue)
        {
            string MaterialType = "Other";
            try { MaterialType = RimMaterialTypes[jsonValue]; } catch (Exception e) { Debug.WriteLine(e); }
            return MaterialType;
        }

        private Dictionary<int, string> BodyMaterialTypes = new Dictionary<int, string>()
        {
            {0,"Glossy" },
            {1,"Matte" },
            {2,"Satin" },
            {3,"Satin Metallic" },
            {4,"Metallic" },
            {5,"Chrome" }     ,
            {6,"Clear Chrome" }
        };
        private string GetBodyMaterialType(int jsonValue)
        {
            string MaterialType = "Other";
            try { MaterialType = BodyMaterialTypes[jsonValue]; } catch (Exception e) { Debug.WriteLine(e); }
            return MaterialType;
        }
    }
}
