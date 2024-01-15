using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.Cars.GT3;

internal class NissanGtrGT3_2018 : ICarSetupConversion
{
    public CarModels CarModel => CarModels.Nissan_GT_R_Nismo_GT3_2018;

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


        private readonly double[] casters = [12.5,
            12.6,
            12.8,
            13.0,
            13.2,
            13.4,
            13.6,
            13.8,
            13.9,
            14.1,
            14.3,
            14.5,
            14.7,
            14.9,
            15.0,
            15.2,
            15.4,
            15.6,
            15.8,
            16.0,
            16.1,
            16.3,
            16.5,
            16.7,
            16.9,
            17.0,
            17.2,
            17.4,
            17.6,
            17.8,
            17.9,
            18.1,
            18.3,
            18.5,
            18.6,
            18.8,
            19.0,
            19.2,
            19.3,
            19.5,
            19.7,
        ];
        public override double Caster(int rawValue)
        {
            return Math.Round(casters[rawValue], 2);
        }

        public override double Toe(Wheel wheel, List<int> rawValue)
        {

            switch (GetPosition(wheel))
            {
                case Position.Front: return Math.Round(-0.2 + 0.01 * rawValue[(int)wheel], 2);
                case Position.Rear: return Math.Round(0.0 + 0.01 * rawValue[(int)wheel], 2);
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
            return Math.Round(47.5 + 0.3 * rawValue, 2);
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
            return Math.Round(12d + rawValue, 2);
        }

        private readonly int[] fronts = [136000, 146000, 156000, 166000, 176000, 186000];
        private readonly int[] rears = [96000, 106000, 116000, 126000, 136000, 146000];
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
