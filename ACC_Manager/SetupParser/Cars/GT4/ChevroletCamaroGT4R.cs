using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.SetupParser.SetupConverter;

namespace ACCSetupApp.SetupParser.Cars.GT4
{

    // TODO (remove this comment when done), all the data in here is not correct.
    internal class ChevroletCamaroGT4R : ICarSetupConversion
    {
        public string CarName => "Chevrolet Camaro GT4 R 2017";

        public string ParseName => "chevrolet_camaro_gt4r";

        CarClasses ICarSetupConversion.CarClass => CarClasses.GT4;

        AbstractTyresSetup ICarSetupConversion.TyresSetup => new TyreSetup();
        private class TyreSetup : AbstractTyresSetup
        {
            public override double Camber(Wheel wheel, List<int> rawValue)
            {
                switch (GetPosition(wheel))
                {
                    case Position.Front: return Math.Round(-5.0 + 0.1 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(-3.5 + 0.1 * rawValue[(int)wheel], 2);
                    default: return -1;
                }
            }

            private readonly double[] casters = new double[] {
                7.1
            };
            public override double Caster(int rawValue)
            {
                return Math.Round(casters[rawValue], 2);
            }

            public override double Toe(Wheel wheel, List<int> rawValue)
            {
                switch (GetPosition(wheel))
                {
                    case Position.Front: return Math.Round(-2.0 + 0.1 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(0.0 + 0.1 * rawValue[(int)wheel], 2);
                    default: return -1;
                }
            }
        }

        IMechanicalSetup ICarSetupConversion.MechanicalSetup => new MechSetup();
        private class MechSetup : IMechanicalSetup
        {
            public int AntiRollBarFront(int rawValue)
            {
                return 0 + rawValue;
            }

            public int AntiRollBarRear(int rawValue)
            {
                return 0 + rawValue;
            }

            public double BrakeBias(int rawValue)
            {
                return Math.Round(47 + 0.2 * rawValue, 2);
            }

            public int BrakePower(int rawValue)
            {
                return 80 + rawValue;
            }

            public int BumpstopRange(List<int> rawValue, Wheel wheel)
            {
                return rawValue[(int)wheel];
            }

            public int BumpstopRate(List<int> rawValue, Wheel wheel)
            {
                return 300 + 100 * rawValue[(int)wheel];
            }

            public int PreloadDifferential(int rawValue)
            {
                return 20 + rawValue * 10;
            }

            public double SteeringRatio(int rawValue)
            {
                return Math.Round(10d + rawValue, 2);
            }

            private readonly int[] fronts = new int[] { 165888, 184320, 202752 };
            private readonly int[] rears = new int[] { 90000, 102000, 114000 };
            public int WheelRate(List<int> rawValue, Wheel wheel)
            {
                switch (GetPosition(wheel))
                {
                    case Position.Front: return fronts[rawValue[(int)wheel]];
                    case Position.Rear: return rears[rawValue[(int)wheel]];
                    default: return -1;
                }
            }
        }

        IDamperSetup ICarSetupConversion.DamperSetup => DefaultDamperSetup;

        IAeroBalance ICarSetupConversion.AeroBalance => new AeroSetup();
        private class AeroSetup : IAeroBalance
        {
            public int BrakeDucts(int rawValue)
            {
                return rawValue;
            }

            public int RearWing(int rawValue)
            {
                return rawValue + 1;
            }

            public int RideHeight(List<int> rawValue, Position position)
            {
                switch (position)
                {
                    case Position.Front: return 115 + rawValue[0];
                    case Position.Rear: return 123 + rawValue[2];
                    default: return -1;
                }
            }

            public int Splitter(int rawValue)
            {
                return rawValue;
            }
        }
    }
}
