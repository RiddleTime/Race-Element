using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.Cars.GT3
{
    internal class Ferrari296GT3 : ICarSetupConversion
    {
        public CarModels CarModel => CarModels.Ferrari_296_GT3_2023;

        CarClasses ICarSetupConversion.CarClass => CarClasses.GT3;
        public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHF2023;

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

            private readonly double[] casters = new double[] {
                8.5, 8.7, 8.9, 9.1, 9.2, 9.4, 9.6, 9.8, 9.9, 10.1, 10.3, 10.5, 10.6,
                10.8, 11.0, 11.2, 11.3, 11.5, 11.7, 11.9, 12.0, 12.2, 12.4, 12.6,
                12.7, 12.9, 13.1, 13.3, 13.4, 13.6, 13.8 };

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
                return rawValue;
            }

            public int AntiRollBarRear(int rawValue)
            {
                return rawValue;
            }

            public double BrakeBias(int rawValue)
            {
                return Math.Round(50.0 + 0.2 * rawValue, 2);
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
                switch (GetPosition(wheel))
                {
                    case Position.Front: return 300 + 100 * rawValue[(int)wheel];
                    case Position.Rear: return 400 + 200 * rawValue[(int)wheel];
                    default: return -1;
                }
            }

            public int PreloadDifferential(int rawValue)
            {
                return 20 + rawValue * 10;
            }

            public double SteeringRatio(int rawValue)
            {
                return Math.Round(13d + rawValue, 2);
            }

            private readonly int[] fronts = new int[] { 163769, 170068, 176367, 182666, 188964, 195263, 201562, 207861, 214160 };
            private readonly int[] rears = new int[] { 122091, 129273, 136455, 143637, 150818, 158000, 165182, 172364, 179546 };

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
                    case Position.Front: return 55 + rawValue[0];
                    case Position.Rear: return 55 + rawValue[2];
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
