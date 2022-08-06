using ACCManager.Broadcast;
using ACCManager.Data.ACC.Tracker.Laps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Tracker.LapDataDB
{
    /// <summary>
    /// All data except for the Index must be divided by 1000 to get the actual value (floating point precision is annoying)
    /// </summary>
    public class LapData
    {
        public Guid CarGuid { get; set; }
        /// <summary>
        /// Lap Index
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// Lap Time
        /// </summary>
        public int Time { get; set; } = -1;
        public bool IsValid { get; set; } = true;
        public int Sector1 { get; set; } = -1;
        public int Sector2 { get; set; } = -1;
        public int Sector3 { get; set; } = -1;

        public LapType LapType { get; set; } = LapType.ERROR;

        /// <summary>
        /// Fuel left at the end of the lap, divide by 1000...
        /// </summary>
        public int FuelUsage { get; set; } = -1;

        public override string ToString()
        {
            return $"Lap: {Index}, Time: {this.GetLapTime():F3}, IsValid: {IsValid}, S1: {this.GetSector1():F3}, S2: {this.GetSector2():F3}, S3: {this.GetSector3():F3}";
        }
    }
}
