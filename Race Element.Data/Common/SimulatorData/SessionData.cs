namespace RaceElement.Data.Common.SimulatorData
{
    public sealed class SessionData
    {
        public SessionConditions Conditions { get; set; } = new();
    }

    public sealed class SessionConditions
    {
        public float AirTemperature { get; set; }
        public float AirPressure { get; set; }
        public float AirVelocity { get; set; }
        public float TrackTemperature { get; set; }
    }
}
