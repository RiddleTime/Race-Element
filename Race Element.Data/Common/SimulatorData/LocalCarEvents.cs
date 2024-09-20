namespace RaceElement.Data.Common.SimulatorData;

/// <summary>
/// Contains a collection of events to listen in on for the local car.
/// </summary>
public sealed class LocalCarEvents
{
    internal LocalCarEvents() { }

    public readonly record struct IntChangeEvent(int Previous, int Next);


    /// <summary>
    /// Kicks off when the lap count increases
    /// </summary>
    public event EventHandler<int>? OnLapGained;
    internal void GainLap(int lapsCompleted) => OnLapGained?.Invoke(this, lapsCompleted);

    /// <summary>
    /// Kicks off when the gear is changed
    /// </summary>
    public event EventHandler<IntChangeEvent>? OnGearChanged;
    internal void GearChanged(IntChangeEvent gearChangedEvent) => OnGearChanged?.Invoke(this, gearChangedEvent);


    /// <summary>
    /// Kicks of when the global position for the local car is changed.
    /// </summary>
    public event EventHandler<IntChangeEvent>? OnGlobalPositionChanged;
    internal void GlobalPositionChanged(IntChangeEvent globalPositionChangedEvent) => OnGlobalPositionChanged?.Invoke(this, globalPositionChangedEvent);
}