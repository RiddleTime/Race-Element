using ACCManager.Broadcast;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.EntryList.TrackPositionGraph
{
    public class Car
    {
        public int CarIndex { get; set; }
        public int LapIndex { get; set; }
        public float SplinePosition { get; set; }
        public CarLocationEnum Location { get; set; }
        public CarLocationEnum PreviousLocation { get; set; }

        public override bool Equals(object obj)
        {
            return this.CarIndex == ((Car)obj).CarIndex;
        }
    }

    public static class CarPositionUpdater
    {
        public static void UpdateLocation(this Car car, float NewSplinePosition, CarLocationEnum newLocation)
        {
            EntryListTracker.CarData carData = EntryListTracker.Instance.Cars[car.CarIndex].Value;
            string currentDriver = carData.CarInfo.Drivers[carData.CarInfo.CurrentDriverIndex].LastName;


            if (newLocation != car.Location)
            {
                if (newLocation == CarLocationEnum.PitEntry && car.Location == CarLocationEnum.Track)
                {
                    car.PreviousLocation = car.Location;
                    car.Location = newLocation;

                    Debug.WriteLine($"{carData.CarInfo} - {currentDriver} has entered the pit entry");
                    // entered pit entry
                }

                if (newLocation == CarLocationEnum.Pitlane && car.Location == CarLocationEnum.PitEntry)
                {
                    car.PreviousLocation = car.Location;
                    car.Location = newLocation;

                    Debug.WriteLine($"{car.CarIndex} - {currentDriver} has been entered the pitlane.");
                }


                if (newLocation == CarLocationEnum.Track && car.Location == CarLocationEnum.PitExit)
                {
                    car.PreviousLocation = car.Location;
                    car.Location = newLocation;

                    Debug.WriteLine($"{car.CarIndex} - {currentDriver} has entered the track");
                }

                if (newLocation == CarLocationEnum.PitExit && car.Location == CarLocationEnum.Pitlane)
                {
                    car.PreviousLocation = car.Location;
                    car.Location = newLocation;

                    Debug.WriteLine($"{car.CarIndex} - {currentDriver} has entered the pit exit");
                }
            }


            if (car.SplinePosition > NewSplinePosition)
            {
                if (newLocation == CarLocationEnum.Track && car.Location == CarLocationEnum.Track)
                {
                    Debug.WriteLine($"{car.CarIndex} - {currentDriver} has gained a lap on track");
                    car.LapIndex++;
                    car.PreviousLocation = car.Location;
                    car.Location = newLocation;
                }

                if (newLocation == CarLocationEnum.Pitlane && car.Location == CarLocationEnum.Pitlane)
                {
                    if (car.LapIndex > 0)
                    {
                        car.LapIndex++;
                        Debug.WriteLine($"{car.CarIndex} - {currentDriver} has gained a lap in the pitlane.");
                    }

                    car.PreviousLocation = car.Location;
                    car.Location = newLocation;
                }
            }

            car.SplinePosition = NewSplinePosition;
        }
    }
}
