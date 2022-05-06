using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.Data.ConversionFactory;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.Data.Cars.GT3
{
    internal class NissanGtrGT3_2015 : ICarSetupConversion
    {
        public CarModels CarModel => CarModels.Nissan_GT_R_Nismo_GT3_2015;

        CarClasses ICarSetupConversion.CarClass => CarClasses.GT3;

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


            private readonly double[] casters = new double[] { 6.0, 6.2, 6.4, 6.6, 6.8, 7.0, 7.2,
                7.3, 7.5, 7.7, 7.9, 8.1, 8.3, 8.5, 8.7, 8.9, 9.1, 9.3, 9.5, 9.7, 9.8, 10.0, 10.2,
                10.4, 10.6, 10.8, 11.0, 11.2, 11.4, 11.6, 11.7, 11.9, 12.1, 12.3, 12.5, 12.7, 12.9,
                13.1, 13.2, 13.4, 13.6, 13.8, 14.0, 14.2, 14.4, 14.5, 14.7, 14.9, 15.1, 15.3, 15.5,
                15.6, 15.8, 16.0, 16.2, 16.4, 16.5, 16.7, 16.9, 17.1, 17.3 };
            public override double Caster(int rawValue)
            {
                return Math.Round(casters[rawValue], 2);
            }

            public override double Toe(Wheel wheel, List<int> rawValue)
            {

                switch (GetPosition(wheel))
                {
                    case Position.Front: return Math.Round(-0.2 + 0.01 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(0.0 + 0.01 * rawValue[(int)wheel], 2);
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
                return Math.Round(47.5 + 0.3 * rawValue, 2);
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
                return Math.Round(12d + rawValue, 2);
            }

            private readonly int[] fronts = new int[] { 122000, 132000, 142000, 152000, 162000, 172000, 182000 };
            private readonly int[] rears = new int[] { 94000, 104000, 114000, 124000, 134000, 144000, 154000 };
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
