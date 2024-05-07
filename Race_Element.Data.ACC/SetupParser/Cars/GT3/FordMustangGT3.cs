using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.ACC.SetupParser.Cars.GT3
{
    internal sealed class FordMustangGT3 : ICarSetupConversion
    {
        public CarModels CarModel => CarModels.Ford_Mustang_GT3_2024;

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

            private readonly double[] casters = [5.3,
5.5, 5.7, 5.9, 6.1, 6.2, 6.4, 6.6, 6.8, 6.9, 7.1, 7.3, 7.5, 7.6,
7.8, 8.0, 8.2, 8.3, 8.5, 8.7, 8.9, 9.0, 9.2, 9.4, 9.6, 9.7, 9.9, 10.1, 10.3, 10.4, 10.6,
10.8,10.9,11.1,11.3,11.5,11.6,11.8,12.0,12.1,12.3];
            public override double Caster(int rawValue)
            {
                return Math.Round(casters[rawValue], 2);
            }

            public override double Toe(Wheel wheel, List<int> rawValue)
            {
                return GetPosition(wheel) switch
                {
                    Position.Front => Math.Round(-0.2 + 0.01 * rawValue[(int)wheel], 2),
                    Position.Rear => Math.Round(-0.1 + 0.01 * rawValue[(int)wheel], 2),
                    _ => 0
                };
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
                return Math.Round(48.5 + 0.3 * rawValue, 2);
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

            private readonly int[] fronts = [105000, 120000, 135000, 150000, 165000, 180000];
            private readonly int[] rears = [90000, 105000, 120000, 135000, 150000, 165000];
            public int WheelRate(List<int> rawValue, Wheel wheel)
            {
                return GetPosition(wheel) switch
                {
                    Position.Front => fronts[rawValue[(int)wheel]],
                    Position.Rear => rears[rawValue[(int)wheel]],
                    _ => -1,
                };
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

            public int RideHeight(List<int> rawValue, Position position, int trackTypeBop)
            {
                switch (position)
                {
                    case Position.Front: return 50 + rawValue[0];
                    case Position.Rear: return 50 + rawValue[2];
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
