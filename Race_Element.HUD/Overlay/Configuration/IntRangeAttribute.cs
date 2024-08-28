using RaceElement.Data.Games;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaceElement.HUD.Overlay.Configuration;

[AttributeUsage(AttributeTargets.Property)]
public sealed class IntRangeAttribute : Attribute
{
    public int Min;
    public int Max;
    public int Increment;

    public int[] GameMins = [];
    public Game[] MinGames = [];

    public int[] GameMaxs = [];
    public Game[] MaxGames = [];

    public IntRangeAttribute()
    {
    }
    public IntRangeAttribute(int min, int max, int increment)
    {
        Min = min;
        Max = max;
        Increment = increment;
    }

    public int GetMin(Game currentGame)
    {
        int min = Min;

        if (GameMins.Length > 0 && MinGames.Length > 0)
        {
            if (MinGames.Contains(currentGame))
            {
                int index = MinGames.ToList().IndexOf(currentGame);
                return GameMins[index];
            }
        }

        return min;
    }

    public int GetMax(Game currentGame)
    {
        int max = Max;

        if (GameMaxs.Length > 0 && MaxGames.Length > 0)
        {
            if (MaxGames.Contains(currentGame))
            {
                int index = MaxGames.ToList().IndexOf(currentGame);
                return GameMaxs[index];
            }
        }

        return max;
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
