using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Util
{
    public class FileUtil
    {

        public static string AccManagerDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "ACC Manager\\";
        public static string AccManagerLogPath = AccManagerDocumentsPath + "\\" + "Log\\";
        public static string AccManagerTagsPath = AccManagerDocumentsPath + "\\" + "Tag\\";


        public static string AccPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Assetto Corsa Competizione\\";
        public static string CustomsPath => AccPath + "Customs\\";
        public static string CarsPath => CustomsPath + "Cars\\";
        public static string LiveriesPath => CustomsPath + "Liveries\\";

        public static string AppDirectory => StripFileName(System.Reflection.Assembly.GetEntryAssembly().Location);


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
    }
}
