using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaceElement.Data.Common.SimulatorData;

namespace RaceElement.HUD.Overlay.Internal
{
    public static class DataUtil
    {

        // Get the grap to the car in front.
        // Restrictions
        //   1) Does not take into account classes
        //   2) Needs the sim to provide the speed of each car (Kmh)
        public static string GetGapToCarInFront(List<KeyValuePair<int, CarInfo>> list, int i)
        {
            // inspired by acc bradcasting client

            if (i < 1 || SessionData.Instance.Track.Length == 0) return "---";

            var carInFront = list[i - 1].Value;
            var currentCar = list[i].Value;
            var splineDistance = Math.Abs(carInFront.TrackPercentCompleted - currentCar.TrackPercentCompleted);
            while (splineDistance > 1f)
                splineDistance -= 1f;
            var gabMeters = splineDistance * SessionData.Instance.Track.Length;

            if (currentCar.Kmh < 10)
            {
                return "---";
            }
            return $"{gabMeters / currentCar.Kmh * 3.6:F1}s ⇅";

        }

        /// <summary>
        /// Format a lap time difference in milliseconds
        /// </summary>        
        public static String GetTimeDiff(int? LaptimeMS)
        {
            if (LaptimeMS == null || LaptimeMS == 0)
            {
                return "--:--.---";
            }

            TimeSpan lapTime = TimeSpan.FromMilliseconds((double)LaptimeMS);
            if (lapTime.Minutes > 0)
                return $"{lapTime:m\\:s\\.f}";
            else
                return $"{lapTime:s\\.f}";
        }

        /// <summary>
        /// Format a lap time
        /// </summary>
        public static String GetLapTime(LapInfo lap)
        {
            if (lap == null || !lap.LaptimeMS.HasValue || lap.LaptimeMS < 0)
            {
                return "--:--.---";
            }

            TimeSpan lapTime = TimeSpan.FromMilliseconds(lap.LaptimeMS.Value);
            return $"{lapTime:mm\\:ss\\.fff}";
        }
    }
}
