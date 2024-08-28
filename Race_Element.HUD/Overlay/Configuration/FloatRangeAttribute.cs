using RaceElement.Data.Games;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaceElement.HUD.Overlay.Configuration;

[AttributeUsage(AttributeTargets.Property)]
public sealed class FloatRangeAttribute : Attribute
{
    public float Min;
    public float Max;
    public float Increment;
    public int Decimals;

    /// <summary>
    /// Used to override default Min value for a specific game.
    /// /// Uses <see cref="MinGames"/> for indexing.
    /// </summary>
    public float[] GameMins = [];
    /// <summary>
    /// Set the min value to override for a specific game.
    /// Uses <see cref="GameMins"/> for indexing.
    /// </summary>
    public Game[] MinGames = [];

    /// <summary>
    /// Used to override default Max value for a specific game.
    /// /// Uses <see cref="MaxGames"/> for indexing.
    /// </summary>
    public float[] GameMaxs = [];
    /// <summary>
    /// Set the max value to override for a specific game.
    /// Uses <see cref="GameMaxs"/> for indexing.
    /// </summary>
    public Game[] MaxGames = [];

    public FloatRangeAttribute() { }

    public FloatRangeAttribute(float min, float max, float increment, int decimals)
    {
        Min = min;
        Max = max;
        Increment = increment;
        Decimals = decimals;
    }

    public float GetMin(Game currentGame)
    {
        float min = Min;

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

    public float GetMax(Game currentGame)
    {
        float max = Max;

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

    public static float[] GetOptionsCollection(FloatRangeAttribute floatRange)
    {
        List<float> collection = [];

        for (float i = floatRange.Min; i < floatRange.Max + floatRange.Increment; i += floatRange.Increment)
        {
            collection.Add(i);
        }

        return collection.ToArray();
    }
}
