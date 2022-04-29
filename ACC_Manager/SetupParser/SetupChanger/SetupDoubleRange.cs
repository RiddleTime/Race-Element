using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.SetupParser.SetupConverter;

namespace ACCSetupApp.SetupParser.SetupRanges
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

            for (double i = doubleRange.Min; i < doubleRange.Max + doubleRange.Increment; i += doubleRange.Increment)
            {
                collection.Add(Math.Round(i, 2));
            }

            return collection.ToArray();
        }
    }
}
