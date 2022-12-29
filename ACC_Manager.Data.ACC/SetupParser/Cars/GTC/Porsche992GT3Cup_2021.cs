using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.Cars.GTC
{
    internal class Porsche992GT3Cup_2021 : ICarSetupConversion
    {
        public CarModels CarModel => CarModels.Porsche_992_GT3_Cup_2021;

        CarClasses ICarSetupConversion.CarClass => CarClasses.CUP; // did this for tyre pressures, should be 'GTC' 
        public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHE2020;


        AbstractTyresSetup ICarSetupConversion.TyresSetup => new TyreSetup();
        private class TyreSetup : AbstractTyresSetup
        {
            public override double Camber(Wheel wheel, List<int> rawValue)
            {
                switch (GetPosition(wheel))
                {
                    case Position.Front: return Math.Round(-5 + 0.1 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(-4.4 + 0.1 * rawValue[(int)wheel], 2);
                    default: return -1;
                }
            }

            private readonly double[] casters = new double[] { 14.8 };
            public override double Caster(int rawValue)
            {
                return Math.Round(casters[rawValue], 2);
            }

            public override double Toe(Wheel wheel, List<int> rawValue)
            {

                switch (GetPosition(wheel))
                {
                    case Position.Front: return Math.Round(-0.4 + 0.01 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(-0.4 + 0.01 * rawValue[(int)wheel], 2);
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
                return Math.Round(50 + 0.2 * rawValue, 2);
            }

            public int BrakePower(int rawValue)
            {
                return 80 + rawValue;
            }

            public int BumpstopRange(List<int> rawValue, Wheel wheel)
            {
                return rawValue[(int)wheel] + 1;
            }

            public int BumpstopRate(List<int> rawValue, Wheel wheel)
            {
                return 1 + 0 * rawValue[(int)wheel];
            }

            public int PreloadDifferential(int rawValue)
            {
                return 0 + rawValue * 10;
            }

            public double SteeringRatio(int rawValue)
            {
                return Math.Round(11d + rawValue, 2);
            }

            private readonly int[] fronts = new int[] { 1 };
            private readonly int[] rears = new int[] { 1 };
            public int WheelRate(List<int> rawValue, Wheel wheel)
            {
                switch (GetPosition(wheel))
                {
                    case Position.Front: return fronts[rawValue[(int)wheel]];
                    case Position.Rear: return rears[rawValue[(int)wheel]];
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
                return rawValue[(int)wheel] + 1;
            }

            int IDamperSetup.BumpSlow(List<int> rawValue, Wheel wheel)
            {
                return rawValue[(int)wheel] + 1;
            }

            int IDamperSetup.ReboundFast(List<int> rawValue, Wheel wheel)
            {
                return rawValue[(int)wheel] + 1;
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
                    case Position.Front: return 72 + rawValue[0];
                    case Position.Rear: return 106 + rawValue[2];
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
