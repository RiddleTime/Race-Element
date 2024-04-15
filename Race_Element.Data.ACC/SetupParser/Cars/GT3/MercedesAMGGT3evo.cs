using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.Cars.GT3;

internal class MercedesAMGGT3evo : ICarSetupConversion
{
    public CarModels CarModel => CarModels.Mercedes_AMG_GT3_Evo_2020;

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

        private readonly double[] casters = [6.0,
            6.2,
            6.4,
            6.6,
            6.7,
            6.9,
            7.1,
            7.3,
            7.5,
            7.7,
            7.9,
            8.1,
            8.2,
            8.4,
            8.6,
            8.8,
            9.0,
            9.2,
            9.4,
            9.5,
            9.7,
            9.9,
            10.1,
            10.3,
            10.5,
            10.7,
            10.8,
            11.0,
            11.2,
            11.4,
            11.6,
            11.8,
            11.9,
            12.1,
            12.3,
            12.5,
            12.7,
            12.8,
            13.0,
            13.2,
            13.4,
            13.6,
            13.7,
            13.9,
            14.1];
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
            return Math.Round(11d + rawValue, 2);
        }

        private readonly int[] fronts = [130000, 143000, 155000, 171000, 187000, 202000];
        private readonly int[] rears = [71000, 83000, 95000, 107000, 119000, 131000];
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

        public int RideHeight(List<int> rawValue, Position position, int trackTypeBop)
        {
            switch (position)
            {
                case Position.Front: return 42 + rawValue[0];
                case Position.Rear: return 67 + rawValue[2];
                default: return -1;
            }
        }

        public int Splitter(int rawValue)
        {
            return rawValue + 1;
        }
    }
}
