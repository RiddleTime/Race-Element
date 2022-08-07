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
        public Guid CarGuid { get; set; }
        public CarClasses CarClass { get; set; }
        public string ParseName { get; set; }
    }

    internal class CarDataDB
    {
        public static void UpsertTrack(DbCarData car)
        {
            using (var db = LocalDatabase.Instance.Database)
            {
                db.GetCollection<DbCarData>().Upsert(car);
            }
        }
    }
}
