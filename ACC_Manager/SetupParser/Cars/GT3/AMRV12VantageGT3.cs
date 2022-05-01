using ACCSetupApp.SetupParser.SetupRanges;
using System;
using System.Collections.Generic;
using static ACCSetupApp.SetupParser.ConversionFactory;
using static ACCSetupApp.SetupParser.SetupConverter;

namespace ACCSetupApp.SetupParser.Cars.GT3
{
    internal class AMRV12VantageGT3 : ICarSetupConversion, ISetupChanger
    {
        public CarModels CarModel => CarModels.Aston_Martin_Vantage_V12_GT3_2013;

        CarClasses ICarSetupConversion.CarClass => CarClasses.GT3;

        private static readonly double[] casters = new double[] { 8.3, 8.5, 8.7, 9.0, 9.2, 9.4, 9.6, 9.9, 10.1, 10.3, 10.5, 10.8, 11.0, 
                11.2, 11.4, 11.6, 11.9, 12.1, 12.3, 12.5, 12.7, 13.0, 13.2, 13.4, 13.6, 13.8, 14.0, 14.3, 14.5, 14.7, 14.9 };
        private static readonly int[] wheelRateFronts = new int[] { 115000, 120000, 125000, 130000, 135000, 140000, 145000, 150000,
                                                        155000, 160000, 165000, 170000, 175000, 180000, 185000 };
        private static readonly int[] wheelRateRears = new int[] { 95000, 100000, 110000, 115000, 120000, 125000, 130000, 135000, 140000, 145000, 150000,
                                                        155000, 160000, 165000, 170000, 175000, 180000, 185000, 190000, 195000};

        ITyreSetupChanger ISetupChanger.TyreSetupChanger => new TyreSetupChanger();
        IElectronicsSetupChanger ISetupChanger.ElectronicsSetupChanger => new ElectronicsSetupChanger();
        IMechanicalSetupChanger ISetupChanger.MechanicalSetupChanger => new MechSetupChanger();
        IAeroSetupChanger ISetupChanger.AeroSetupChanger => new AeroSetupChanger();
        IDamperSetupChanger ISetupChanger.DamperSetupChanger => new DamperSetupChanger();

        private class TyreSetupChanger : ITyreSetupChanger
        {
            public SetupDoubleRange TyrePressures => TyrePressuresGT3;
            public SetupDoubleRange CamberFront => new SetupDoubleRange(-4, -1.5, 0.1);
            public SetupDoubleRange CamberRear => new SetupDoubleRange(-3.5, -1, 0.1);
            public SetupDoubleRange ToeFront => new SetupDoubleRange(-0.4, 0.4, 0.01);
            public SetupDoubleRange ToeRear => ToeFront;
            public SetupDoubleRange Caster => new SetupDoubleRange(casters);
        }

        private class ElectronicsSetupChanger : IElectronicsSetupChanger
        {
            public SetupIntRange TractionControl => new SetupIntRange(0, 11, 1);
            public SetupIntRange ABS => new SetupIntRange(0, 11, 1);
            public SetupIntRange EcuMap => new SetupIntRange(1, 10, 1);
            public SetupIntRange TractionControlCut => new SetupIntRange(0, 11, 1);
        }

        private class MechSetupChanger : IMechanicalSetupChanger
        {
            public SetupIntRange AntiRollBarFront => new SetupIntRange(0, 6, 1);
            public SetupIntRange AntiRollBarRear => AntiRollBarFront;
            public SetupDoubleRange BrakeBias => new SetupDoubleRange(43.0, 64.0, 0.2);
            public SetupIntRange PreloadDifferential => new SetupIntRange(20, 300, 10);
            public SetupIntRange BrakePower => new SetupIntRange(80, 100, 1);
            public SetupDoubleRange SteeringRatio => new SetupDoubleRange(11, 17, 1);
            public SetupIntRange WheelRateFronts => new SetupIntRange(wheelRateFronts);
            public SetupIntRange WheelRateRears => new SetupIntRange(wheelRateRears);
            public SetupIntRange BumpstopRate => new SetupIntRange(300, 2500, 100);
            public SetupIntRange BumpstopRangeFronts => new SetupIntRange(0, 49, 1);
            public SetupIntRange BumpstopRangeRears => new SetupIntRange(0, 50, 1);
        }

        private class AeroSetupChanger : IAeroSetupChanger
        {
            public SetupIntRange RideHeightFront => new SetupIntRange(53, 85, 1);
            public SetupIntRange RideHeightRear => new SetupIntRange(55, 90, 1);
            public SetupIntRange BrakeDucts => new SetupIntRange(0, 6, 1);
            public SetupIntRange Splitter => new SetupIntRange(0, 5, 1);
            public SetupIntRange RearWing => new SetupIntRange(0, 12, 1);
        }

        private class DamperSetupChanger : IDamperSetupChanger
        {
            public SetupIntRange BumpSlow => new SetupIntRange(0, 17, 1);
            public SetupIntRange BumpFast => new SetupIntRange(0, 12, 1);
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

//            private readonly double[] casters = new double[] {
//               8.3, 8.5, 8.7, 9.0, 9.2, 9.4, 9.6, 9.9, 10.1, 10.3, 10.5, 10.8, 11.0, 11.2, 11.4, 11.6,
//              11.9, 12.1, 12.3, 12.5, 12.7, 13.0, 13.2, 13.4, 13.6, 13.8, 14.0, 14.3, 14.5, 14.7, 14.9
//         };
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

//            private readonly int[] fronts = new int[] { 115000, 120000, 125000, 130000, 135000, 140000, 145000, 150000,
//                                                        155000, 160000, 165000, 170000, 175000, 180000, 185000 };
//            private readonly int[] rears = new int[] { 95000, 100000, 110000, 115000, 120000, 125000, 130000, 135000, 140000, 145000, 150000,
//                                                        155000, 160000, 165000, 170000, 175000, 180000, 185000, 190000, 195000 };
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
}
