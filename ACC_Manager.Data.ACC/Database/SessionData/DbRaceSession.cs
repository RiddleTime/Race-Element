using ACCManager.Data.ACC.Database.GameData;
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
        /// <summary>
        /// Returns all db Car Guids which exist with the given track guid
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public static List<Guid> GetAllCarsForTrack(Guid trackId)
        {
            var sessionCollection = LocalDatabase.Database.GetCollection<DbRaceSession>();

            return sessionCollection.Find(x => x.TrackGuid == trackId)
                .Select(x => x.CarGuid)
                .Distinct()
                .ToList();
        }

        public static List<DbRaceSession> GetAll()
        {
            var collection = LocalDatabase.Database.GetCollection<DbRaceSession>();
            return collection.FindAll().OrderBy(x => x.UtcStart).ToList();
        }

        public static void Update(DbRaceSession raceSession)
        {
            var collection = LocalDatabase.Database.GetCollection<DbRaceSession>();

            try
            {
                var storedSession = collection.FindOne(x => x._id == raceSession._id);
                if (storedSession != null)
                {
                    storedSession = raceSession;
                    collection.Update(storedSession);
                    Debug.WriteLine($"Updated Race session {raceSession._id}");
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        public static void Insert(DbRaceSession raceSession)
        {
            var collection = LocalDatabase.Database.GetCollection<DbRaceSession>();
            collection.EnsureIndex(x => x._id, true);

            collection.Insert(raceSession);

            Debug.WriteLine($"Inserted new race session {raceSession.SessionIndex} {raceSession.SessionType} {raceSession._id}");
        }
    }
}
