using System;
using System.Collections.Generic;

namespace RaceElement.Data.SetupRanges
{
    public class SetupDoubleRange
    {
        public double Min;
        public double Max;
        public double Increment;
        public double[] LUT;

        public SetupDoubleRange(double[] LUT)
        {
            this.LUT = LUT;
        }

        public SetupDoubleRange(double min, double max, double increment)
        {
            Min = min;
            Max = max;
            Increment = increment;
        }

        public static double[] GetOptionsCollection(SetupDoubleRange doubleRange)
        {
            if (doubleRange.LUT != null)
            {
                return doubleRange.LUT;
            }

            List<double> collection = new List<double>();

            for (double i = doubleRange.Min; i < doubleRange.Max + Math.Round(doubleRange.Increment, 2); i += Math.Round(doubleRange.Increment, 2))
            {
                i = Math.Round(i, 2);
                collection.Add(Math.Round(i, 2));
            }

            return collection.ToArray();
        }
    }
}
