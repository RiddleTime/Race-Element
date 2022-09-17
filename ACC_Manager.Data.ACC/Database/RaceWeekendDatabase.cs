using ACCManager.Data.ACC.Database.LapDataDB;
using ACCManager.Util;
using LiteDB;
using System;
using System.Diagnostics;
using System.IO;

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


                if (_database == null)
                    _database = new LiteDatabase($"Filename={fileName}; Initial Size=16KB;");

                if (_database == null)
                    Trace.WriteLine("Something went wrong initializing the LocalDatabase.Database");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return _database;

        }

        public static LiteDatabase OpenDatabase(string file)
        {
            try
            {
                LiteDatabase db = new LiteDatabase($"Filename={file}; Mode=ReadOnly;");

                if (db == null)
                    Trace.WriteLine("Something went wrong initializing the LocalDatabase.Database");
                else
                    return db;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return null;
        }

        public static LiteDatabase Database { get => _database; }

        public static void Close()
        {
            if (_database == null)
                return;

            bool anyLaps = LapDataCollection.Any();
            _database.Dispose();
            _database = null;

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
