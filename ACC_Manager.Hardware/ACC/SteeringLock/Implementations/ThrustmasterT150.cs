using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Hardware.ACC.SteeringLock.Implementations
{
    internal class ThrustmasterT150 : ThrustmasterT500
    {
        public override string ControllerName => "Thrustmaster T150RS";

        public override bool Test(string productGuid)
        {
            return string.Equals(productGuid, "B667044F-0000-0000-0000-504944564944", StringComparison.OrdinalIgnoreCase);
        }

        protected override int ProductId => 0xb667;
    }
}
