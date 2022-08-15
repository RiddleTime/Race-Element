using ACCManager.Data.ACC.Database.GameData;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.Data.ACC.Database.SessionData
{
    public class DbRaceSession
    {
        public Guid Id { get; set; }

        public Guid RaceWeekendId { get; set; }
        public Guid TrackId { get; set; }
        public Guid CarId { get; set; }

        public DateTime UtcStart { get; set; }
        public DateTime UtcEnd { get; set; }
        public AcSessionType SessionType { get; set; }
        public bool IsOnline { get; set; }
        public int SessionIndex { get; set; }
    }

    public class RaceSessionCollection
    {
        private static ILiteCollection<DbRaceSession> _collection;
        private static ILiteCollection<DbRaceSession> Collection
        {
            get
            {
                if (_collection == null)
                    _collection = RaceWeekendDatabase.Database.GetCollection<DbRaceSession>();

                return _collection;
            }
        }

        /// <summary>
        /// Returns all db Car Guids which exist with the given track guid
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public static List<Guid> GetAllCarsForTrack(Guid trackId)
        {
            return Collection.Find(x => x.TrackId == trackId)
                .Select(x => x.CarId)
                .Distinct()
                .ToList();
        }

        public static List<DbRaceSession> GetAll()
        {
            return Collection.FindAll().OrderByDescending(x => x.UtcStart).ToList();
        }

        public static void Update(DbRaceSession raceSession)
        {
            try
            {
                var storedSession = Collection.FindOne(x => x.Id == raceSession.Id);
                if (storedSession != null)
                {
                    storedSession = raceSession;
                    Collection.Update(storedSession);
                    Debug.WriteLine($"Updated Race session {raceSession.Id}");
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        public static void Insert(DbRaceSession raceSession)
        {
            RaceWeekendDatabase.Database.BeginTrans();
            Collection.EnsureIndex(x => x.Id, true);
            Collection.Insert(raceSession);
            RaceWeekendDatabase.Database.Commit();
            Debug.WriteLine($"Inserted new race session {raceSession.SessionIndex} {raceSession.SessionType} {raceSession.Id}");
        }

        public static void Delete(DbRaceSession raceSession)
        {
            RaceWeekendDatabase.Database.BeginTrans();
            Collection.Delete(raceSession.Id);
            RaceWeekendDatabase.Database.Commit();
        }
    }
}
