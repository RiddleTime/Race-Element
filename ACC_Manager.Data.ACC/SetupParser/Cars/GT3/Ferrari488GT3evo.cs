using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.Data.ConversionFactory;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.Data.Cars.GT3
{
    internal class Ferrari488GT3evo : ICarSetupConversion
    {
        public CarModels CarModel => CarModels.Ferrari_488_GT3_Evo_2020;

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

            private readonly double[] casters = new double[] { 5.0, 5.1, 5.3, 5.5, 5.6,
                5.8, 6.0, 6.1, 6.3, 6.5, 6.6, 6.8, 7.0, 7.1, 7.3, 7.5, 7.6, 7.8,
                8.0, 8.1, 8.3, 8.5, 8.6, 8.8, 9.0, 9.1, 9.3, 9.5, 9.6, 9.8, 9.9,
                10.1, 10.3, 10.4, 10.6, 10.8, 10.9, 11.1, 11.2, 11.4, 11.6, 11.7, 11.9,
                12.1, 12.2, 12.4, 12.5, 12.7, 12.9, 13.0, 13.2, 13.3, 13.5, 13.7, 13.8,
                14.0, 14.1, 14.3, 14.4, 14.6, 14.8, 14.9, 15.1, 15.2, 15.4, 15.5, 15.7,
                15.9, 16.0, 16.2, 16.3, 16.5, 16.6, 16.8, 16.9, 17.1, 17.3, 17.4, 17.6,
                17.7, 17.9, 18.0, 18.2, 18.3, 18.5, 18.6, 18.8, 18.9, 19.1, 19.2, 19.4,
                19.5, 19.7, 19.8, 20.0, 20.1, 20.3, 20.4, 20.6 };
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
                return Math.Round(47.0 + 0.2 * rawValue, 2);
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

            private readonly int[] fronts = new int[] { 94000, 101000, 107000, 113000, 120000, 126000, 138600, 151000, 163800, 176000, 189000 };
            private readonly int[] rears = new int[] { 106000, 113000, 120000, 127000, 134000, 141000, 155000, 169500, 183600, 198000, 212000 };
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
