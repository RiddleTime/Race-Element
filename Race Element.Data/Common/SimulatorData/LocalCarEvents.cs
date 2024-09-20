namespace RaceElement.Data.Common.SimulatorData;

/// <summary>
/// Contains a collection of events to listen in on for the local car.
/// </summary>
public sealed class LocalCarEvents
{
    internal LocalCarEvents() { }

    /// <summary>
    /// Kicks off when the lap count increases
    /// </summary>
    public event EventHandler<int>? OnLapGained;
    internal void GainLap(int lapsCompleted) => OnLapGained?.Invoke(this, lapsCompleted);

    /// <summary>
    /// Kicks off when the gear is changed
    /// </summary>
    public event EventHandler<GearChangeEvent>? OnGearChanged;
    public readonly record struct GearChangeEvent(int Previous, int Next);
    internal void GearChanged(GearChangeEvent gearChangedEvent) => OnGearChanged?.Invoke(this, gearChangedEvent);
}