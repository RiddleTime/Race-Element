using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Overlay.Configuration
{
    public class ByteRangeAttribute : Attribute
    {
        public byte Min;
        public byte Max;
        public byte Increment;

        public ByteRangeAttribute(byte min, byte max, byte increment)
        {
            Min = min;
            Max = max;
            Increment = increment;
        }

        public static byte[] GetOptionsCollection(ByteRangeAttribute intRange)
        {
            List<byte> collection = new List<byte>();

            for (byte i = intRange.Min; i < intRange.Max + intRange.Increment; i += intRange.Increment)
            {
                collection.Add(i);
            }

            return collection.ToArray();
        }
    }
}
