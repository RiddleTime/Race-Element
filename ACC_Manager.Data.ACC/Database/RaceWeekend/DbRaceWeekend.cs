using LiteDB;
using System;
using System.Diagnostics;
using System.Linq;

namespace ACCManager.Data.ACC.Database.RaceWeekend
{
    public class DbRaceWeekend
    {
        public Guid Id { get; set; }

        public DateTime UtcStart { get; set; }
        public DateTime UtcEnd { get; set; }
    }

    public class RaceWeekendCollection
    {
        public static DbRaceWeekend Current { get; private set; }

        private static ILiteCollection<DbRaceWeekend> Collection
        {
            get
            {
                ILiteCollection<DbRaceWeekend> _collection = RaceWeekendDatabase.Database.GetCollection<DbRaceWeekend>();
                _collection.EnsureIndex(x => x.Id, true);
                return _collection;
            }
        }

        public static void End()
        {
            try
            {
                var storedWeekend = Collection.FindOne(x => x.Id == Current.Id);
                if (storedWeekend != null)
                {
                    storedWeekend.UtcEnd = DateTime.UtcNow;
                    Collection.Update(storedWeekend);
                    Debug.WriteLine($"Updated Race weekend {Current.Id}");
                    RaceWeekendDatabase.Close();
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        public static void Insert(DbRaceWeekend raceWeekend)
        {
            RaceWeekendDatabase.Database.BeginTrans();
            Collection.Insert(raceWeekend);
            RaceWeekendDatabase.Database.Commit();
            Current = Collection.FindAll().First();
            Debug.WriteLine($"Inserted new race weekend {raceWeekend.Id}");
        }
    }
}
