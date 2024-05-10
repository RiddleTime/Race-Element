using System.Collections.Generic;

namespace RaceElement.Broadcast.Structs;

public class LapInfo
{
    /// <summary>
    /// Lap time in milliseconds. Use "GetLapTimeMS()"
    /// as this value sometime is reported wrong by
    /// the server.
    /// </summary>
    public int? LaptimeMS { get; internal set; }

    /// <summary>
    /// Sector time in milliseconds.
    /// </summary>
    public List<int?> Splits { get; } = [];

    /// <summary>
    /// Internal server identifier assigned to the car.
    /// </summary>
    public ushort CarIndex { get; internal set; }

    /// <summary>
    ///  TODO
    /// </summary>
    public ushort DriverIndex { get; internal set; }

    /// <summary>
    /// Lap is not valid due corner cut, out of track, etc.
    /// </summary>
    public bool IsInvalid { get; internal set; }

    /// <summary>
    /// Improving its own the best time.
    /// </summary>
    public bool IsValidForBest { get; internal set; }

    /// <summary>
    /// TODO
    /// </summary>
    public LapType Type { get; internal set; }

    /// <summary>
    /// Preferred to use this method instead of LapTimeMS property.
    /// The LapTimeMS Property sometimes doesn't return the correct
    /// laptime.
    /// </summary>
    public int GetLapTimeMS()
    {
        int lapTimeMs = 0;
        for (int i = 0; i < Splits.Count; i++)
        {
            lapTimeMs += Splits[i].GetValueOrDefault();
        }

        return lapTimeMs;
    }

    public override string ToString()
    {
        return $"{LaptimeMS,5}|{string.Join("|", Splits)}";
    }
}
