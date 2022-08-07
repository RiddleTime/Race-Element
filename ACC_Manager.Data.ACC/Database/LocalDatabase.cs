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
    public class LocalDatabase
    {
        private static readonly string _file = FileUtil.AccManangerDataPath + "accm.db";

        private static LiteDatabase _database;
        public static LiteDatabase Database
        {
            get
            {
                try
                {
                    if (_database == null) _database = new LiteDatabase($"Filename={_file};");
                    if (_database == null) Trace.WriteLine("Something went wrong initializing the LocalDatabase.Database");

                    Trace.WriteLine($"ACCM DB Version: {_database.UserVersion}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                return _database;
            }
        }

        public static void Close()
        {
            _database.Dispose();
        }
    }
}
