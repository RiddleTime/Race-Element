using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACC_Manager.Util.NumberExtensions
{
    public static class IntegerExtensions
    {
        /// <summary>
        /// Sets this value or returns it, clipped by min and max (inclusive)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Clip(ref this int value, int min, int max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }
    }
}
