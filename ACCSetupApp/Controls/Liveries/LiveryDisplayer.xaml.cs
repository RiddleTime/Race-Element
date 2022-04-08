using ACCSetupApp.LiveryParser;
using ACCSetupApp.SetupParser;
using ACCSetupApp.Util;
using DdsFileTypePlus;
using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        internal static LiveryDisplayer Instance;

        private LiveryTreeCar Livery { get; set; }
        private ConversionFactory conversionFactory { get; set; }

        public LiveryDisplayer()
        {
            InitializeComponent();

            this.conversionFactory = new ConversionFactory();
            buttonGenerateDDS.Click += ButtonGenerateDDS_Click;

            Instance = this;
        }

        private Dictionary<string, string> pngsToDDS = new Dictionary<string, string>()
            {
                {"decals_0.dds","decals.png" },
                {"sponsors_0.dds" ,"sponsors.png"},
                {"decals_1.dds","decals.png" },
                {"sponsors_1.dds","sponsors.png" }
            };
        private void ButtonGenerateDDS_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.EnqueueSnackbarMessage($"Generating DDS files... this may take a while...");
            ThreadPool.QueueUserWorkItem(x =>
            {
                Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    buttonGenerateDDS.IsEnabled = false;
                    buttonGenerateDDS.Content = "Generating dds files... this may take a while";
                    LiveryBrowser.Instance.liveriesTreeView.IsEnabled = false;
                }));
                for (int i = 0; i < pngsToDDS.Count; i++)
                {
                    DirectoryInfo customSkinDir = new DirectoryInfo(FileUtil.LiveriesPath + Livery.carsRoot.customSkinName);
                    FileInfo[] sponsorFiles = customSkinDir.GetFiles(pngsToDDS.ElementAt(i).Value);
                    if (sponsorFiles != null && sponsorFiles.Length > 0)
                    {
                        MainWindow.Instance.EnqueueSnackbarMessage($"Generating {pngsToDDS.ElementAt(i).Key}");
                        FileInfo sponsorsFile = sponsorFiles[0];
                        FileStream actualFileStream = sponsorsFile.OpenRead();

                        Bitmap bitmap = new Bitmap(actualFileStream);

                        if (pngsToDDS.ElementAt(i).Key.Contains("_1"))
                        {
                            bitmap = ResizeBitmap(bitmap, 2048, 2048);
                        }

                        float hR = bitmap.HorizontalResolution;
                        float vR = bitmap.VerticalResolution;

                        Surface surface = Surface.CopyFromBitmap(bitmap);

                        FileInfo targetFile = new FileInfo($"{customSkinDir}\\{pngsToDDS.ElementAt(i).Key}");
                        if (targetFile.Exists)
                            targetFile.Delete();

                        FileStream write = targetFile.OpenWrite();
                        DdsFile.Save(write, DdsFileFormat.BC7, DdsErrorMetric.Perceptual, BC7CompressionMode.Slow, true, true, ResamplingAlgorithm.SuperSampling, surface, this.ProgressChanged);
                        write.Close();
                        actualFileStream.Close();

                        GC.Collect();
                    }
                }
                Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    buttonGenerateDDS.Visibility = Visibility.Hidden;
                    buttonGenerateDDS.IsEnabled = true;
                    buttonGenerateDDS.Content = "Generate DDS Files";

                    SetLivery(Livery);
                    LiveryBrowser.Instance.liveriesTreeView.IsEnabled = true;
                }));
                MainWindow.Instance.EnqueueSnackbarMessage($"DDS generating completed.");
            });
        }

        private Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }
            return result;
        }

        private bool HasDdsFiles()
        {
            for (int i = 0; i < pngsToDDS.Count; i++)
            {
                KeyValuePair<string, string> kvp = pngsToDDS.ElementAt(i);

                DirectoryInfo customSkinDir = new DirectoryInfo(FileUtil.LiveriesPath + Livery.carsRoot.customSkinName);
                //check if png exists
                FileInfo[] foundFiles = customSkinDir.GetFiles(kvp.Value);

                if (foundFiles != null && foundFiles.Length > 0)
                {
                    foundFiles = customSkinDir.GetFiles(kvp.Key);
                    if (foundFiles == null || foundFiles.Length == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal void SetLivery(LiveryTreeCar livery)
        {
            ThreadPool.QueueUserWorkItem(x => { GC.Collect(); });

            if (livery == null)
                return;

            Livery = livery;
            Livery.carsRoot = GetLivery(livery.carsFile);

            decalsLabel.Content = string.Empty;
            decalsImage.Source = null;
            sponsorsLabel.Content = string.Empty;
            sponsorsImage.Source = null;
            stackPanelLiveryInfo.Children.Clear();
            stackPanelMainInfo.Children.Clear();
            skinMainInfo.Visibility = Visibility.Hidden;
            buttonGenerateDDS.Visibility = HasDdsFiles() ? Visibility.Hidden : Visibility.Visible;


            ThreadPool.QueueUserWorkItem(x => { GC.Collect(); });


            ThreadPool.QueueUserWorkItem(x =>
            {
                Instance.Dispatcher.BeginInvoke(new Action(() =>
                {



                    CarsJson.Root carsRoot = Livery.carsRoot;
                    string customSkinName = carsRoot.customSkinName;

                    if (customSkinName != null && customSkinName.Length > 0)
                    {
                        skinMainInfo.Visibility = Visibility.Visible;

                        stackPanelMainInfo.Children.Add(GetInfoLabel($"{carsRoot.teamName}", HorizontalAlignment.Center, 25));
                        stackPanelMainInfo.Children.Add(GetInfoLabel($"{carsRoot.customSkinName}", HorizontalAlignment.Center, 19));
                        stackPanelMainInfo.Children.Add(GetInfoLabel($"{conversionFactory.GetCarName(carsRoot.carModelType)}", HorizontalAlignment.Center, 16));

                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Display Name: {carsRoot.displayName}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Race Number: {carsRoot.raceNumber}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Nationality: {GetNationality(carsRoot.nationality)}"));

                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($""));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Body Base Layer: {GetBodyMaterialType(carsRoot.skinMaterialType1)}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Body Accent: {GetBodyMaterialType(carsRoot.skinMaterialType2)}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Body Trim: {GetBodyMaterialType(carsRoot.skinMaterialType3)}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Rim Base: {GetRimMaterialType(carsRoot.rimMaterialType1)}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Rim Accent: {GetRimMaterialType(carsRoot.rimMaterialType2)}"));

                        DirectoryInfo customSkinDir = new DirectoryInfo(FileUtil.LiveriesPath + customSkinName);
                        if (customSkinDir.Exists)
                        {
                            FileInfo[] sponsorFiles = customSkinDir.GetFiles("sponsors.png");
                            if (sponsorFiles != null && sponsorFiles.Length > 0)
                            {
                                FileInfo sponsorsFile = sponsorFiles[0];
                                sponsorsLabel.Content = "Sponsors";
                                sponsorsImage.Source = new BitmapImage(new Uri(sponsorsFile.FullName, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
                            }


                            // // Displaying dds files takes way too fucking long.. 
                            //FileInfo[] sponsorDddsFiles = customSkinDir.GetFiles("sponsors_0.dds");
                            //if (sponsorDddsFiles != null && sponsorDddsFiles.Length > 0)
                            //{

                            //    // https://stackoverflow.com/questions/23618171/load-dds-file-from-stream-and-display-in-wpf-application

                            //    // https://stackoverflow.com/questions/1118496/using-image-control-in-wpf-to-display-system-drawing-bitmap/1118557#1118557
                            //    // https://code.google.com/archive/p/kprojects/downloads
                            //    // https://github.com/ptrsuder/ddsfiletype-plus-hack

                            //    FileInfo sponsorsFile = sponsorDddsFiles[0];

                            //    Surface surface = DdsFile.Load(sponsorsFile.FullName);


                            //    System.Drawing.Bitmap bmp = surface.CreateAliasedBitmap();
                            //    MemoryStream ms = new MemoryStream();
                            //    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            //    ms.Position = 0;
                            //    BitmapImage bi = new BitmapImage();
                            //    bi.BeginInit();
                            //    bi.StreamSource = ms;
                            //    bi.EndInit();
                            //    sponsorsImage.Source = bi;



                            //    //ThreadPool.QueueUserWorkItem(y =>
                            //    //{
                            //    //    FileInfo test = new FileInfo(sponsorsFile.FullName.Replace("_0", "_9779"));
                            //    //    if (test.Exists)
                            //    //        test.Delete();


                            //    //    FileStream write = test.OpenWrite();
                            //    //    DdsFile.Save(write, DdsFileFormat.BC7Srgb, DdsErrorMetric.Perceptual, BC7CompressionMode.Slow, true, true, ResamplingAlgorithm.Bilinear, surface, this.ProgressChanged);
                            //    //    //new BitmapImage(new Uri(sponsorsFile.FullName, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
                            //    //    write.Close();

                            //    //    GC.Collect();
                            //    //});
                            //}

                            FileInfo[] decalFiles = customSkinDir.GetFiles("decals.png");
                            if (decalFiles != null && decalFiles.Length > 0)
                            {
                                FileInfo decalsFile = decalFiles[0];
                                decalsLabel.Content = "Decals";
                                decalsImage.Source = new BitmapImage(new Uri(decalsFile.FullName, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable));
                            }


                            // // WIll never display dds files as generating them takes way too fucking long
                            //FileInfo[] decalDDSFiles = customSkinDir.GetFiles("decals_0.dds");
                            //if (decalDDSFiles != null && decalDDSFiles.Length > 0)
                            //{

                            //    // https://stackoverflow.com/questions/23618171/load-dds-file-from-stream-and-display-in-wpf-application

                            //    // https://stackoverflow.com/questions/1118496/using-image-control-in-wpf-to-display-system-drawing-bitmap/1118557#1118557
                            //    // https://code.google.com/archive/p/kprojects/downloads
                            //    // https://github.com/ptrsuder/ddsfiletype-plus-hack

                            //    FileInfo decalsFile = decalDDSFiles[0];

                            //    Surface surface = DdsFile.Load(decalsFile.FullName);

                            //    System.Drawing.Bitmap bmp = surface.CreateAliasedBitmap();
                            //    MemoryStream ms = new MemoryStream();
                            //    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            //    ms.Position = 0;
                            //    BitmapImage bi = new BitmapImage();
                            //    bi.BeginInit();
                            //    bi.StreamSource = ms;
                            //    bi.EndInit();
                            //    decalsImage.Source = bi;
                            //}
                        }
                    }
                }));
            });
        }

        private void ProgressChanged(object sender, ProgressEventArgs e)
        {

            Debug.WriteLine(e.Percent.ToString());
            //progressBar1.Value = (int) Math.Round(e.Percent);
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
