using System;
using System.Text;

namespace RaceElement.Util.SystemExtensions;

public static class FloatExtensions
{
    /// <summary>
    /// Sets this value or returns it, clipped by min and max (inclusive)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float Clip(ref this float value, float min, float max)
    {
        if (value < min) value = min;
        if (value > max) value = max;
        return value;
    }

    /// <summary>
    /// Sets this value or returns it, clipped by max (inclusive)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float ClipMax(ref this float value, float max)
    {
        if (value > max) value = max;
        return value;
    }

    /// <summary>
    /// Sets this value or returns it, clipped by min (inclusive)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float ClipMin(ref this float value, float min)
    {
        if (value < min) value = min;
        return value;
    }

    public static float Cubic(float x)
    {
        return x * x * (3.0f - 2.0f * x);
    }
    public static float SmoothStep(float x, float edge0, float edge1)
    {
        // Scale, bias and clamp x to 0..1 range
        x = (x - edge0) / (edge1 - edge0);
        x = Math.Min(1.0f, Math.Max(0.0f, x));
        // Evaluate polynomial
        return Cubic(x);
    }
    public static float Lerp(float weight, float a, float b)
    {
        return a * (1.0f - weight) + b * weight;
    }

    public static string ToString(this float[] values, int decimals)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < values.Length; i++)
        {
            double v = values[i];
            builder.Append($"{{{v.ToString($"F{decimals}")}}}");
            if (i < values.Length - 1)
                builder.Append(", ");
        }
        return builder.ToString();
    }
}
