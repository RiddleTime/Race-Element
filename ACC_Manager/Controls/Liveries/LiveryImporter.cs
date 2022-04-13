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

namespace ACCSetupApp.Controls
{
    internal static class LiveryImporter
    {

        public static void ImportLiveryZips()
        {
            try
            {
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
                            if (fi.Exists && fi.OpenRead() != null)
                            {
                                IArchive archive = null;

                                switch (fi.Extension)
                                {
                                    case ".7z":
                                    case ".zip":
                                    case ".rar": archive = ArchiveFactory.Open(fileNames[i]); break;
                                    default:
                                        {
                                            return;
                                        }
                                }

                                List<IArchiveEntry> archiveEntries = archive.Entries.ToList();
                                archiveEntries.ForEach(x =>
                                {
                                    //Debug.WriteLine(x.Key);
                                    if (!x.Key.StartsWith("__MACOSX") && (x.Key.ToLower().Contains("cars/") || x.Key.ToLower().Contains("cars\\")))
                                    {
                                        CarsJson.Root carRoot = GetLivery(x.OpenEntryStream());
                                        if (carRoot != null && carRoot.customSkinName != string.Empty)
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

                                                MainWindow.Instance.EnqueueSnackbarMessage($"Imported {carRoot.teamName} / {carRoot.customSkinName}");
                                            }
                                        }
                                    };
                                });
                            }

                        }
                    }

                }

            }
            catch (Exception ex) { LogWriter.WriteToLog(ex); }

            MainWindow.Instance.EnqueueSnackbarMessage($"Finished importing liveries");
            LiveryBrowser.Instance.FetchAllCars();
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
                LogWriter.WriteToLog(ex);
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
                LogWriter.WriteToLog(e);
                Debug.WriteLine(e);
            }

            return null;
        }
    }
}
