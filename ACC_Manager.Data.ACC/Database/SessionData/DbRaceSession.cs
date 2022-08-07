using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.Data.ACC.Database.SessionData
{
    internal class DbRaceSession
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public DateTime UtcStart { get; set; }
        public AcSessionType SessionType { get; set; }
        public int SessionIndex { get; set; }
    }

    internal class RaceSessionCollection
    {
        public static void Upsert(DbRaceSession raceSession)
        {
            using (var db = LocalDatabase.Instance.Database)
            {
                var col = db.GetCollection<DbRaceSession>();
                col.Upsert(raceSession);
            }
        }
    }
}
