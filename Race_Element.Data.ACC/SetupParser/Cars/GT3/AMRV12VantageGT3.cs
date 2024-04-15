using RaceElement.Data.SetupRanges;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.Cars.GT3;

internal class AMRV12VantageGT3 : ICarSetupConversion, ISetupChanger
{
    public CarModels CarModel => CarModels.Aston_Martin_Vantage_V12_GT3_2013;

    CarClasses ICarSetupConversion.CarClass => CarClasses.GT3;
    public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHF2023;

    private static readonly double[] casters = [8.3,
        8.5,
        8.7,
        9.0,
        9.2,
        9.4,
        9.6,
        9.9,
        10.1,
        10.3,
        10.5,
        10.8,
        11.0,
        11.2,
        11.4,
        11.6,
        11.9,
        12.1,
        12.3,
        12.5,
        12.7,
        13.0,
        13.2,
        13.4,
        13.6,
        13.8,
        14.0,
        14.3,
        14.5,
        14.7,
        14.9];
    private static readonly int[] wheelRateFronts = [115000,
        120000,
        125000,
        130000,
        135000,
        140000,
        145000,
        150000,
        155000,
        160000,
        165000,
        170000,
        175000,
        180000,
        185000];
    private static readonly int[] wheelRateRears = [95000,
        100000,
        105000,
        110000,
        115000,
        120000,
        125000,
        130000,
        135000,
        140000,
        145000,
        150000,
        155000,
        160000,
        165000,
        170000,
        175000,
        180000,
        185000,
        190000,
        195000];

    ITyreSetupChanger ISetupChanger.TyreSetupChanger => new TyreSetupChanger();
    IElectronicsSetupChanger ISetupChanger.ElectronicsSetupChanger => new ElectronicsSetupChanger();
    IMechanicalSetupChanger ISetupChanger.MechanicalSetupChanger => new MechSetupChanger();
    IAeroSetupChanger ISetupChanger.AeroSetupChanger => new AeroSetupChanger();
    IDamperSetupChanger ISetupChanger.DamperSetupChanger => new DamperSetupChanger();

    private class TyreSetupChanger : ITyreSetupChanger
    {
        public SetupDoubleRange TyrePressures => TyrePressuresGT3;
        public SetupDoubleRange CamberFront => CamberFrontGT3;
        public SetupDoubleRange CamberRear => CamberRearGT3;
        public SetupDoubleRange ToeFront => new(-0.4, 0.4, 0.01);
        public SetupDoubleRange ToeRear => ToeFront;
        public SetupDoubleRange Caster => new(casters);
    }

    private class ElectronicsSetupChanger : IElectronicsSetupChanger
    {
        public SetupIntRange TractionControl => new(0, 11, 1);
        public SetupIntRange ABS => new(0, 11, 1);
        public SetupIntRange EcuMap => new(1, 8, 1);
        public SetupIntRange TractionControlCut => new(0, 0, 1);
    }

    private class MechSetupChanger : IMechanicalSetupChanger
    {
        public SetupIntRange AntiRollBarFront => new(0, 8, 1);
        public SetupIntRange AntiRollBarRear => AntiRollBarFront;
        public SetupDoubleRange BrakeBias => new(57.0, 78.0, 0.2);
        public SetupIntRange PreloadDifferential => new(20, 300, 10);
        public SetupIntRange BrakePower => new(80, 100, 1);
        public SetupDoubleRange SteeringRatio => new(14, 18, 1);
        public SetupIntRange WheelRateFronts => new(wheelRateFronts);
        public SetupIntRange WheelRateRears => new(wheelRateRears);
        public SetupIntRange BumpstopRate => new(300, 2500, 100);
        public SetupIntRange BumpstopRangeFronts => new(0, 22, 1);
        public SetupIntRange BumpstopRangeRears => new(0, 68, 1);
    }

    private class AeroSetupChanger : IAeroSetupChanger
    {
        public SetupIntRange RideHeightFront => new(55, 80, 1);
        public SetupIntRange RideHeightRear => new(55, 90, 1);
        public SetupIntRange BrakeDucts => BrakeDuctsGT3;
        public SetupIntRange Splitter => new(0, 0, 1);
        public SetupIntRange RearWing => new(0, 10, 1);
    }

    private class DamperSetupChanger : IDamperSetupChanger
    {
        public SetupIntRange BumpSlow => new(0, 40, 1);
        public SetupIntRange BumpFast => new(0, 49, 1);
        public SetupIntRange ReboundSlow => BumpSlow;
        public SetupIntRange ReboundFast => BumpFast;
    }

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
            return 0 + rawValue;
        }

        public int AntiRollBarRear(int rawValue)
        {
            return 0 + rawValue;
        }

        public double BrakeBias(int rawValue)
        {
            return Math.Round(57 + 0.2 * rawValue, 2);
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
            return Math.Round(14d + rawValue, 2);
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
