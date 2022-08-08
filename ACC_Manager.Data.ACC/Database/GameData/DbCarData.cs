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
    public class DbCarData
    {
#pragma warning disable IDE1006 // Naming Styles
        public Guid _id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public string ParseName { get; set; }
    }

    public class CarDataCollection
    {
        public static DbCarData GetCarData(Guid id)
        {
            var collection = LocalDatabase.Database.GetCollection<DbCarData>();
            return collection.FindById(id);
        }

        public static DbCarData GetCarData(string parseName)
        {
            var collection = LocalDatabase.Database.GetCollection<DbCarData>();
            var result = collection.FindOne(x => x.ParseName == parseName);

            if (result == null)
            {
                CreateNewCar(parseName);
                result = collection.FindOne(x => x.ParseName == parseName);
            }

            return result;
        }

        private static void CreateNewCar(string parseName)
        {
            Insert(new DbCarData() { ParseName = parseName });
        }

        public static void Insert(DbCarData car)
        {
            var collection = LocalDatabase.Database.GetCollection<DbCarData>();
            collection.EnsureIndex(x => x._id, true);
            collection.Insert(car);

            Debug.WriteLine($"Inserted new car data {car._id} {car.ParseName}");
        }
    }
}
