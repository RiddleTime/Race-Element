using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.Data.ACC.Database.GameData
{
    internal class DbCarData
    {
        public Guid Guid { get; set; }
        public CarClasses CarClass { get; set; }
        public string ParseName { get; set; }
    }
}
