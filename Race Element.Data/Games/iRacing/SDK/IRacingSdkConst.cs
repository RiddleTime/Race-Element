using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games.iRacing.SDK
{
    public class IRacingSdkConst
    {
        public const int MaxNumCars = 64;
        public const int UnlimitedLaps = 32767;
        public const float UnlimitedTime = 604800.0f;
        public static readonly int[] VarTypeBytes = { 1, 1, 4, 4, 4, 8 };
    }
}
