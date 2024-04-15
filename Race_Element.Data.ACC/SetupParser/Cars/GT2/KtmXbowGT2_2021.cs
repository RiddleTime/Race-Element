using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.ACC.SetupParser.Cars.GT2;

internal class KtmXbowGT2_2021 : ICarSetupConversion
{
    public ConversionFactory.CarModels CarModel => CarModels.KTM_Xbow_GT2_2021;

    public CarClasses CarClass => CarClasses.GT2;

    public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHF2023_GT4;

    private static readonly double[] casters = [3.2,
        3.4,
        3.6,
        3.9,
        4.1,
        4.3,
        4.5,
        4.7,
        4.9,
        5.2,
        5.4,
        5.6,
        5.8,
        6.0,
        6.2,
        6.5,
        6.7,
        6.9,
        7.1,
        7.3,
        7.5,
        7.8,
        8.0,
        8.2,
        8.4,
        8.6,
        8.8,
        9.0,
        9.2,
        9.5,
        9.7,
        9.9,
        10.1,
        10.3,
        10.5,
        10.7,
        10.9,
        11.1,
        11.4,
        11.6,
        11.8
    ];
    private static readonly int[] wheelRateFronts = [87000, 97000, 107000, 117000, 127000];
    private static readonly int[] wheelRateRears = [81000, 91000, 101000, 111000, 121000, 131000];

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
            return Math.Round(-0.4 + 0.01 * rawValue[(int)wheel], 2);
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
            return 300 + 100 * rawValue[(int)wheel];
        }

        public int PreloadDifferential(int rawValue)
        {
            return 10 + rawValue * 10;
        }

        public double SteeringRatio(int rawValue)
        {
            return Math.Round(11d + rawValue, 2);
        }

        public int WheelRate(List<int> rawValue, Wheel wheel)
        {
            switch (GetPosition(wheel))
            {
                case Position.Front: return wheelRateFronts[rawValue[(int)wheel]];
                case Position.Rear: return wheelRateRears[rawValue[(int)wheel]];
                default: return -1;
            }
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
                Position.Front => 75 + rawValue[0],
                Position.Rear => 200 + rawValue[2],
                _ => -1,
            };
        }

        public int Splitter(int rawValue)
        {
            return rawValue;
        }
    }
}
