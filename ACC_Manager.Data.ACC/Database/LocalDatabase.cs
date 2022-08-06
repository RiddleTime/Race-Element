using ACCManager.Util;
using LiteDB;
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
        private static LocalDatabase _instance;
        public static LocalDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LocalDatabase();

                return _instance;
            }
        }

        private readonly string _file = FileUtil.AccManangerDataPath + "accm.db";

        public LiteDatabase Database
        {
            get
            {
                return new LiteDatabase(_file);
            }
        }
    }
}
