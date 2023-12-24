using System;
using System.Collections.Generic;

namespace RaceElement.HUD.Overlay.Configuration
{
    public class FloatRangeAttribute : Attribute
    {
        public float Min;
        public float Max;
        public float Increment;
        public int Decimals;

        public FloatRangeAttribute(float min, float max, float increment, int decimals)
        {
            Min = min;
            Max = max;
            Increment = increment;
            Decimals = decimals;
        }

        public static float[] GetOptionsCollection(FloatRangeAttribute floatRange)
        {
            List<float> collection = new();

            for (float i = floatRange.Min; i < floatRange.Max + floatRange.Increment; i += floatRange.Increment)
            {
                collection.Add(i);
            }

            return collection.ToArray();
        }
    }
}
