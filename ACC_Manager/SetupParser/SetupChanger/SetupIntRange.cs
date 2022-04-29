using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.SetupParser.SetupChanger
{
    public class SetupIntRange
    {
        public int Min;
        public int Max;
        public int Increment;
        public int[] LUT;

        public SetupIntRange(int[] LUT)
        {
            this.LUT = LUT;
        }

        public SetupIntRange(int min, int max, int increment)
        {
            Min = min;
            Max = max;
            Increment = increment;
        }
    }
}
