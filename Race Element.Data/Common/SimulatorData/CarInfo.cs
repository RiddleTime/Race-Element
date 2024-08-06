using System.Collections.Generic;

namespace RaceElement.Data.Common.SimulatorData;

public sealed class CarInfo
{
    public ushort CarIndex { get; }
    // TODO: define car model type values
    public byte CarModelType { get; protected internal set; }
    public string TeamName { get; protected internal set; }
    public int RaceNumber { get; protected internal set; }
    public byte CupCategory { get; protected internal set; }
    public int CurrentDriverIndex { get; protected internal set; }
    public IList<DriverInfo> Drivers { get; } = new List<DriverInfo>();

    // TODO: what do SplinePosition and Position mean?
    public float SplinePosition { get; protected internal set; }
    public float Position { get; protected internal set; }

    public CarLocationEnum CarLocation { get; set; }
    public int Kmh { get; set; }
    public int Laps { get; set; }

    // TODO: add other info from RealtimeCarUpdate/LapInfo as needed

    public CarInfo(ushort carIndex)
    {
        CarIndex = carIndex;
    }

    internal void AddDriver(DriverInfo driverInfo)
    {
        Drivers.Add(driverInfo);
    }

    public string GetCurrentDriverName()
    {
        if (CurrentDriverIndex < Drivers.Count)
            return Drivers[CurrentDriverIndex].LastName;
        return "nobody(?)";
    }

    public enum CarLocationEnum
{
    NONE = 0,
    Track = 1,
    Pitlane = 2,
    PitEntry = 3,
    PitExit = 4
}


}
