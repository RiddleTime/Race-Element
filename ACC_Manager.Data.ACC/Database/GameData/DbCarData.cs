using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.Data.ACC.Database.GameData
{
    internal class DbCarData
    {
#pragma warning disable IDE1006 // Naming Styles
        public Guid _id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public CarClasses CarClass { get; set; }
        public string ParseName { get; set; }
    }

    internal class CarDataCollection
    {
        public static void Create()
        {
            new Thread(() =>
            {
                var collection = LocalDatabase.Database.GetCollection<DbCarData>();
            }).Start();
        }

        public static void Upsert(DbCarData car)
        {
            Debug.WriteLine($"Inserting new car data {car._id} {car.CarClass} {car.ParseName}");

            var collection = LocalDatabase.Database.GetCollection<DbCarData>();
            collection.EnsureIndex(x => x._id, true);
            collection.Insert(car);
        }
    }
}
