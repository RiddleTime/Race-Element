using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.Cars.GTC
{
    internal class LamborghiniHuracanST_2015 : ICarSetupConversion
    {
        public CarModels CarModel => CarModels.Lamborghini_Huracan_ST_2015;

        CarClasses ICarSetupConversion.CarClass => CarClasses.ST; // did this for tyre pressures, should be 'GTC'
        public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHF2023;

        AbstractTyresSetup ICarSetupConversion.TyresSetup => new TyreSetup();
        private class TyreSetup : AbstractTyresSetup
        {
            public override double Camber(Wheel wheel, List<int> rawValue)
            {
                switch (GetPosition(wheel))
                {
                    case Position.Front: return Math.Round(-5 + 0.1 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(-5 + 0.1 * rawValue[(int)wheel], 2);
                    default: return -1;
                }
            }

            private readonly double[] casters = new double[] { 6.2, 6.5, 6.7, 7.0, 7.3, 7.5, 7.8, 8.1,
                8.3, 8.6, 8.9, 9.1, 9.4, 9.6, 9.9, 10.2, 10.4, 10.7, 10.9, 11.2, 11.5, 11.7, 12.0, 12.2,
                12.5, 12.8, 13.0, 13.3, 13.5, 13.8, 14.0, 14.3, 14.5, 14.8, 15.0 };
            public override double Caster(int rawValue)
            {
                return Math.Round(casters[rawValue], 2);
            }

            public override double Toe(Wheel wheel, List<int> rawValue)
            {

                switch (GetPosition(wheel))
                {
                    case Position.Front: return Math.Round(-0.4 + 0.01 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(-0.4 + 0.01 * rawValue[(int)wheel], 2);
                    default: return -1;
                }

            }
        }

        IMechanicalSetup ICarSetupConversion.MechanicalSetup => new MechSetup();
        private class MechSetup : IMechanicalSetup
        {
            public int AntiRollBarFront(int rawValue)
            {
                return rawValue;
            }

            public int AntiRollBarRear(int rawValue)
            {
                return rawValue;
            }

            public double BrakeBias(int rawValue)
            {
                return Math.Round(50 + 0.2 * rawValue, 2);
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

            private readonly int[] fronts = new int[] { 121000, 144000, 167000, 190000, 201000, 212000 };
            private readonly int[] rears = new int[] { 117000, 136000, 154000, 164000, 173000, 191000 };
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
                return rawValue;
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
