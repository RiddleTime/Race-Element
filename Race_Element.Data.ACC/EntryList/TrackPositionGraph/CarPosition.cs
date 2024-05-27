using RaceElement.Broadcast;
using RaceElement.Util.SystemExtensions;
using System.Diagnostics;

namespace RaceElement.Data.ACC.EntryList.TrackPositionGraph;

public sealed class Car
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
    public static void UpdateLocation(this Car car, float newSplinePosition, CarLocationEnum newLocation)
    {
        EntryListTracker.CarData carData = EntryListTracker.Instance._entryListCars[car.CarIndex];
        if (carData.CarInfo == null)
            return;
        string currentDriver = carData.CarInfo.Drivers[carData.CarInfo.CurrentDriverIndex].LastName;


        string carInfo = $"#{carData.CarInfo.RaceNumber}".FillEnd(5, ' ');

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

                                    break;
                                }

                            case CarLocationEnum.NONE:
                                {
                                    car.PreviousLocation = car.Location;
                                    car.Location = newLocation;

                                    Debug.WriteLine($"{carInfo} has come into the pit entry on the formation lap");
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

                                    break;
                                }
                            case CarLocationEnum.PitExit:
                                {
                                    car.PreviousLocation = car.Location;
                                    car.Location = newLocation;

                                    break;
                                }
                            case CarLocationEnum.Track:
                                {
                                    car.PreviousLocation = car.Location;
                                    car.Location = newLocation;

                                    break;
                                }
                            case CarLocationEnum.NONE:
                                {
                                    Debug.WriteLine($"{carInfo} has joined the server");
                                    car.PreviousLocation = car.Location;
                                    car.Location = newLocation;

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

                                    break;
                                }
                        }
                        break;
                    }
                case CarLocationEnum.Track:
                    {
                        switch (car.Location)
                        {
                            case CarLocationEnum.PitEntry:
                                {
                                    car.PreviousLocation = car.Location;
                                    car.Location = newLocation;

                                    //Debug.WriteLine($"{carInfo} has cancelled his pit entry");
                                    break;
                                }
                            case CarLocationEnum.PitExit:
                                {
                                    car.PreviousLocation = car.Location;
                                    car.Location = newLocation;

                                    break;
                                }

                            case CarLocationEnum.NONE:
                                {
                                    car.PreviousLocation = car.Location;
                                    car.Location = newLocation;

                                    break;
                                }
                        }
                        break;
                    }
            }

            if (newLocation == CarLocationEnum.Track && car.Location == CarLocationEnum.Pitlane)
            {

                car.PreviousLocation = car.Location;
                car.Location = newLocation;
            }

            if (car.PreviousLocation != car.Location)
                Debug.WriteLine($"{carInfo} | lap:{car.LapIndex} | {car.PreviousLocation}->{car.Location}");
        }


        if (car.SplinePosition > newSplinePosition && car.SplinePosition > 0.99)
        {
            if (newLocation == CarLocationEnum.Track && car.Location == CarLocationEnum.Track)
            {
                Debug.WriteLine($"{carInfo} has gained a lap on track");
                car.LapIndex++;
                car.PreviousLocation = car.Location;
                car.Location = newLocation;
            }

            if (newLocation == CarLocationEnum.Pitlane && car.Location == CarLocationEnum.Pitlane && car.PreviousLocation == CarLocationEnum.PitEntry)
            {
                if (car.LapIndex > 0)
                {
                    car.LapIndex++;
                    Debug.WriteLine($"{carInfo} has gained a lap in the pitlane.");
                }

                car.PreviousLocation = car.Location;
                car.Location = newLocation;
            }

            if (newLocation == CarLocationEnum.Track && car.Location == CarLocationEnum.NONE)
            {
                car.PreviousLocation = car.Location;
                car.Location = newLocation;
            }
        }

        car.SplinePosition = newSplinePosition;
    }
}
