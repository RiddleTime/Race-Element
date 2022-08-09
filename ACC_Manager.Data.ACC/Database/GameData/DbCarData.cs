using LiteDB;
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
        private static ILiteCollection<DbCarData> _collection;
        private static ILiteCollection<DbCarData> Collection
        {
            get
            {
                if (_collection == null)
                    _collection = LocalDatabase.Database.GetCollection<DbCarData>();

                return _collection;
            }
        }

        public static List<DbCarData> GetAll()
        {
            return Collection.FindAll().OrderBy(x => x.ParseName).ToList();
        }

        public static DbCarData GetCarData(Guid id)
        {
            return Collection.FindById(id);
        }

        public static DbCarData GetCarData(string parseName)
        {
            var result = Collection.FindOne(x => x.ParseName == parseName);

            if (result == null)
            {
                Insert(new DbCarData() { ParseName = parseName });
                result = Collection.FindOne(x => x.ParseName == parseName);
            }

            return result;
        }

        public static void Insert(DbCarData car)
        {
            Collection.EnsureIndex(x => x._id, true);
            Collection.Insert(car);

            Debug.WriteLine($"Inserted new car data {car._id} {car.ParseName}");
        }
    }
}
