using RaceElement.Data.ACC.Session;
using System.Collections.Generic;
using System.Diagnostics;

namespace RaceElement.Data.ACC.EntryList.TrackPositionGraph
{
    public class PositionGraph
    {
        private Dictionary<int, Car> _cars = new();

        private static PositionGraph _instance;
        public static PositionGraph Instance
        {
            get
            {
                if (_instance == null) _instance = new PositionGraph();
                return _instance;
            }
        }

        private PositionGraph()
        {
            RaceSessionTracker.Instance.OnACSessionTypeChanged += (s, e) => ResetData();
        }

        public void AddCar(int carIndex)
        {
            Car newCar = new()
            {
                CarIndex = carIndex,
                Location = Broadcast.CarLocationEnum.NONE,
                LapIndex = 0,
                SplinePosition = 0,
                PreviousLocation = Broadcast.CarLocationEnum.NONE,
            };

            if (!_cars.TryGetValue(carIndex, out _))
                _cars.Add(carIndex, newCar);
        }

        public Car GetCar(int carIndex)
        {
            if (_cars.TryGetValue(carIndex, out Car car))
            {
                return car;
            }

            return null;
        }

        public void RemoveCar(int carIndex)
        {
            if (_cars.ContainsKey(carIndex))
                _cars.Remove(carIndex);
        }

        public void ResetData()
        {
            Debug.WriteLine("Reset Position Graph");
            //_cars.Clear();

            foreach (Car car in _cars.Values)
            {
                _cars[car.CarIndex].Location = Broadcast.CarLocationEnum.NONE;
                _cars[car.CarIndex].LapIndex = 0;
                _cars[car.CarIndex].SplinePosition = 0;
                _cars[car.CarIndex].PreviousLocation = Broadcast.CarLocationEnum.NONE;
            }
        }
    }
}
