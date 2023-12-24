using RaceElement.Data.SetupRanges;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.Cars.TCX;

internal class BMWM2CSRacing : ICarSetupConversion, ISetupChanger
{
    public CarModels CarModel => CarModels.BMW_M2_Cup_2020;

    CarClasses ICarSetupConversion.CarClass => CarClasses.TCX;
    public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHF2023_GT4;


    private static readonly double[] casters = new double[] { 8.5 };
    private static readonly int[] wheelRateFronts = new int[] { 162000, 180000, 198000 };
    private static readonly int[] wheelRateRears = new int[] { 103000, 117000, 131000 };

    ITyreSetupChanger ISetupChanger.TyreSetupChanger => new TyreSetupChanger();
    IElectronicsSetupChanger ISetupChanger.ElectronicsSetupChanger => new ElectronicsSetupChanger();
    IMechanicalSetupChanger ISetupChanger.MechanicalSetupChanger => new MechSetupChanger();
    IAeroSetupChanger ISetupChanger.AeroSetupChanger => new AeroSetupChanger();
    IDamperSetupChanger ISetupChanger.DamperSetupChanger => new DamperSetupChanger();

    private class TyreSetupChanger : ITyreSetupChanger
    {
        public SetupDoubleRange TyrePressures => TyrePressuresGT4;
        public SetupDoubleRange CamberFront => new(-5.0, -3.0, 0.1);
        public SetupDoubleRange CamberRear => new(-3.5, -2.0, 0.1);
        public SetupDoubleRange ToeFront => new(-0.2, 0.2, 0.01);
        public SetupDoubleRange ToeRear => new(0.0, 0.31, 0.01);
        public SetupDoubleRange Caster => new(casters);
    }

    private class ElectronicsSetupChanger : IElectronicsSetupChanger
    {
        public SetupIntRange TractionControl => new(0, 4, 1);
        public SetupIntRange ABS => new(0, 2, 1);
        public SetupIntRange EcuMap => new(1, 1, 1);
        public SetupIntRange TractionControlCut => new(0, 0, 1);
    }

    private class MechSetupChanger : IMechanicalSetupChanger
    {
        public SetupIntRange AntiRollBarFront => new(0, 2, 1);
        public SetupIntRange AntiRollBarRear => AntiRollBarFront;
        public SetupDoubleRange BrakeBias => new(56.0, 56.0, 0.2);
        public SetupIntRange PreloadDifferential => new(20, 20, 10);
        public SetupIntRange BrakePower => new(80, 100, 1);
        public SetupDoubleRange SteeringRatio => new(10, 17, 1);
        public SetupIntRange WheelRateFronts => new(wheelRateFronts);
        public SetupIntRange WheelRateRears => new(wheelRateRears);
        public SetupIntRange BumpstopRate => new(300, 1000, 100);
        public SetupIntRange BumpstopRangeFronts => new(0, 48, 1);
        public SetupIntRange BumpstopRangeRears => new(0, 40, 1);
    }

    private class AeroSetupChanger : IAeroSetupChanger
    {
        public SetupIntRange RideHeightFront => new(125, 140, 1);
        public SetupIntRange RideHeightRear => RideHeightFront;
        public SetupIntRange BrakeDucts => BrakeDuctsGT3;
        public SetupIntRange Splitter => new(0, 0, 1);
        public SetupIntRange RearWing => new(1, 5, 1);
    }

    private class DamperSetupChanger : IDamperSetupChanger
    {
        public SetupIntRange BumpSlow => new(1, 20, 1);
        public SetupIntRange BumpFast => new(0, 0, 1);
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
                case Position.Front: return Math.Round(-5 + 0.1 * rawValue[(int)wheel], 2);
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

            switch (GetPosition(wheel))
            {
                case Position.Front: return Math.Round(-0.2 + 0.01 * rawValue[(int)wheel], 2);
                case Position.Rear: return Math.Round(0 + 0.01 * rawValue[(int)wheel], 2);
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
            return Math.Round(56 + 0.2 * rawValue, 2);
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
            return 300 + 0 * rawValue[(int)wheel];
        }

        public int PreloadDifferential(int rawValue)
        {
            return 20 + rawValue * 10;
        }

        public double SteeringRatio(int rawValue)
        {
            return Math.Round(10d + rawValue, 2);
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

    //IDamperSetup ICarSetupConversion.DamperSetup => DefaultDamperSetup;
    IDamperSetup ICarSetupConversion.DamperSetup => new CustomDamperSetup();
    private class CustomDamperSetup : IDamperSetup
    {
        int IDamperSetup.BumpFast(List<int> rawValue, Wheel wheel)
        {
            return rawValue[(int)wheel];
        }

        int IDamperSetup.BumpSlow(List<int> rawValue, Wheel wheel)
        {
            return rawValue[(int)wheel] + 1;
        }

        int IDamperSetup.ReboundFast(List<int> rawValue, Wheel wheel)
        {
            return rawValue[(int)wheel];
        }

        int IDamperSetup.ReboundSlow(List<int> rawValue, Wheel wheel)
        {
            return rawValue[(int)wheel] + 1;
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

        public int RideHeight(List<int> rawValue, Position position)
        {
            switch (position)
            {
                case Position.Front: return 125 + rawValue[0];
                case Position.Rear: return 125 + rawValue[2];
                default: return -1;
            }
        }

        public int Splitter(int rawValue)
        {
            return rawValue;
        }
    }
}
