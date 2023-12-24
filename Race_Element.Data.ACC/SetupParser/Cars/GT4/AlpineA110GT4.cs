using RaceElement.Data.SetupRanges;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.Cars.GT4
{
    internal class AlpineA110GT4 : ICarSetupConversion, ISetupChanger
    {
        public CarModels CarModel => CarModels.Alpine_A110_GT4_2018;

        CarClasses ICarSetupConversion.CarClass => CarClasses.GT4;
        public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHF2023_GT4;

        private static readonly double[] casters = new double[] { 7.3, 7.5, 7.7, 7.9, 8.1, 8.3, 8.5, 8.6, 8.8, 9.0, 9.2,
                    9.4, 9.6, 9.8, 10.0, 10.1, 10.3, 10.5, 10.7, 10.9, 11.1, 11.3, 11.5, 11.6, 11.8, 12.0, 12.2,
                    12.4, 12.6, 12.7, 12.9, 13.1, 13.3, 13.5, 13.7 };
        private static readonly int[] wheelRateFronts = new int[] { 62500, 72500, 82500, 92500 };
        private static readonly int[] wheelRateRears = new int[] { 73300, 83300, 93300, 103300 };

        ITyreSetupChanger ISetupChanger.TyreSetupChanger => new TyreSetupChanger();
        IElectronicsSetupChanger ISetupChanger.ElectronicsSetupChanger => new ElectronicsSetupChanger();
        IMechanicalSetupChanger ISetupChanger.MechanicalSetupChanger => new MechSetupChanger();
        IAeroSetupChanger ISetupChanger.AeroSetupChanger => new AeroSetupChanger();
        IDamperSetupChanger ISetupChanger.DamperSetupChanger => new DamperSetupChanger();

        private class TyreSetupChanger : ITyreSetupChanger
        {
            public SetupDoubleRange TyrePressures => TyrePressuresGT4;
            public SetupDoubleRange CamberFront => new(-5.0, 0.0, 0.1);
            public SetupDoubleRange CamberRear => CamberFront;
            public SetupDoubleRange ToeFront => new(-0.4, 0.4, 0.01);
            public SetupDoubleRange ToeRear => ToeFront;
            public SetupDoubleRange Caster => new(casters);
        }

        private class ElectronicsSetupChanger : IElectronicsSetupChanger
        {
            public SetupIntRange TractionControl => new(0, 5, 1);
            public SetupIntRange ABS => new(0, 10, 1);
            public SetupIntRange EcuMap => new(1, 1, 1);
            public SetupIntRange TractionControlCut => new(0, 0, 1);
        }

        private class MechSetupChanger : IMechanicalSetupChanger
        {
            public SetupIntRange AntiRollBarFront => new(0, 3, 1);
            public SetupIntRange AntiRollBarRear => new(0, 2, 1);
            public SetupDoubleRange BrakeBias => new(45.0, 70.0, 0.2);
            public SetupIntRange PreloadDifferential => new(10, 80, 10);
            public SetupIntRange BrakePower => new(80, 100, 1);
            public SetupDoubleRange SteeringRatio => new(12, 18, 1);
            public SetupIntRange WheelRateFronts => new(wheelRateFronts);
            public SetupIntRange WheelRateRears => new(wheelRateRears);
            public SetupIntRange BumpstopRate => new(300, 1000, 100);
            public SetupIntRange BumpstopRangeFronts => new(0, 0, 1);
            public SetupIntRange BumpstopRangeRears => new(0, 0, 1);
        }

        private class AeroSetupChanger : IAeroSetupChanger
        {
            public SetupIntRange RideHeightFront => new(95, 120, 1);
            public SetupIntRange RideHeightRear => new(85, 125, 1);
            public SetupIntRange BrakeDucts => BrakeDuctsGT3;
            public SetupIntRange Splitter => new(0, 0, 1);
            public SetupIntRange RearWing => new(0, 4, 1);
        }

        private class DamperSetupChanger : IDamperSetupChanger
        {
            public SetupIntRange BumpSlow => new(0, 24, 1);
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
                    case Position.Rear: return Math.Round(-5 + 0.1 * rawValue[(int)wheel], 2);
                    default: return -1;
                }
            }

            //private readonly double[] casters = new double[] { 7.3, 7.5, 7.7, 7.9, 8.1, 8.3, 8.5, 8.6, 8.8, 9.0, 9.2, 9.4, 9.6, 9.8, 10.0, 10.1, 10.3, 10.5, 10.7, 10.9, 11.1, 11.3, 11.5, 11.6, 11.8, 12.0, 12.2, 12.4, 12.6, 12.7, 12.9, 13.1, 13.3, 13.5, 13.7 };
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
                return Math.Round(45 + 0.2 * rawValue, 2);
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

            //private readonly int[] fronts = new int[] { 62500, 72500, 82500, 92500 };
            //private readonly int[] rears = new int[] { 73300, 83300, 93300, 103300 };
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

            public int RideHeight(List<int> rawValue, Position position)
            {
                switch (position)
                {
                    case Position.Front: return 95 + rawValue[0];
                    case Position.Rear: return 85 + rawValue[2];
                    default: return -1;
                }
            }

            public int Splitter(int rawValue)
            {
                return rawValue;
            }
        }
    }
}
