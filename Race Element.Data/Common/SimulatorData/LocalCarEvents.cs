namespace RaceElement.Data.Common.SimulatorData;

/// <summary>
/// Contains a collection of events to listen in on for the local car.
/// </summary>
public sealed class LocalCarEvents
{
    /// <summary>
    /// See <see cref="LocalCarData.CarModel"/> for live data.
    /// </summary>
    public readonly CarModelEvents CarModel = new();

    /// <summary>
    /// See <see cref="LocalCarData.Inputs"/> for live data.
    /// </summary>
    public readonly InputEvents Inputs = new();

    /// <summary>
    /// See <see cref="LocalCarData.Race"/> for live data.
    /// </summary>
    public readonly RaceEvents Race = new();

    internal LocalCarEvents() { }

    public readonly record struct IntChangeEvent(int Previous, int Next);
    public readonly record struct StringChangeEvent(string Previous, string Next);

    /// <see cref="LocalCarData.CarModel"/>
    #region InputsData
    public sealed class CarModelEvents
    {
        internal CarModelEvents() { }

        /// <summary>
        /// Kicks off when the Game Name of the car model is changed
        /// </summary>
        public event EventHandler<StringChangeEvent>? OnGameNameChanged;
        internal void GameNameChanged(StringChangeEvent gameNameChange) => OnGameNameChanged?.Invoke(this, gameNameChange);
    }

    /// <see cref="LocalCarData.Inputs"/>
    #region InputsData
    public sealed class InputEvents
    {
        internal InputEvents() { }
        /// <summary>
        /// Kicks off when the gear is changed
        /// </summary>
        public event EventHandler<IntChangeEvent>? OnGearChanged;
        internal void GearChanged(IntChangeEvent gearChangedEvent) => OnGearChanged?.Invoke(this, gearChangedEvent);

    }
    #endregion

    public sealed class RaceEvents
    {
        internal RaceEvents() { }

        /// <summary>
        /// Kicks off when the lap count increases
        /// </summary>
        public event EventHandler<int>? OnLapGained;
        internal void GainLap(int lapsCompleted) => OnLapGained?.Invoke(this, lapsCompleted);

        /// <summary>
        /// Kicks of when the global position for the local car is changed.
        /// </summary>
        public event EventHandler<IntChangeEvent>? OnGlobalPositionChanged;
        internal void GlobalPositionChanged(IntChangeEvent globalPositionChangedEvent) => OnGlobalPositionChanged?.Invoke(this, globalPositionChangedEvent);

        /// <summary>
        /// Kicks of when the global position for the local car is changed.
        /// </summary>
        public event EventHandler<IntChangeEvent>? OnClassPositionChanged;
        internal void ClassPositionChanged(IntChangeEvent classPositionChangedEvent) => OnClassPositionChanged?.Invoke(this, classPositionChangedEvent);
    }
}
