namespace RaceElement.Data.Common.SimulatorData
{
    public sealed class SessionData
    {
        public WeatherConditions Weather { get; set; } = new();
        public TrackConditions Track { get; set; } = new();
    }

    public class TrackConditions
    {
        /// <summary>
        /// The track temperature in celsius
        /// </summary>
        public float TrackTemperature { get; set; }
    }

    public sealed class WeatherConditions
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
}
