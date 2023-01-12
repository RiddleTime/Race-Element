using System;

namespace RaceElement.Hardware.ACC.SteeringLock.Implementations
{
    internal class ThrustmasterSF1000TSPC : ThrustmasterT500 {
        public override string ControllerName => "Thrustmaster SF1000 TS-PC Racer";

        public override bool Test(string productGuid) {
            return string.Equals(productGuid, "B696044F-0000-0000-0000-504944564944", StringComparison.OrdinalIgnoreCase);
        }

        protected override int ProductId => 0xb696;
    }
}