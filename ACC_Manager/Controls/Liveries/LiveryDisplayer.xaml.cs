using ACCManager.Controls.Liveries;
using ACCManager.Data;
using ACCManager.LiveryParser;
using ACCManager.Util;
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
using static ACCManager.Controls.LiveryBrowser;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for LiveryDisplayer.xaml
    /// </summary>
    public partial class LiveryDisplayer : UserControl
    {
        internal static LiveryDisplayer Instance;

        private LiveryTreeCar Livery { get; set; }
        private bool _isLoading = false;

        public LiveryDisplayer()
        {
            InitializeComponent();

            buttonGenerateDDS.Click += ButtonGenerateDDS_Click;

            Instance = this;

            this.IsVisibleChanged += (s, e) =>
            {
                if (!(bool)e.NewValue)
                {
                    Cache();
                }
            };
        }

        private void ButtonGenerateDDS_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.EnqueueSnackbarMessage($"Generating DDS files... this may take a while...");
            ThreadPool.QueueUserWorkItem((WaitCallback)(x =>
            {
                Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    buttonGenerateDDS.IsEnabled = false;
                    buttonGenerateDDS.Content = "Generating dds files... this may take a while";
                    LiveryBrowser.Instance.liveriesTreeViewTeams.IsEnabled = false;
                    LiveryBrowser.Instance.liveriesTreeViewCars.IsEnabled = false;
                    LiveryBrowser.Instance.buttonImportLiveries.IsEnabled = false;
                    LiveryBrowser.Instance.buttonGenerateAllDDS.IsEnabled = false;
                }));

                DDSutil.GenerateDDS(Livery);

                Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    buttonGenerateDDS.Visibility = Visibility.Hidden;
                    buttonGenerateDDS.IsEnabled = true;
                    buttonGenerateDDS.Content = "Generate DDS Files";

                    SetLivery(Livery);
                    LiveryBrowser.Instance.liveriesTreeViewTeams.IsEnabled = true;
                    LiveryBrowser.Instance.liveriesTreeViewCars.IsEnabled = true;
                    LiveryBrowser.Instance.buttonImportLiveries.IsEnabled = true;
                    LiveryBrowser.Instance.buttonGenerateAllDDS.IsEnabled = true;
                }));
                MainWindow.Instance.EnqueueSnackbarMessage($"DDS generating completed.");
            }));
        }

        public void ReloadLivery()
        {
            if (!_isLoading)
            {
                SetLivery(this.Livery);
                this.Visibility = Visibility.Visible;
            }
        }

        public void Cache()
        {
            Debug.WriteLine(imageGrid.Children.Count);

            foreach (var control in imageGrid.Children)
            {
                if (control is System.Windows.Controls.Image)
                {
                    System.Windows.Controls.Image imageControl = (System.Windows.Controls.Image)control;
                    BitmapImage image = (BitmapImage)imageControl.Source;
                    if (image != null && image.StreamSource != null)
                    {
                        image.StreamSource.Close();
                        image.StreamSource.Dispose();
                    }
                    imageControl.Source = null;
                    imageControl = null;
                }
            }
            imageGrid.Children.Clear();
            imageGrid.UpdateLayout();

            stackPanelLiveryInfo.Children.Clear();
            stackPanelMainInfo.Children.Clear();
            skinMainInfo.Visibility = Visibility.Hidden;
            buttonGenerateDDS.Visibility = Visibility.Hidden;

            this.Visibility = Visibility.Collapsed;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        }

        internal void SetLivery(LiveryTreeCar livery)
        {
            _isLoading = true;

            imageGrid.Children.Clear();
            stackPanelLiveryInfo.Children.Clear();
            stackPanelMainInfo.Children.Clear();
            skinMainInfo.Visibility = Visibility.Hidden;
            buttonGenerateDDS.Visibility = Visibility.Hidden;

            if (livery == null)
                return;

            Livery = livery;
            Livery.CarsRoot = LiveryImporter.GetLivery(livery.CarsFile);
            buttonGenerateDDS.Visibility = DDSutil.HasDdsFiles(Livery) ? Visibility.Hidden : Visibility.Visible;

            ThreadPool.QueueUserWorkItem(x =>
            {
                Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    CarsJson.Root carsRoot = Livery.CarsRoot;
                    string customSkinName = carsRoot.CustomSkinName;

                    if (customSkinName != null && customSkinName.Length > 0)
                    {
                        skinMainInfo.Visibility = Visibility.Visible;

                        if (carsRoot.TeamName != String.Empty)
                            stackPanelMainInfo.Children.Add(GetInfoLabel($"{carsRoot.TeamName}", HorizontalAlignment.Center, 25, "Team Name"));
                        stackPanelMainInfo.Children.Add(GetInfoLabel($"{carsRoot.CustomSkinName}", HorizontalAlignment.Center, 19, "Skin Name"));
                        stackPanelMainInfo.Children.Add(GetInfoLabel($"{ConversionFactory.GetCarName(carsRoot.CarModelType)}", HorizontalAlignment.Center, 16, "Car model type"));

                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Display Name: {carsRoot.DisplayName}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Race Number: {carsRoot.RaceNumber}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Nationality: {GetNationality(carsRoot.Nationality)}"));

                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($""));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Body Base Layer: {GetBodyMaterialType(carsRoot.SkinMaterialType1)}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Body Accent: {GetBodyMaterialType(carsRoot.SkinMaterialType2)}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Body Trim: {GetBodyMaterialType(carsRoot.SkinMaterialType3)}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Rim Base: {GetRimMaterialType(carsRoot.RimMaterialType1)}"));
                        stackPanelLiveryInfo.Children.Add(GetInfoLabel($"Rim Accent: {GetRimMaterialType(carsRoot.RimMaterialType2)}"));

                        DirectoryInfo customSkinDir = new DirectoryInfo(FileUtil.LiveriesPath + customSkinName);
                        if (customSkinDir.Exists)
                        {
                            imageGrid.Children.Clear();

                            FileInfo[] decalFiles = customSkinDir.GetFiles("decals.png");
                            if (decalFiles != null && decalFiles.Length > 0)
                            {
                                FileInfo decalsFile = decalFiles[0];
                                System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image()
                                {
                                    Stretch = Stretch.Fill,
                                    Height = 366,
                                    Width = 366,
                                };
                                imageGrid.Children.Add(imageControl);
                                LoadPhoto(imageControl, decalsFile.FullName);

                                ThreadPool.QueueUserWorkItem(gc =>
                                {
                                    Thread.Sleep(1 * 1000);
                                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                                });
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


                            FileInfo[] sponsorFiles = customSkinDir.GetFiles("sponsors.png");
                            if (sponsorFiles != null && sponsorFiles.Length > 0)
                            {
                                FileInfo sponsorsFile = sponsorFiles[0];
                                System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image() { Stretch = Stretch.UniformToFill, Height = 366, Width = 366 };
                                imageGrid.Children.Add(imageControl);
                                LoadPhoto(imageControl, sponsorsFile.FullName);
                                ThreadPool.QueueUserWorkItem(gc =>
                                {
                                    Thread.Sleep(2 * 1000);
                                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                                });
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




                            UpdateImageSize();
                            _isLoading = false;


                        }
                    }
                }));
            });
        }

        private void LoadPhoto(System.Windows.Controls.Image imageControl, string path)
        {
            ThreadPool.QueueUserWorkItem(x =>
            {
                BitmapImage bmi = new BitmapImage();
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    bmi.BeginInit();
                    bmi.CacheOption = BitmapCacheOption.OnLoad;
                    bmi.StreamSource = stream;
                    bmi.EndInit();

                    bmi.Freeze();
                    stream.Close();
                    stream.Dispose();
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    imageControl.Source = bmi;
                }));

            });
        }

        private Label GetInfoLabel(string text, HorizontalAlignment allignmment = HorizontalAlignment.Left, int size = 13, string toolTip = "")
        {
            Label label = new Label()
            {
                Content = text,
                Style = Resources["MaterialDesignLabel"] as Style,
                FontSize = size,
                HorizontalAlignment = allignmment
            };
            if (toolTip != string.Empty)
            {
                label.ToolTip = toolTip;
            }
            return label;
        }

        private static Dictionary<int, string> Nationalities = new Dictionary<int, string>()
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
            try { nationality = Nationalities[jsonValue]; } catch (Exception) { }
            return nationality;
        }


        private static Dictionary<int, string> RimMaterialTypes = new Dictionary<int, string>()
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

        private static Dictionary<int, string> BodyMaterialTypes = new Dictionary<int, string>()
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

        private void StackPanelDecals_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateImageSize();
        }

        private void UpdateImageSize()
        {
            double lowestSize = imageGrid.ActualWidth;
            if (imageGrid.ActualHeight < lowestSize)
            {
                lowestSize = imageGrid.ActualHeight;
            }

            foreach (System.Windows.Controls.Image imageControl in imageGrid.Children)
            {
                imageControl.Width = lowestSize;
                imageControl.Height = lowestSize;
            }
        }
    }
}
