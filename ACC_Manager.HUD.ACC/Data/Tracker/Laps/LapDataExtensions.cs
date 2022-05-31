using ACC_Manager.Util.NumberExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Data.Tracker.Laps
{
    public static class LapDataExtensions
    {
        #region LapData Extensions

        public static float GetLapTime(this LapData lap)
        {
            return lap.Time / 1000f;
        }

        public static float GetSector1(this LapData lap)
        {
            return lap.Sector1 / 1000f;
        }

        public static float GetSector2(this LapData lap)
        {
            return lap.Sector2 / 1000f;
        }

        public static float GetSector3(this LapData lap)
        {
            return lap.Sector3 / 1000f;
        }

        public static float GetFuelUsage(this LapData lap)
        {
            return lap.FuelUsage / 1000f;
        }

        #endregion

        #region List<LapData> Extensions

        public static int GetPotentialFastestLapTime(this List<LapData> laps)
        {
            if (laps.Count == 0) return -1;

            int fastestSector1 = laps.GetFastestSector(1);
            if (fastestSector1 == int.MaxValue) return -1;

            int fastestSector2 = laps.GetFastestSector(2);
            if (fastestSector2 == int.MaxValue) return -1;

            int fastestSector3 = laps.GetFastestSector(3);
            if (fastestSector3 == int.MaxValue) return -1;

            return fastestSector1 + fastestSector2 + fastestSector3;
        }

        public static int GetAverageFuelUsage(this List<LapData> laps)
        {
            return laps.GetAverageFuelUsage(laps.Count);
        }

        public static int GetAverageFuelUsage(this List<LapData> laps, int lapAmount)
        {
            lapAmount.ClipMax(laps.Count);
            if (lapAmount < 2)
                return -1;

            int total = 0;
            for (int i = 0; i < lapAmount; i++)
                total += laps[laps.Count - (lapAmount - i)].FuelUsage;

            return total / lapAmount;
        }

        public static int GetAverageLapTime(this List<LapData> laps)
        {
            return laps.GetAverageLapTime(laps.Count);
        }

        public static int GetAverageLapTime(this List<LapData> laps, bool onlyValidLaps)
        {
            return laps.GetAverageLapTime(laps.Count, onlyValidLaps);
        }

        public static int GetAverageLapTime(this List<LapData> laps, int lapAmount)
        {
            return laps.GetAverageLapTime(lapAmount, false);
        }

        public static int GetAverageLapTime(this List<LapData> laps, int lapAmount, bool onlyValidLaps)
        {
            lapAmount.ClipMax(laps.Count);
            if (lapAmount == 0)
                return 0;

            int total = 0;
            int validCount = 0;
            for (int i = 0; i < lapAmount; i++)
            {
                LapData lap = laps[laps.Count - (lapAmount - i)];
                if (onlyValidLaps)
                {
                    if (lap.IsValid)
                    {
                        validCount++;
                        total += lap.Time;
                    }
                }
                else
                    total += lap.Time;
            }

            if (onlyValidLaps)
                return total / validCount;
            else
                return total / lapAmount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sector">1, 2 or 3</param>
        /// <param name="time">laptime as int</param>
        /// <returns>true if the given sector time is faster than any others of that sector in these laps</returns>
        public static bool IsSectorFastest(this List<LapData> laps, int sector, int time)
        {
            List<LapData> data = laps;

            sector.Clip(1, 3);

            switch (sector)
            {
                case 1:
                    foreach (LapData timing in data)
                        if (timing.IsValid && timing.Sector1 < time)
                            return false; break;

                case 2:
                    foreach (LapData timing in data)
                        if (timing.IsValid && timing.Sector2 < time)
                            return false; break;

                case 3:
                    foreach (LapData timing in data)
                        if (timing.IsValid && timing.Sector3 < time)
                            return false; break;

                default: return true;
            }

            return true;
        }

        /// <summary>
        /// Returns the fastest sector time, int.MaxValue if no valid fastest sector available
        /// </summary>
        /// <param name="laps"></param>
        /// <param name="sector"></param>
        /// <returns></returns>
        public static int GetFastestSector(this List<LapData> laps, int sector)
        {
            sector.Clip(1, 3);
            if (laps.Count == 0)
                return int.MaxValue;

            int fastest = int.MaxValue;
            switch (sector)
            {
                case 1:
                    foreach (LapData lap in laps)
                        if (lap.IsValid && (lap.Sector1 < fastest) && lap.Sector1 != -1)
                            fastest = lap.Sector1; break;

                case 2:
                    foreach (LapData lap in laps)
                        if (lap.IsValid && (lap.Sector2 < fastest) && lap.Sector2 != -1)
                            fastest = lap.Sector2; break;

                case 3:
                    foreach (LapData lap in laps)
                        if (lap.IsValid && (lap.Sector3 < fastest) && lap.Sector3 != -1)
                            fastest = lap.Sector3; break;

                default: return fastest;
            }

            return fastest;
        }

        #endregion
    }
}
