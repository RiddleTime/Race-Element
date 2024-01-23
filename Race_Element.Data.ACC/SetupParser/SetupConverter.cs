using RaceElement.Data.SetupRanges;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;

namespace RaceElement.Data;

public class SetupConverter
{
    public enum CarClasses
    {
        GT2,
        GT3,
        GT4,
        CUP,
        ST,
        TCX,
        CHL
    }

    public enum DryTyreCompounds
    {
        DHF2023_GT4,
        DHF2023,
    }

    public enum Wheel
    {
        FrontLeft,
        FrontRight,
        RearLeft,
        RearRight
    }

    public enum Position
    {
        Front,
        Rear
    }

    public enum TyreCompound
    {
        Dry,
        Wet
    }

    public static Position GetPosition(Wheel wheel) => wheel switch
    {
        Wheel.FrontLeft => Position.Front,
        Wheel.FrontRight => Position.Front,
        _ => Position.Rear,
    };

    public interface ICarSetupConversion
    {
        CarModels CarModel { get; }
        CarClasses CarClass { get; }
        DryTyreCompounds DryTyreCompound { get; }

        AbstractTyresSetup TyresSetup { get; }
        IDamperSetup DamperSetup { get; }
        IMechanicalSetup MechanicalSetup { get; }
        IAeroBalance AeroBalance { get; }

    }

    public interface IMechanicalSetup
    {
        int AntiRollBarFront(int rawValue);
        int AntiRollBarRear(int rawValue);
        int PreloadDifferential(int rawValue);

        double BrakeBias(int rawValue);

        /**
         * The brake power in %
         */
        int BrakePower(int rawValue);

        double SteeringRatio(int rawValue);
        int WheelRate(List<int> rawValue, Wheel wheel);

        int BumpstopRate(List<int> rawValue, Wheel wheel);
        int BumpstopRange(List<int> rawValue, Wheel wheel);
    }

    public interface IDamperSetup
    {
        int BumpSlow(List<int> rawValue, Wheel wheel);
        int BumpFast(List<int> rawValue, Wheel wheel);
        int ReboundSlow(List<int> rawValue, Wheel wheel);
        int ReboundFast(List<int> rawValue, Wheel wheel);
    }

    internal static IDamperSetup DefaultDamperSetup = new DefaultDamperSetupImplementation();
    private class DefaultDamperSetupImplementation : IDamperSetup
    {
        int IDamperSetup.BumpFast(List<int> rawValue, Wheel wheel)
        {
            return rawValue[(int)wheel];
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

    public interface IAeroBalance
    {
        int RideHeight(List<int> rawValue, Position position);
        int BrakeDucts(int rawValue);
        int RearWing(int rawValue);
        int Splitter(int rawValue);
    }

    public interface IElectronicsSetup
    {
        int TractionControl { get; }
        int ABS { get; }
        int EcuMap { get; }
        int TractionControl2 { get; }
    }

    public interface ISetupChanger
    {
        ITyreSetupChanger TyreSetupChanger { get; }
        IElectronicsSetupChanger ElectronicsSetupChanger { get; }
        IMechanicalSetupChanger MechanicalSetupChanger { get; }
        IAeroSetupChanger AeroSetupChanger { get; }
        IDamperSetupChanger DamperSetupChanger { get; }
    }

    public interface ITyreSetupChanger
    {
        SetupDoubleRange TyrePressures { get; }
        SetupDoubleRange CamberFront { get; }
        SetupDoubleRange CamberRear { get; }
        SetupDoubleRange ToeFront { get; }
        SetupDoubleRange ToeRear { get; }
        SetupDoubleRange Caster { get; }
    }

    public interface IElectronicsSetupChanger
    {
        SetupIntRange TractionControl { get; }
        SetupIntRange ABS { get; }
        SetupIntRange EcuMap { get; }
        SetupIntRange TractionControlCut { get; }
    }

    public static SetupDoubleRange TyrePressuresGT3 = new(20.3, 35, 0.1);
    public static SetupDoubleRange TyrePressuresGT4 = new(17.0, 35, 0.1);
    public static SetupDoubleRange CamberFrontGT3 => new(-4, -1.5, 0.1);
    public static SetupDoubleRange CamberRearGT3 => new(-3.5, -1, 0.1);
    public static SetupIntRange BrakeDuctsGT3 => new(0, 6, 1);

    public interface IMechanicalSetupChanger
    {
        SetupIntRange AntiRollBarFront { get; }
        SetupIntRange AntiRollBarRear { get; }
        SetupIntRange PreloadDifferential { get; }
        SetupDoubleRange BrakeBias { get; }
        SetupIntRange BrakePower { get; }
        SetupDoubleRange SteeringRatio { get; }
        SetupIntRange WheelRateFronts { get; }
        SetupIntRange WheelRateRears { get; }
        SetupIntRange BumpstopRate { get; }
        SetupIntRange BumpstopRangeFronts { get; }
        SetupIntRange BumpstopRangeRears { get; }
    }

    public interface IAeroSetupChanger
    {
        SetupIntRange RideHeightFront { get; }
        SetupIntRange RideHeightRear { get; }
        SetupIntRange BrakeDucts { get; }
        SetupIntRange RearWing { get; }
        SetupIntRange Splitter { get; }
    }

    public interface IDamperSetupChanger
    {
        SetupIntRange BumpSlow { get; }
        SetupIntRange BumpFast { get; }
        SetupIntRange ReboundSlow { get; }
        SetupIntRange ReboundFast { get; }
    }

    public abstract class AbstractTyresSetup
    {
        public static TyreCompound Compound(int rawValue)
        {
            return rawValue switch
            {
                0 => TyreCompound.Dry,
                1 => TyreCompound.Wet,
                _ => TyreCompound.Dry,
            };
        }

        public static double TirePressure(DryTyreCompounds compound, Wheel wheel, List<int> rawValue) => compound switch
        {
            DryTyreCompounds.DHF2023 => Math.Round(20.3f + 0.1f * rawValue[(int)wheel], 2),
            DryTyreCompounds.DHF2023_GT4 => Math.Round(17.0f + 0.1f * rawValue[(int)wheel], 2),
            _ => -1,
        };

        public abstract double Toe(Wheel wheel, List<int> rawValue);
        public abstract double Camber(Wheel wheel, List<int> rawValue);
        public abstract double Caster(int rawValue);
    }
}

