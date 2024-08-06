using System.Collections.Concurrent;

namespace RaceElement.Data.Common.SimulatorData
{
    public sealed record SessionData
    {
        public WeatherConditions Weather { get; set; } = new();
        public TrackData Track { get; set; } = new();

        internal readonly ConcurrentDictionary<int, CarInfo> _entryListCars = [];
        public List<KeyValuePair<int, CarInfo>> Cars
        {
            get
            {
                return [.. _entryListCars];
            }
        }


        // TODO: for now only for testing
        public void AddCar(int carIndex, CarInfo car) {
            _entryListCars.TryAdd(carIndex, car);
        }

        private static SessionData _instance;
        public static SessionData Instance
        {
            get
            {
                _instance ??= new SessionData();
                return _instance;
            }
        }

        public int FocusedCarIndex {get; set;}
        public RaceSessionType SessionType { get; set; }
    }

    public sealed record TrackData
    {
        /// <summary>
        /// The track name based on the way the game provides it
        /// </summary>
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// The track temperature in celsius
        /// </summary>
        public float Temperature { get; set; }
    }

    public sealed record WeatherConditions
    {
        /// <summary>
        /// The air temperature in celsius
        /// </summary>
        public float AirTemperature { get; set; }
        public float AirPressure { get; set; }

        /// <summary>
        /// The speed of the air in km/h
        /// </summary>
        public float AirVelocity { get; set; }

        /// <summary>
        /// The direction of the air in degrees
        /// </summary>
        public float AirDirection { get; set; }
    }

    public enum RaceSessionType
{
    Practice = 0,
    Qualifying = 4,
    Superpole = 9,
    Race = 10,
    Hotlap = 11,
    Hotstint = 12,
    HotlapSuperpole = 13,
    Replay = 14
};
}
