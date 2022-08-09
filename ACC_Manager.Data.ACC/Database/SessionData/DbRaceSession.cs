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
#pragma warning disable IDE1006 // Naming Styles
        public Guid _id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public Guid TrackGuid { get; set; }
        public Guid CarGuid { get; set; }

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
                    _collection = LocalDatabase.Database.GetCollection<DbRaceSession>();

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
            return Collection.Find(x => x.TrackGuid == trackId)
                .Select(x => x.CarGuid)
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
                var storedSession = Collection.FindOne(x => x._id == raceSession._id);
                if (storedSession != null)
                {
                    storedSession = raceSession;
                    Collection.Update(storedSession);
                    Debug.WriteLine($"Updated Race session {raceSession._id}");
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        public static void Insert(DbRaceSession raceSession)
        {
            Collection.EnsureIndex(x => x._id, true);
            Collection.Insert(raceSession);

            Debug.WriteLine($"Inserted new race session {raceSession.SessionIndex} {raceSession.SessionType} {raceSession._id}");
        }
    }
}
