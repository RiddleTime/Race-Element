using System;
using System.IO;
using System.Security.Cryptography;

namespace RaceElement.Util
{
    public class FileUtil
    {
        public static string RaceElementAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + "Race Element\\";

        public static string RaceElementInternalPath = RaceElementAppDataPath + "Internal\\";
        public static string RaceElementLogPath = RaceElementAppDataPath + "Log\\";
        public static string RaceElementTagsPath = RaceElementAppDataPath + "Tag\\";
        public static string RaceElementOverlayPath = RaceElementAppDataPath + "Overlay\\";
        public static string RaceElementSettingsPath = RaceElementAppDataPath + "Settings\\";
        public static string RaceElementDataPath = RaceElementAppDataPath + "Data\\";
        public static string RaceElementDownloadCachePath = RaceElementAppDataPath + "DownloadCache\\";


        public static string AccPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Assetto Corsa Competizione\\";
        public static string CustomsPath => AccPath + "Customs\\";
        public static string CarsPath => CustomsPath + "Cars\\";
        public static string LiveriesPath => CustomsPath + "Liveries\\";
        public static string AccConfigPath => AccPath + "Config\\";

        public static string AppDirectory => StripFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
        public static string AppFullName => AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName;


        /// <summary>
        /// Strips the file name from a windows directory path
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>removed the filename from the path is what it returns</returns>
        public static string StripFileName(string fileName)
        {
            string[] dashSplit = fileName.Split('\\');
            string result = String.Empty;

            for (int i = 0; i < dashSplit.Length - 1; i++)
            {
                result += dashSplit[i] + '\\';
            }

            return result;
        }

        public static string GetFileName(string fullName)
        {
            string[] split = fullName.Split('/');

            if (split.Length == 1 && split[0].Contains("\\"))
            {
                split = fullName.Split('\\');
            }

            return split[split.Length - 1].Replace("\\", "");
        }

        /// <summary>
        /// Calculates the SHA256 Hash of given file
        /// </summary>
        /// <param name="file"></param>
        /// <returns>base64 string of SHA256 hash, or empty string if file doesn't exist or when exception occurs</returns>
        public static string GetBase64Hash(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            if (!fileInfo.Exists) return string.Empty;

            try
            {
                using (FileStream fileStream = File.OpenRead(file))
                {
                    byte[] hash = SHA256Managed.Create().ComputeHash((Stream)fileStream);
                    fileStream.Close();
                    return Convert.ToBase64String(hash);
                }
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog(e);
                return string.Empty;
            }
        }
    }
}
