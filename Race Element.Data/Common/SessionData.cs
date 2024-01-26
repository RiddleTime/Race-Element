using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Common
{
    public class SessionData
    {
        public SessionConditions Conditioons { get; set; } = new();
    }

    public class SessionConditions
    {
        public float AirTemperature { get; set; }
        public float AirPressure { get; set; }
        public float AirVelocity { get; set; }
        public float TrackTemperature { get; set; }
    }
}
