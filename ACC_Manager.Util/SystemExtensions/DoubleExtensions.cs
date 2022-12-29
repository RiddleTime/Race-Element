namespace RaceElement.Util.SystemExtensions
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// Sets this value or returns it, clipped by min and max (inclusive)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double Clip(ref this double value, double min, double max)
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
        public static double ClipMax(ref this double value, double max)
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
        public static double ClipMin(ref this double value, double min)
        {
            if (value < min) value = min;
            return value;
        }

        public static string ToString(this double[] values, int decimals)
        {
            var value = string.Empty;
            for (int i = 0; i < values.Length; i++)
            {
                double v = values[i];
                value += $"{{{v.ToString($"F{decimals}")}}}";
                if (i < values.Length - 1)
                    value += ", ";
            }
            return value;
        }
    }
}
