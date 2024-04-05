using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.ACC.SetupParser.Cars.GT2;

internal class MercedesAmgGT2_2023 : ICarSetupConversion
{
    public ConversionFactory.CarModels CarModel => CarModels.Mercedes_AMG_GT2_2023;

    public CarClasses CarClass => CarClasses.GT2;

    public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHF2023_GT4;

    private static readonly double[] casters = [9.2,
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
        14.1,
        14.3,
        14.5,
        14.6,
        14.8,
        15.0,
        15.2,
        15.4,
        15.5,
        15.7,
        15.9,
        16.1,
        16.2,
        16.4,
    ];
    private static readonly int[] wheelRateFronts = [78000, 88000, 104000];
    private static readonly int[] wheelRateRears = [66000, 76000, 86000];

    AbstractTyresSetup ICarSetupConversion.TyresSetup => new TyreSetup();
    private class TyreSetup : AbstractTyresSetup
    {
        public override double Camber(Wheel wheel, List<int> rawValue)
        {
            switch (GetPosition(wheel))
            {
                case Position.Front: return Math.Round(-3.5 + 0.1 * rawValue[(int)wheel], 2);
                case Position.Rear: return Math.Round(-3 + 0.1 * rawValue[(int)wheel], 2);
                default: return -1;
            }
        }

        public override double Caster(int rawValue)
        {
            if (rawValue > casters.Length - 1) return casters[^1];
            return Math.Round(casters[rawValue], 2);
        }

        public override double Toe(Wheel wheel, List<int> rawValue)
        {
            return GetPosition(wheel) switch
            {
                Position.Front => Math.Round(-.2 + 0.01 * rawValue[(int)wheel], 2),
                Position.Rear => Math.Round(0 + 0.01 * rawValue[(int)wheel], 2),
                _ => -1,
            };
        }
    }
    IDamperSetup ICarSetupConversion.DamperSetup => DefaultDamperSetup;

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
            return Math.Round(55 + 0.3 * rawValue, 2);
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
            return 1000;
        }

        public int PreloadDifferential(int rawValue)
        {
            return 10 + rawValue * 10;
        }

        public double SteeringRatio(int rawValue)
        {
            return Math.Round(10d + rawValue, 2);
        }

        public int WheelRate(List<int> rawValue, Wheel wheel)
        {
            return GetPosition(wheel) switch
            {
                Position.Front => wheelRateFronts[rawValue[(int)wheel]],
                Position.Rear => wheelRateRears[rawValue[(int)wheel]],
                _ => -1,
            };
        }
    }
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
            return position switch
            {
                Position.Front => 122 + rawValue[0],
                Position.Rear => 140 + rawValue[2],
                _ => -1,
            };
        }
        public int Splitter(int rawValue)
        {
            return rawValue;
        }
    }
}
