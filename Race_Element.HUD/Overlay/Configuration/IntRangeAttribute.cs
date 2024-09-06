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

    /// <summary>
    /// Used to override default Min value for a specific game.
    /// /// Uses <see cref="MinGames"/> for indexing.
    /// </summary>
    public int[] GameMins = [];
    /// <summary>
    /// Set the min value to override for a specific game.
    /// Uses <see cref="GameMins"/> for indexing.
    /// </summary>
    public Game[] MinGames = [];

    /// <summary>
    /// Used to override default Max value for a specific game.
    /// /// Uses <see cref="MaxGames"/> for indexing.
    /// </summary>
    public int[] GameMaxs = [];
    /// <summary>
    /// Set the max value to override for a specific game.
    /// Uses <see cref="GameMaxs"/> for indexing.
    /// </summary>
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

        if (GameMins.Length > 0 && MinGames.Length == GameMins.Length)
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

        if (GameMaxs.Length > 0 && MaxGames.Length == GameMaxs.Length)
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
