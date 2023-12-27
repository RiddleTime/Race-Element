using System.Collections.Generic;

namespace RaceElement.Data.SetupRanges;

public class SetupIntRange
{
    public int Min;
    public int Max;
    public int Increment;
    public int[] LUT;

    public SetupIntRange(int[] LUT)
    {
        this.LUT = LUT;
    }

    public SetupIntRange(int min, int max, int increment)
    {
        Min = min;
        Max = max;
        Increment = increment;
    }

    public static int[] GetOptionsCollection(SetupIntRange intRange)
    {
        if (intRange.LUT != null)
        {
            return intRange.LUT;
        }

        List<int> collection = new();

        for (int i = intRange.Min; i < intRange.Max + intRange.Increment; i += intRange.Increment)
        {
            collection.Add(i);
        }

        return collection.ToArray();
    }
}
