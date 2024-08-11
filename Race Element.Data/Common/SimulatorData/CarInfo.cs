using System.Collections.Generic;

namespace RaceElement.Data.Common.SimulatorData;

public sealed class CarInfo
{
    public uint CarIndex { get; }
    
    public string TeamName { get; protected internal set; }
    public int RaceNumber { get; set; }
    public byte CupCategory { get; protected internal set; }
    public int CurrentDriverIndex { get; set; }
    public IList<DriverInfo> Drivers { get; } = new List<DriverInfo>();

    // Percentage 0.0f (0%) to 1.0f to (100%) of track completed. Might not read full 0% and 100% at start and end of lap and is therefore rounded in that case.
    public float TrackPercentCompleted { get; set; }
    public float Position { get; set; }

    public CarLocationEnum CarLocation { get; set; }
    // Speed of all player's cars. Not all sims provide this
    public int Kmh { get; set; }
    public int Laps { get; set; }
    public int CupPosition { get; set; }
    public LapInfo LastLap { get; set; }
    public LapInfo CurrentLap { get; set; }
    
    // Gap to class leader
    public float GapToClassLeaderMs { get; set; }
    // Gap from player's car to others regardless of class
    public float GapToPlayerMs { get; set; }

    // TODO : is this the lap number we're in? 
    public int LapIndex{ get; set; }
    public string CarClass { get; set; }
    public bool IsSpectator { get; set; }


    public CarInfo(uint carIndex)
    {
        CarIndex = carIndex;
    }

    public void AddDriver(DriverInfo driverInfo)
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
