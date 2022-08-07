using ACCManager.Broadcast;
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
        public Guid RaceSessionGuid { get; set; } = new Guid();
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

        public override string ToString()
        {
            return $"Lap: {Index}, Time: {this.GetLapTime():F3}, IsValid: {IsValid}, S1: {this.GetSector1():F3}, S2: {this.GetSector2():F3}, S3: {this.GetSector3():F3}, LapType: {this.LapType}";
        }
    }

    public class LapDataDB
    {
        public static void UpsertLap(DbLapData lap)
        {
            using (var db = LocalDatabase.Instance.Database)
            {
                db.GetCollection<DbLapData>().Upsert(lap);
            }
        }
    }
}
