namespace RaceElement.Data.Common.SimulatorData
{
    public sealed record SessionData
    {
        public WeatherConditions Weather { get; set; } = new();
        public TrackData Track { get; set; } = new();
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
}
