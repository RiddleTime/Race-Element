using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.Cars.GT3;

internal class Mclaren650sGT3_2015 : ICarSetupConversion
{
    public CarModels CarModel => CarModels.McLaren_650S_GT3_2015;

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
            5.6,
            5.8,
            6.0,
            6.3,
            6.5,
            6.8,
            7.0,
            7.3,
            7.5,
            7.8,
            8.0,
            8.2,
            8.5,
            8.7,
            9.0,
            9.2,
            9.4,
            9.7,
            9.9,
            10.2,
            10.4,
            10.7,
            10.9,
            11.1,
            11.4,
            11.6,
            11.8,
            12.1,
            12.3,
            12.6,
            12.8,
            13.0,
            13.3,
            13.5,
            13.7,
            14.0,
            14.2,
            14.4,
            14.7,
            14.9];
        public override double Caster(int rawValue)
        {
            return Math.Round(casters[rawValue], 2);
        }

        public override double Toe(Wheel wheel, List<int> rawValue)
        {
            switch (GetPosition(wheel))
            {
                case Position.Front: return Math.Round(-0.4 + 0.1 * rawValue[(int)wheel], 2);
                case Position.Rear: return Math.Round(-0.4 + 0.1 * rawValue[(int)wheel], 2);
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
            return Math.Round(11d + rawValue, 2);
        }

        private readonly int[] fronts = [126000, 136000, 146000, 156000, 166000, 176000];
        private readonly int[] rears = [126000, 136000, 146000, 156000, 166000, 176000];
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
                case Position.Front: return 56 + rawValue[0];
                case Position.Rear: return 56 + rawValue[2];
                default: return -1;
            }
        }

        public int Splitter(int rawValue)
        {
            return rawValue;
        }
    }
}
