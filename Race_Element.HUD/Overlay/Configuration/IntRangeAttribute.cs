using System;
using System.Collections.Generic;

namespace RaceElement.HUD.Overlay.Configuration;

[AttributeUsage(AttributeTargets.Property)]
public sealed class IntRangeAttribute : Attribute
{
    public int Min;
    public int Max;
    public int Increment;

    public IntRangeAttribute(int min, int max, int increment)
    {
        Min = min;
        Max = max;
        Increment = increment;
    }

    public static int[] GetOptionsCollection(IntRangeAttribute intRange)
    {
        List<int> collection = [];

        for (int i = intRange.Min; i < intRange.Max + intRange.Increment; i += intRange.Increment)
        {
            collection.Add(i);
        }

        return collection.ToArray();
    }
}
