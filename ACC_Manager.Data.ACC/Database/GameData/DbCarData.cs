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
        public Guid Id { get; set; }

        public string ParseName { get; set; }
    }

    public class CarDataCollection
    {
        private static ILiteCollection<DbCarData> Collection
        {
            get
            {
                ILiteCollection<DbCarData> _collection = RaceWeekendDatabase.Database.GetCollection<DbCarData>();

                return _collection;
            }
        }

        private static ILiteCollection<DbCarData> GetCollection(ILiteDatabase db)
        {
            return db.GetCollection<DbCarData>();
        }

        public static List<DbCarData> GetAll(LiteDatabase db)
        {
            return GetCollection(db).FindAll().OrderBy(x => x.ParseName).ToList();
        }

        public static List<DbCarData> GetAll()
        {
            return Collection.FindAll().OrderBy(x => x.ParseName).ToList();
        }

        public static DbCarData GetCarData(ILiteDatabase db, Guid id)
        {
            return GetCollection(db).FindById(id);
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
            RaceWeekendDatabase.Database.BeginTrans();
            Collection.EnsureIndex(x => x.Id, true);
            Collection.Insert(car);
            RaceWeekendDatabase.Database.Commit();

            Debug.WriteLine($"Inserted new car data {car.Id} {car.ParseName}");
        }
    }
}
