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
