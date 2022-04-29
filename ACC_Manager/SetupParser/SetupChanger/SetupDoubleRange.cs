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
    }
}
