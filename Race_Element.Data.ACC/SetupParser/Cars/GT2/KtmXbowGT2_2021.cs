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

    private static readonly double[] casters = [];
    private static readonly int[] wheelRateFronts = [];
    private static readonly int[] wheelRateRears = [];
    private static readonly double[] casters = [];
    private static readonly int[] wheelRateFronts = [];
    private static readonly int[] wheelRateRears = [];

    AbstractTyresSetup ICarSetupConversion.TyresSetup => new TyreSetup();
    private class TyreSetup : AbstractTyresSetup
    {
        public override double Camber(Wheel wheel, List<int> rawValue)
        {
            throw new NotImplementedException();
        }

        public override double Caster(int rawValue)
        {
            throw new NotImplementedException();
        }

        public override double Toe(Wheel wheel, List<int> rawValue)
        {
            throw new NotImplementedException();
        }
    }
    IDamperSetup ICarSetupConversion.DamperSetup => new DamperSetup();
    private class DamperSetup : IDamperSetup
    {
        public int BumpFast(List<int> rawValue, Wheel wheel)
        {
            throw new NotImplementedException();
        }

        public int BumpSlow(List<int> rawValue, Wheel wheel)
        {
            throw new NotImplementedException();
        }

        public int ReboundFast(List<int> rawValue, Wheel wheel)
        {
            throw new NotImplementedException();
        }

        public int ReboundSlow(List<int> rawValue, Wheel wheel)
        {
            throw new NotImplementedException();
        }
    }

    IMechanicalSetup ICarSetupConversion.MechanicalSetup => new MechSetup();
    private class MechSetup : IMechanicalSetup
    {
        public int AntiRollBarFront(int rawValue)
        {
            throw new NotImplementedException();
        }

        public int AntiRollBarRear(int rawValue)
        {
            throw new NotImplementedException();
        }

        public double BrakeBias(int rawValue)
        {
            throw new NotImplementedException();
        }

        public int BrakePower(int rawValue)
        {
            throw new NotImplementedException();
        }

        public int BumpstopRange(List<int> rawValue, Wheel wheel)
        {
            throw new NotImplementedException();
        }

        public int BumpstopRate(List<int> rawValue, Wheel wheel)
        {
            throw new NotImplementedException();
        }

        public int PreloadDifferential(int rawValue)
        {
            throw new NotImplementedException();
        }

        public double SteeringRatio(int rawValue)
        {
            throw new NotImplementedException();
        }

        public int WheelRate(List<int> rawValue, Wheel wheel)
        {
            throw new NotImplementedException();
        }
    }
    IAeroBalance ICarSetupConversion.AeroBalance => new AeroSetup();
    private class AeroSetup : IAeroBalance
    {
        public int BrakeDucts(int rawValue)
        {
            throw new NotImplementedException();
        }

        public int RearWing(int rawValue)
        {
            throw new NotImplementedException();
        }

        public int RideHeight(List<int> rawValue, Position position)
        {
            throw new NotImplementedException();
        }

        public int Splitter(int rawValue)
        {
            throw new NotImplementedException();
        }
    }
}
