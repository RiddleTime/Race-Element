using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Common
{
    public class CommonSessionData
    {
        public CommonSessionConditions Conditioons { get; set; } = new();
    }

    public class CommonSessionConditions
    {
        public float AirTemperature { get; set; }
        public float AirPressure { get; set; }
        public float AirVelocity { get; set; }
        public float TrackTemperature { get; set; }
    }
}
