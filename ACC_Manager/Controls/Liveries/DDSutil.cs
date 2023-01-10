using RaceElement.Util;
using DdsFileTypePlus;
using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using static RaceElement.Controls.LiveryBrowser;

namespace RaceElement.Controls.Liveries
{
    internal class DDSutil
    {
        private static readonly Dictionary<string, string> pngsToDDS = new Dictionary<string, string>()
            {
                //{"decals_0.dds","decals.png" },
                //{"sponsors_0.dds" ,"sponsors.png"},
                {"decals_1.dds","decals.png" },
                {"sponsors_1.dds","sponsors.png" }
            };

        public static void GenerateDDS(LiveryTreeCar livery)
        {
            try
            {
                for (int i = 0; i < pngsToDDS.Count; i++)
                {
                    DirectoryInfo customSkinDir = new DirectoryInfo(FileUtil.LiveriesPath + livery.CarsRoot.CustomSkinName);
                    FileInfo[] liveryPngFiles = customSkinDir.GetFiles(pngsToDDS.ElementAt(i).Value);
                    if (liveryPngFiles != null && liveryPngFiles.Length > 0)
                    {
                        MainWindow.Instance.Dispatcher.Invoke(new Action(() =>
                        {
                            MainWindow.Instance.ClearSnackbar();
                            MainWindow.Instance.EnqueueSnackbarMessage($"Generating {pngsToDDS.ElementAt(i).Key} for {livery.CarsRoot.CustomSkinName}");
                        }));

                        FileInfo pngFile = liveryPngFiles[0];
                        FileStream actualFileStream = pngFile.OpenRead();

                        Bitmap bitmap = new Bitmap(actualFileStream);

                        if (pngsToDDS.ElementAt(i).Key.Contains("_1"))
                        {
                            bitmap = ResizeBitmap(bitmap, 2048, 2048);
                        }

                        Surface surface = Surface.CopyFromBitmap(bitmap);

                        FileInfo targetFile = new FileInfo($"{customSkinDir}\\{pngsToDDS.ElementAt(i).Key}");
                        if (targetFile.Exists)
                            targetFile.Delete();

                        FileStream write = targetFile.OpenWrite();
                        DdsFile.Save(write, DdsFileFormat.BC7, DdsErrorMetric.Perceptual, BC7CompressionMode.Slow, true, true, ResamplingAlgorithm.SuperSampling, surface, ProgressChanged);
                        write.Close();
                        actualFileStream.Close();

                        GC.Collect();
                    }
                }
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog(e);
            }
        }

        public static bool HasDdsFiles(LiveryTreeCar livery)
        {
            try
            {
                for (int i = 0; i < pngsToDDS.Count; i++)
                {
                    KeyValuePair<string, string> kvp = pngsToDDS.ElementAt(i);

                    DirectoryInfo customSkinDir = new DirectoryInfo(FileUtil.LiveriesPath + livery.CarsRoot.CustomSkinName);
                    if (customSkinDir != null && customSkinDir.Exists)
                    {
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
                }
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog(e);
                return false;
            }

            return true;
        }

        private static Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }
            return result;
        }

        private static void ProgressChanged(object sender, ProgressEventArgs e)
        {
            //Debug.WriteLine(e.Percent.ToString());
            //progressBar1.Value = (int) Math.Round(e.Percent);
        }

    }
}
