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
            EntryListTracker.CarData carData = EntryListTracker.Instance._entryListCars[car.CarIndex];
            string currentDriver = carData.CarInfo.Drivers[carData.CarInfo.CurrentDriverIndex].LastName;


            if (newLocation != car.Location)
            {
                switch (newLocation)
                {
                    case CarLocationEnum.PitEntry:
                        {
                            switch (car.Location)
                            {
                                case CarLocationEnum.Track:
                                    {
                                        car.PreviousLocation = car.Location;
                                        car.Location = newLocation;

                                        Debug.WriteLine($"{carData.CarInfo} - {currentDriver} has entered the pit entry");
                                        break;
                                    }
                            }
                            break;
                        }
                    case CarLocationEnum.Pitlane:
                        {
                            switch (car.Location)
                            {
                                case CarLocationEnum.PitEntry:
                                    {
                                        car.PreviousLocation = car.Location;
                                        car.Location = newLocation;

                                        Debug.WriteLine($"{car.CarIndex} - {currentDriver} has been entered the pitlane.");
                                        break;
                                    }
                            }
                            break;
                        }

                    case CarLocationEnum.PitExit:
                        {
                            switch (car.Location)
                            {
                                case CarLocationEnum.Pitlane:
                                    {
                                        car.PreviousLocation = car.Location;
                                        car.Location = newLocation;

                                        Debug.WriteLine($"{car.CarIndex} - {currentDriver} has entered the pit exit");
                                        break;
                                    }
                            }
                            break;
                        }
                    case CarLocationEnum.Track:
                        {
                            switch (car.Location)
                            {
                                case CarLocationEnum.PitExit:
                                    {
                                        car.PreviousLocation = car.Location;
                                        car.Location = newLocation;

                                        Debug.WriteLine($"{car.CarIndex} - {currentDriver} has entered the track");
                                        break;
                                    }
                            }
                            break;
                        }
                }

                if (newLocation == CarLocationEnum.Track && car.Location == CarLocationEnum.Pitlane)
                {
                    Debug.WriteLine($"{car.CarIndex} - {currentDriver} has started his first lap.");
                    car.PreviousLocation = car.Location;
                    car.Location = newLocation;
                }
            }


            if (car.SplinePosition > NewSplinePosition && car.SplinePosition > 0.99)
            {
                if (newLocation == CarLocationEnum.Track && car.Location == CarLocationEnum.Track)
                {
                    Debug.WriteLine($"{car.CarIndex} - {currentDriver} has gained a lap on track");
                    car.LapIndex++;
                    car.PreviousLocation = car.Location;
                    car.Location = newLocation;
                }

                if (newLocation == CarLocationEnum.Pitlane && car.Location == CarLocationEnum.Pitlane && car.PreviousLocation == CarLocationEnum.PitEntry)
                {
                    if (car.LapIndex > 0)
                    {
                        car.LapIndex++;
                        Debug.WriteLine($"{car.CarIndex} - {currentDriver} has gained a lap in the pitlane.");
                    }

                    car.PreviousLocation = car.Location;
                    car.Location = newLocation;
                }

                if (newLocation == CarLocationEnum.Track && car.Location == CarLocationEnum.Pitlane)
                {

                    car.PreviousLocation = car.Location;
                    car.Location = newLocation;
                }
            }

            car.SplinePosition = NewSplinePosition;
        }
    }
}
