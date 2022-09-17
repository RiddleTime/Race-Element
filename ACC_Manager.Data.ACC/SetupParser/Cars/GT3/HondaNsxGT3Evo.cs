using System;
using System.Collections.Generic;
using static ACCManager.Data.ConversionFactory;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.Data.Cars.GT3
{
    internal class HondaNsxGT3Evo : ICarSetupConversion
    {
        public CarModels CarModel => CarModels.Honda_NSX_GT3_Evo_2019;

        CarClasses ICarSetupConversion.CarClass => CarClasses.GT3;
        public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHE2020;

        AbstractTyresSetup ICarSetupConversion.TyresSetup => new TyreSetup();
        private class TyreSetup : AbstractTyresSetup
        {
            public override double Camber(Wheel wheel, List<int> rawValue)
            {
                switch (GetPosition(wheel))
                {
                    case Position.Front: return Math.Round(-4 + 0.1 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(-3.5 + 0.1 * rawValue[(int)wheel], 2);
                    default: return -1;
                }
            }

            private readonly double[] casters = new double[] { 7.2, 7.4, 7.6, 7.8, 8.0, 8.2, 8.4, 8.6, 8.8, 8.9, 9.1 };
            public override double Caster(int rawValue)
            {
                return Math.Round(casters[rawValue], 2);
            }

            public override double Toe(Wheel wheel, List<int> rawValue)
            {
                return Math.Round(-0.4 + 0.01 * rawValue[(int)wheel], 2);
            }
        }

        IMechanicalSetup ICarSetupConversion.MechanicalSetup => new MechSetup();
        private class MechSetup : IMechanicalSetup
        {
            public int AntiRollBarFront(int rawValue)
            {
                return 1 + rawValue;
            }

            public int AntiRollBarRear(int rawValue)
            {
                return 1 + rawValue;
            }

            public double BrakeBias(int rawValue)
            {
                return Math.Round(44 + 0.2 * rawValue, 2);
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
                return 200 + 50 * rawValue[(int)wheel];
            }

            public int PreloadDifferential(int rawValue)
            {
                return 20 + rawValue * 10;
            }

            public double SteeringRatio(int rawValue)
            {
                return Math.Round(10d + rawValue, 2);
            }


            private readonly int[] fronts = new int[] { 73000, 79080, 85160, 91240,
                97320, 103400, 109480, 115560, 121640, 127720, 133800, 139880, 145960,
                152040, 158120, 164200, 170280 };
            private readonly int[] rears = new int[] { 126800, 134700, 142600, 150500,
                158400, 166300, 174200, 182100, 190000, 197900, 205800, 213700,
                221600, 229500, 237400, 245300, 253200 };
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
                    case Position.Front: return 54 + rawValue[0];
                    case Position.Rear: return 54 + rawValue[2];
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
