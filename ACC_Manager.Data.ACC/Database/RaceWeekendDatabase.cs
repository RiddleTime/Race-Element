using ACCManager.Data.ACC.Database.LapDataDB;
using ACCManager.Util;
using LiteDB;
using LiteDB.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Database
{
    /// <summary>
    /// https://github.com/mbdavid/LiteDB
    /// </summary>
    public class RaceWeekendDatabase
    {
        private static string fileName;

        private static LiteDatabase _database;
        internal static LiteDatabase CreateDatabase(string trackParseName, string carParseName, DateTime startTime)
        {
            try
            {
                DirectoryInfo dataDir = new DirectoryInfo(FileUtil.AccManangerDataPath);
                if (!dataDir.Exists)
                    dataDir.Create();

                fileName = FileUtil.AccManangerDataPath + $"{trackParseName}-{carParseName}-" + $"{startTime:G}".Replace(":", ".").Replace("/", ".").Replace(" ", "-") + ".rwdb";


                if (_database == null) _database = new LiteDatabase($"Filename={fileName};");
                if (_database == null) Trace.WriteLine("Something went wrong initializing the LocalDatabase.Database");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return _database;

        }

        public static LiteDatabase Database { get => _database; }

        public static void Close()
        {
            bool anyLaps = LapDataCollection.Any();
            _database.Dispose();

            if (!anyLaps)
            {
                try
                {
                    new FileInfo(fileName).Delete();
                    Debug.WriteLine("Deleted rwdb file, contains no laps.");
                }
                catch (Exception e)
                {
                    LogWriter.WriteToLog(e);
                }
            }
        }
    }
}
