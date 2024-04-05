using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.ACC.SetupParser.Cars.GT2;

internal class MaseratiGT2_2023 : ICarSetupConversion
{
    public ConversionFactory.CarModels CarModel => CarModels.Maserati_GT2_2023;

    public CarClasses CarClass => CarClasses.GT2;

    public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHF2023_GT4;

    private static readonly double[] casters = [8.5,
        8.7,
        8.9,
        9.1,
        9.2,
        9.4,
        9.6,
        9.8,
        9.9,
        10.1,
        10.3];
    private static readonly int[] wheelRateFronts = [165180, 179540, 193900];
    private static readonly int[] wheelRateRears = [173913, 189036, 204158];

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
            return Math.Round(55 + 0.2 * rawValue, 2);
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
            return GetPosition(wheel) switch
            {
                Position.Front => 300 + 100 * rawValue[(int)wheel],
                Position.Rear => 400 + 200 * rawValue[(int)wheel],
            };
        }

        public int PreloadDifferential(int rawValue)
        {
            return 20 + rawValue * 10;
        }

        public double SteeringRatio(int rawValue)
        {
            return Math.Round(11d + rawValue, 2);
        }

        public int WheelRate(List<int> rawValue, Wheel wheel)
        {
            return GetPosition(wheel) switch
            {
                Position.Front => wheelRateFronts[rawValue[(int)wheel]],
                Position.Rear => wheelRateRears[rawValue[(int)wheel]],
                _ => -1
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
                Position.Front => 80 + rawValue[0],
                Position.Rear => 80 + rawValue[2],
                _ => -1
            };
        }

        public int Splitter(int rawValue)
        {
            return rawValue;
        }
    }
}
