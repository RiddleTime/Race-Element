using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.Overlay.Configuration
{
    public class IntRangeAttribute : Attribute
    {
        public int Min;
        public int Max;
        public int Increment;
        public int[] LUT;

        public IntRangeAttribute(int[] LUT)
        {
            this.LUT = LUT;
        }

        public IntRangeAttribute(int min, int max, int increment)
        {
            Min = min;
            Max = max;
            Increment = increment;
        }

        public static int[] GetOptionsCollection(IntRangeAttribute intRange)
        {
            if (intRange.LUT != null)
            {
                return intRange.LUT;
            }

            List<int> collection = new List<int>();

            for (int i = intRange.Min; i < intRange.Max + intRange.Increment; i += intRange.Increment)
            {
                collection.Add(i);
            }

            return collection.ToArray();
        }
    }
}
