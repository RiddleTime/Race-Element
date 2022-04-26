using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.SetupParser.ConversionFactory;
using static ACCSetupApp.SetupParser.SetupConverter;

namespace ACCSetupApp.SetupParser.Cars.TCX
{
    internal class BMWM2CSRacing : ICarSetupConversion
    {
        public CarModels CarModel => CarModels.BMW_M2_Cup_2020;

        CarClasses ICarSetupConversion.CarClass => CarClasses.TCX;

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

            private readonly double[] casters = new double[] { 8.5 };
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

            private readonly int[] fronts = new int[] { 162000, 180000, 198000 };
            private readonly int[] rears = new int[] { 103000, 117000, 131000 };
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
}
