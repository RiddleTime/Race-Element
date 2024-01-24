using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.ACC.SetupParser.Cars.GT2;

internal class AudiR8LMS_GT2_2021 : ICarSetupConversion
{
    public ConversionFactory.CarModels CarModel => CarModels.Audi_R8_LMS_GT2_2021;

    public CarClasses CarClass => CarClasses.GT2;

    public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHF2023_GT4;

    private static readonly double[] casters = [6.6,
        6.8,
        7.0,
        7.2,
        7.4,
        7.6,
        7.8,
        8.0,
        8.2,
        8.4,
        8.6,
        8.8,
        9.0,
        9.2,
        9.4,
        9.6,
        9.8,
        10.0,
        10.2,
        10.4,
        10.6,
        10.8,
        11.0,
        11.2,
        11.4,
        11.6,
        11.8,
        12.0,
        12.2,
        12.3,
        12.5,
        12.7,
        12.9,
        13.1,
        13.3
    ];
    private static readonly int[] wheelRateFronts = [142000, 160000];
    private static readonly int[] wheelRateRears = [146000, 163000];

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
    IDamperSetup ICarSetupConversion.DamperSetup => new DamperSetup();
    private class DamperSetup : IDamperSetup
    {
        public int BumpFast(List<int> rawValue, Wheel wheel)
        {
            return rawValue[(int)wheel];
        }

        public int BumpSlow(List<int> rawValue, Wheel wheel)
        {
            return rawValue[(int)wheel] + 1;
        }

        public int ReboundFast(List<int> rawValue, Wheel wheel)
        {
            return rawValue[(int)wheel];
        }

        public int ReboundSlow(List<int> rawValue, Wheel wheel)
        {
            return rawValue[(int)wheel] + 1;
        }
    }

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
            return 10 + rawValue * 10;
        }

        public double SteeringRatio(int rawValue)
        {
            return Math.Round(12d + rawValue, 2);
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
            return rawValue;
        }

        public int RideHeight(List<int> rawValue, Position position)
        {
            return position switch
            {
                Position.Front => 80 + rawValue[0],
                Position.Rear => 80 + rawValue[2],
                _ => -1,
            };
        }

        public int Splitter(int rawValue)
        {
            return rawValue;
        }
    }
}
