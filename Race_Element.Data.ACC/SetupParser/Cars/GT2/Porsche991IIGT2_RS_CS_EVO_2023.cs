using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.ACC.SetupParser.Cars.GT2;

internal class Porsche991IIGT2_RS_CS_EVO_2023 : ICarSetupConversion
{
    public ConversionFactory.CarModels CarModel => CarModels.Porsche_991_II_GT2_RS_CS_EVO_2023;

    public CarClasses CarClass => CarClasses.GT2;

    public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHF2023_GT4;

    private static readonly double[] casters = [7.3,
        7.4,
        7.5,
        7.6,
        7.7,
        7.8,
        7.9,
        8.0,
        8.1,
        8.2,
        8.3,
        8.4,
        8.5,
        8.6,
        8.7,
        8.8,
        8.9,
        9.0,
        9.1,
        9.2,
        9.3,
        9.4,
        9.5,
        9.6,
        9.7,
        9.8,
        9.9,
        10.0,
        10.1,
        10.2,
        10.3];
    private static readonly int[] wheelRateFronts = [0, 1, 2, 3];
    private static readonly int[] wheelRateRears = [0, 1, 2, 3];

    AbstractTyresSetup ICarSetupConversion.TyresSetup => new TyreSetup();
    private class TyreSetup : AbstractTyresSetup
    {
        public override double Camber(Wheel wheel, List<int> rawValue)
        {
            return GetPosition(wheel) switch
            {
                Position.Front => Math.Round(-3.5 + 0.1 * rawValue[(int)wheel], 2),
                Position.Rear => Math.Round(-3 + 0.1 * rawValue[(int)wheel], 2),
                _ => -1,
            };
        }

        public override double Caster(int rawValue)
        {
            if (rawValue > casters.Length - 1) return casters[^1];
            return Math.Round(casters[rawValue], 2);
        }

        public override double Toe(Wheel wheel, List<int> rawValue)
        {
            return Math.Round(-.4 + 0.01 * rawValue[(int)wheel], 2);
        }
    }
    IDamperSetup ICarSetupConversion.DamperSetup => new DamperSetup();

    private class DamperSetup : IDamperSetup
    {
        int IDamperSetup.BumpFast(List<int> rawValue, Wheel wheel)
        {
            return 0;
        }

        int IDamperSetup.BumpSlow(List<int> rawValue, Wheel wheel)
        {
            return rawValue[(int)wheel];
        }

        int IDamperSetup.ReboundFast(List<int> rawValue, Wheel wheel)
        {
            return rawValue[(int)wheel];
        }

        int IDamperSetup.ReboundSlow(List<int> rawValue, Wheel wheel)
        {
            return rawValue[(int)wheel];
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
            return 1000;
        }

        public int PreloadDifferential(int rawValue)
        {
            return 0;
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
                Position.Front => 100 + rawValue[0],
                Position.Rear => 167 + rawValue[2],
                _ => -1,
            };
        }

        public int Splitter(int rawValue)
        {
            return rawValue;
        }
    }
}

