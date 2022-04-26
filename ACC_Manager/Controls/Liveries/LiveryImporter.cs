using ACCSetupApp.LiveryParser;
using ACCSetupApp.Util;
using Newtonsoft.Json;
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using static ACCSetupApp.Controls.LiveryBrowser;

namespace ACCSetupApp.Controls
{
    internal static class LiveryImporter
    {

        private static int liveryCount = 0;
        private static List<LiveryTreeCar> ImportedLiveries = new List<LiveryTreeCar>();

        public static void ImportLiveryZips()
        {
            liveryCount = 0;
            ImportedLiveries.Clear();

            try
            {
                LiveryBrowser.Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LiveryBrowser.Instance.liveriesTreeViewTeams.IsEnabled = false;
                    LiveryBrowser.Instance.liveriesTreeViewCars.IsEnabled = false;
                    LiveryBrowser.Instance.buttonImportLiveries.IsEnabled = false;
                    LiveryBrowser.Instance.buttonGenerateAllDDS.IsEnabled = false;
                    LiveryDisplayer.Instance.IsEnabled = false;
                }));

                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension and default file extension 
                dlg.AddExtension = true;
                dlg.CheckPathExists = true;
                dlg.Filter = "Livery archive|*.zip;*.rar;*.7z";
                dlg.Multiselect = true;
                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    string[] fileNames = dlg.FileNames;
                    if (fileNames != null && fileNames.Length > 0)
                    {
                        for (int i = 0; i < fileNames.Length; i++)
                        {
                            FileInfo fi = new FileInfo(fileNames[i]);
                            if (fi.Exists)
                            {
                                FileStream fs = fi.OpenRead();
                                if (fs != null)
                                    ImportArchive(fi);

                                fs.Close();
                            }
                        }
                    }
                }
                else
                {
                    goto enableUI;
                }

            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }

            if (liveryCount > 0)
            {
                MainWindow.Instance.EnqueueSnackbarMessage($"Imported {liveryCount} liveries.");
                LiveryBrowser.Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Debug.WriteLine("opening AfterImport.");
                    AfterImport.Instance.Open(ImportedLiveries);
                }));
            }
            else
                MainWindow.Instance.EnqueueSnackbarMessage($"The archive(s) you choose are not supported. Manually import them.");


            enableUI:
            LiveryBrowser.Instance.FetchAllCars();
            LiveryBrowser.Instance.Dispatcher.BeginInvoke(new Action(() =>
            {
                LiveryBrowser.Instance.liveriesTreeViewTeams.IsEnabled = true;
                LiveryBrowser.Instance.liveriesTreeViewCars.IsEnabled = true;
                LiveryBrowser.Instance.buttonImportLiveries.IsEnabled = true;
                LiveryBrowser.Instance.buttonGenerateAllDDS.IsEnabled = true;
                LiveryDisplayer.Instance.IsEnabled = true;
            }));
        }

        private static void ImportArchive(FileInfo fi)
        {
            IArchive archive = null;

            switch (fi.Extension)
            {
                case ".7z":
                case ".zip":
                case ".rar": archive = ArchiveFactory.Open(fi.FullName, new SharpCompress.Readers.ReaderOptions() { LeaveStreamOpen = false }); break;
                default:
                    {
                        return;
                    }
            }

            List<IArchiveEntry> archiveEntries = archive.Entries.ToList();
            List<IArchiveEntry> availableSkins = archiveEntries.FindAll((x) =>
            {
                Stream entryStream = x.OpenEntryStream();
                CarsJson.Root carRoot = GetLivery(entryStream);
                entryStream.Close();
                return carRoot != null && carRoot.customSkinName != null && !carRoot.customSkinName.Equals(string.Empty);
            });

            // check importing strategy
            if (availableSkins.Count > 0)
            {
                List<IArchiveEntry> carsFolders = archiveEntries.FindAll((x) =>
                  {
                      return x.Key.ToLower().EndsWith("cars/") || x.Key.ToLower().EndsWith("cars\\");
                  });

                if (availableSkins.Count == 1)
                {
                    Debug.WriteLine("WTF???? noob export, going to try the botched import strategy.");
                    ImportStrategies.BotchedImportStrategy(archiveEntries);
                }
                else
                {
                    Debug.WriteLine("Multiple liveries detected within 1 archive, using default import strategy.");
                    ImportStrategies.DefaultImportStrategy(archiveEntries);
                }
            }

            archive.Dispose();
        }

        private static class ImportStrategies
        {
            public static void BotchedImportStrategy(List<IArchiveEntry> archiveEntries)
            {
                archiveEntries.ForEach(x =>
                {
                    //Debug.WriteLine(x.Key);
                    if (!x.Key.StartsWith("__MACOSX"))
                    {
                        CarsJson.Root carRoot = GetLivery(x.OpenEntryStream());
                        if (carRoot != null && carRoot.customSkinName != null && carRoot.customSkinName != string.Empty)
                        {
                            Debug.WriteLine($"Found livery {carRoot.teamName} / {carRoot.customSkinName}");

                            string carsJsonFileName = $"{FileUtil.CarsPath}{FileUtil.GetFileName(x.Key)}";

                            string[] validSkinFiles = new string[] { "sponsors.json", "sponsors.png", "decals.json", "decals.png", ".dds" };

                            List<IArchiveEntry> skinFiles = archiveEntries.FindAll(s =>
                            {
                                foreach (string validSkinFile in validSkinFiles)
                                {
                                    if (s.Key.ToLower().EndsWith(validSkinFile) && !FileUtil.GetFileName(s.Key).StartsWith(".") && !s.Key.Contains("__MACOSX"))
                                    {
                                        return true;
                                    }
                                }

                                return false;

                            });

                            if (skinFiles.Count == 0)
                            {
                                skinFiles = archiveEntries.FindAll(s => s.Key.Contains($"\\{carRoot.customSkinName}\\"));
                            }
                            if (skinFiles.Count == 0)
                            {
                                skinFiles = archiveEntries.FindAll(s => s.Key.Contains($"\\\\{carRoot.customSkinName}\\\\"));
                            }


                            if (skinFiles.Count > 0)
                            {
                                x.WriteToFile(carsJsonFileName);

                                foreach (IArchiveEntry skinFile in skinFiles)
                                {
                                    for (int s = 0; s < validSkinFiles.Length; s++)
                                    {
                                        if (skinFile.Key.ToLower().EndsWith(validSkinFiles[s]))
                                        {
                                            string liveryFolder = $"{FileUtil.LiveriesPath}{carRoot.customSkinName}\\";
                                            Directory.CreateDirectory(liveryFolder);

                                            string skinFileName = $"{liveryFolder}{FileUtil.GetFileName(skinFile.Key)}";
                                            skinFile.WriteToFile(skinFileName);
                                            Debug.WriteLine($"Imported livery file: {skinFileName}");


                                        }
                                    }

                                }
                                LiveryTreeCar ltc = new LiveryTreeCar() { carsFile = new FileInfo(carsJsonFileName), carsRoot = carRoot };
                                if (!ImportedLiveries.Contains(ltc))
                                {
                                    ImportedLiveries.Add(ltc);
                                    liveryCount++;
                                }
                                MainWindow.Instance.EnqueueSnackbarMessage($"Imported {carRoot.teamName} / {carRoot.customSkinName}");
                            }
                        }
                    };
                });
            }

            public static void DefaultImportStrategy(List<IArchiveEntry> archiveEntries)
            {
                archiveEntries.ForEach(x =>
                {
                    //Debug.WriteLine(x.Key);
                    if (!x.Key.StartsWith("__MACOSX") && (x.Key.ToLower().Contains("cars/") || x.Key.ToLower().Contains("cars\\")))
                    {
                        Stream entryStream = x.OpenEntryStream();
                        CarsJson.Root carRoot = GetLivery(entryStream);
                        entryStream.Close();
                        if (carRoot != null && carRoot.customSkinName != null && carRoot.customSkinName != string.Empty)
                        {
                            Debug.WriteLine($"Found livery {carRoot.teamName} / {carRoot.customSkinName}");

                            string carsJsonFileName = $"{FileUtil.CarsPath}{FileUtil.GetFileName(x.Key)}";


                            List<IArchiveEntry> skinFiles = archiveEntries.FindAll(s =>
                            {
                                return s.Key.Contains($"/{carRoot.customSkinName}/") && !FileUtil.GetFileName(s.Key).StartsWith(".") && !s.Key.Contains("__MACOSX");
                            });

                            if (skinFiles.Count == 0)
                            {
                                skinFiles = archiveEntries.FindAll(s => s.Key.Contains($"\\{carRoot.customSkinName}\\"));
                            }
                            if (skinFiles.Count == 0)
                            {
                                skinFiles = archiveEntries.FindAll(s => s.Key.Contains($"\\\\{carRoot.customSkinName}\\\\"));
                            }

                            if (skinFiles.Count > 0)
                            {
                                x.WriteToFile(carsJsonFileName);

                                string[] validSkinFiles = new string[] { "sponsors.json", "sponsors.png", "decals.json", "decals.png", ".dds" };
                                foreach (IArchiveEntry skinFile in skinFiles)
                                {
                                    for (int s = 0; s < validSkinFiles.Length; s++)
                                    {
                                        if (skinFile.Key.ToLower().EndsWith(validSkinFiles[s]))
                                        {
                                            string liveryFolder = $"{FileUtil.LiveriesPath}{carRoot.customSkinName}\\";
                                            Directory.CreateDirectory(liveryFolder);

                                            string skinFileName = $"{liveryFolder}{FileUtil.GetFileName(skinFile.Key)}";
                                            skinFile.WriteToFile(skinFileName);
                                            Debug.WriteLine($"Imported livery file: {skinFileName}");

                                        }
                                    }

                                }
                                LiveryTreeCar ltc = new LiveryTreeCar() { carsFile = new FileInfo(carsJsonFileName), carsRoot = carRoot };
                                if (!ImportedLiveries.Contains(ltc))
                                {
                                    ImportedLiveries.Add(ltc);
                                    liveryCount++;
                                }
                                MainWindow.Instance.EnqueueSnackbarMessage($"Imported {carRoot.teamName} / {carRoot.customSkinName}");
                            }
                        }
                    };
                });
            }
        }


        public static CarsJson.Root GetLivery(FileInfo file)
        {
            if (!file.Exists)
                return null;

            try
            {
                using (FileStream fileStream = file.OpenRead())
                {
                    return GetLivery(fileStream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                //LogWriter.WriteToLog(ex);
            }
            return null;
        }

        public static CarsJson.Root GetLivery(Stream stream)
        {
            string jsonString = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    jsonString = reader.ReadToEnd();
                    jsonString = jsonString.Replace("\0", "");
                    reader.Close();
                    stream.Close();
                }

                return JsonConvert.DeserializeObject<CarsJson.Root>(jsonString);
            }
            catch (Exception e)
            {
                //LogWriter.WriteToLog(e);
                Debug.WriteLine(e);
            }

            return null;
        }
    }
}
