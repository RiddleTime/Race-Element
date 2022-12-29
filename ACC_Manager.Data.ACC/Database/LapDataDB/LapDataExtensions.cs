using RaceElement.Util.SystemExtensions;
using System.Collections.Generic;
using System.Linq;

namespace RaceElement.Data.ACC.Database.LapDataDB
{
    public static class LapDataExtensions
    {
        #region DbLapData Extensions

        public static float GetLapTime(this DbLapData lap)
        {
            return lap.Time / 1000f;
        }

        public static float GetSector1(this DbLapData lap)
        {
            return lap.Sector1 / 1000f;
        }

        public static float GetSector2(this DbLapData lap)
        {
            return lap.Sector2 / 1000f;
        }

        public static float GetSector3(this DbLapData lap)
        {
            return lap.Sector3 / 1000f;
        }

        public static float GetFuelUsage(this DbLapData lap)
        {
            return lap.FuelUsage / 1000f;
        }

        #endregion

        #region Dictionary<int, DbLapData> Extensions

        public static int GetPotentialFastestLapTime(this Dictionary<int, DbLapData> laps)
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

        /// <summary>
        /// Finds the lap index of the fastest lap
        /// </summary>
        /// <returns>-1 if not fastest lap</returns>
        public static int GetFastestLapIndex(this Dictionary<int, DbLapData> laps)
        {
            int fastestTime = int.MaxValue;
            int fastestLapIndex = -1;

            foreach (var key in laps.Keys)
            {
                DbLapData lap = laps[key];
                if (lap.Time < fastestTime && lap.IsValid)
                {
                    fastestTime = lap.Time;
                    fastestLapIndex = lap.Index;
                }

            }

            return fastestLapIndex;
        }

        public static int GetAverageFuelUsage(this Dictionary<int, DbLapData> laps)
        {
            return laps.GetAverageFuelUsage(laps.Count);
        }

        public static int GetAverageFuelUsage(this Dictionary<int, DbLapData> laps, int lapAmount)
        {
            lapAmount.ClipMax(laps.Count);
            if (lapAmount < 2)
                return -1;

            int total = 0;
            foreach (DbLapData lap in laps.Select(x => x.Value).OrderByDescending(x => x.Index).Take(lapAmount))
                total += lap.FuelUsage;

            return total / lapAmount;
        }

        public static int GetAverageLapTime(this Dictionary<int, DbLapData> laps)
        {
            return laps.GetAverageLapTime(laps.Count);
        }

        public static int GetAverageLapTime(this Dictionary<int, DbLapData> laps, bool onlyValidLaps)
        {
            return laps.GetAverageLapTime(laps.Count, onlyValidLaps);
        }

        public static int GetAverageLapTime(this Dictionary<int, DbLapData> laps, int lapAmount)
        {
            return laps.GetAverageLapTime(lapAmount, false);
        }

        public static int GetAverageLapTime(this Dictionary<int, DbLapData> laps, int lapAmount, bool onlyValidLaps)
        {
            lapAmount.ClipMax(laps.Count);
            if (lapAmount == 0)
                return 0;

            int total = 0;
            int validCount = 0;

            foreach (DbLapData lap in laps.Select(x => x.Value))
            {
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
        public static bool IsSectorFastest(this Dictionary<int, DbLapData> laps, int sector, int time)
        {
            Dictionary<int, DbLapData> data = laps;

            sector.Clip(1, 3);

            switch (sector)
            {
                case 1:
                    foreach (DbLapData timing in data.Select(x => x.Value))
                        if (timing.IsValid && timing.Sector1 < time)
                            return false; break;

                case 2:
                    foreach (DbLapData timing in data.Select(x => x.Value))
                        if (timing.IsValid && timing.Sector2 < time)
                            return false; break;

                case 3:
                    foreach (DbLapData timing in data.Select(x => x.Value))
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
        public static int GetFastestSector(this Dictionary<int, DbLapData> laps, int sector)
        {
            sector.Clip(1, 3);
            if (laps.Count == 0)
                return int.MaxValue;

            int fastest = int.MaxValue;
            switch (sector)
            {
                case 1:
                    foreach (DbLapData lap in laps.Select(x => x.Value))
                        if (lap.IsValid && (lap.Sector1 < fastest) && lap.Sector1 != -1)
                            fastest = lap.Sector1; break;

                case 2:
                    foreach (DbLapData lap in laps.Select(x => x.Value))
                        if (lap.IsValid && (lap.Sector2 < fastest) && lap.Sector2 != -1)
                            fastest = lap.Sector2; break;

                case 3:
                    foreach (DbLapData lap in laps.Select(x => x.Value))
                        if (lap.IsValid && (lap.Sector3 < fastest) && lap.Sector3 != -1)
                            fastest = lap.Sector3; break;

                default: return fastest;
            }

            return fastest;
        }

        public static int GetLastLapTime(this Dictionary<int, DbLapData> laps, bool onlyValidLaps = false)
        {
            if (laps.Count == 0) return -1;

            int lastLapIndex = -1;

            foreach (DbLapData lap in laps.Select(x => x.Value))
            {
                if (onlyValidLaps && !lap.IsValid)
                {
                    continue;
                }

                if (lastLapIndex < lap.Index)
                    lastLapIndex = lap.Index;
            }

            return laps[lastLapIndex].Time;
        }

        public static int GetBestLapTime(this Dictionary<int, DbLapData> laps)
        {
            if (laps.Count == 0) return -1;

            int idx = GetFastestLapIndex(laps);
            laps.TryGetValue(idx, out DbLapData fastest);

            if (fastest == null)
                return -1;

            return fastest.Time;
        }

        #endregion
    }
}
