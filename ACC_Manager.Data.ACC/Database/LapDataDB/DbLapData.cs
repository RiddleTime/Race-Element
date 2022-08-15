using ACCManager.Broadcast;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Database.LapDataDB
{
    /// <summary>
    /// All data except for the Index must be divided by 1000 to get the actual value (floating point precision is annoying)
    /// </summary>
    public class DbLapData
    {
        public Guid Id { get; set; }

        public Guid RaceSessionId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The time when this lap was completed
        /// </summary>
        public DateTime UtcCompleted { get; set; }

        /// <summary>
        /// Lap Index
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// Lap Time in milliseconds
        /// </summary>
        public int Time { get; set; } = -1;
        public int Sector1 { get; set; } = -1;
        public int Sector2 { get; set; } = -1;
        public int Sector3 { get; set; } = -1;

        public bool IsValid { get; set; } = true;

        public LapType LapType { get; set; } = LapType.ERROR;

        /// <summary>
        /// Milliliters of Fuel left at the end of the lap, divide by 1000...
        /// </summary>
        public int FuelUsage { get; set; } = -1;
        public float FuelInTank { get; set; } = -1;

        public override string ToString()
        {
            return $"Lap: {Index}, Time: {new TimeSpan(0, 0, 0, 0, Time):mm\\:ss\\:fff}, IsValid: {IsValid}, S1: {this.GetSector1():F3}, S2: {this.GetSector2():F3}, S3: {this.GetSector3():F3}, LapType: {this.LapType}";
        }
    }

    public class LapDataCollection
    {
        private static ILiteCollection<DbLapData> _collection;
        private static ILiteCollection<DbLapData> Collection
        {
            get
            {
                if (_collection == null)
                    _collection = RaceWeekendDatabase.Database.GetCollection<DbLapData>();

                return _collection;
            }
        }

        public static bool Any()
        {
            return Collection.Count() != 0;
        }

        public static void Insert(DbLapData lap)
        {
            RaceWeekendDatabase.Database.BeginTrans();
            Collection.EnsureIndex(x => x.Id, true);
            Collection.Insert(lap);
            RaceWeekendDatabase.Database.Commit();
        }

        public static Dictionary<int, DbLapData> GetForSession(Guid sessionId)
        {
            return Collection.Find(x => x.RaceSessionId == sessionId).ToDictionary(x => x.Index);
        }
    }
}
