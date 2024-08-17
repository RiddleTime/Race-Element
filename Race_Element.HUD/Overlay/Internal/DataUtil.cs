using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaceElement.Data.Common.SimulatorData;

namespace RaceElement.HUD.Overlay.Internal
{
    internal static class DataUtil
    {

        // Get the grap to the car in front.
        // Restrictions
        //   1) Does not take into account classes
        //   2) Needs the sim to provide the speed of each car (Kmh)
        private static string GetGapToCarInFront(List<KeyValuePair<int, CarInfo>> list, int i)
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
    }
}
