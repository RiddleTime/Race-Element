using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Util;
using LiteDB;
using System;
using System.Diagnostics;
using System.IO;

namespace RaceElement.Data.ACC.Database
{
    /// <summary>
    /// https://github.com/mbdavid/LiteDB
    /// </summary>
    public class RaceWeekendDatabase
    {
        private static string fileName;

        internal static LiteDatabase CreateDatabase(string trackParseName, string carParseName, DateTime startTime)
        {
            try
            {
                DirectoryInfo dataDir = new DirectoryInfo(FileUtil.AccManangerDataPath);
                if (!dataDir.Exists)
                    dataDir.Create();

                fileName = FileUtil.AccManangerDataPath + $"{trackParseName}-{carParseName}-" + $"{startTime:G}".Replace(":", ".").Replace("/", ".").Replace(" ", "-") + ".rwdb";


                if (Database == null)
                    Database = new LiteDatabase($"Filename={fileName}; Initial Size=16KB;");

                if (Database == null)
                    Trace.WriteLine("Something went wrong initializing the LocalDatabase.Database");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return Database;

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

        public static LiteDatabase Database { get; private set; }

        public static void Close()
        {
            if (Database == null)
                return;

            bool anyLaps = LapDataCollection.Any();
            Database.Dispose();
            Database = null;

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
