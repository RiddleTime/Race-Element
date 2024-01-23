using System.Collections.Generic;

namespace RaceElement.Broadcast.Structs;

public class LapInfo
{
    public int? LaptimeMS { get; internal set; }
    public List<int?> Splits { get; } = [];
    public ushort CarIndex { get; internal set; }
    public ushort DriverIndex { get; internal set; }
    public bool IsInvalid { get; internal set; }
    public bool IsValidForBest { get; internal set; }
    public LapType Type { get; internal set; }

    /// <summary>
    /// Prefered to use this method instead of LapTimeMS property.
    /// The LapTimeMS Property sometimes doesn't return the correct laptime.
    /// </summary>
    /// <param name="splits"></param>
    /// <returns></returns>
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
