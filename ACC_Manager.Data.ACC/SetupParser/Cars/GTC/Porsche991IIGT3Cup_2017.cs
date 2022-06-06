using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.Data.ConversionFactory;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.Data.Cars.GTC
{
    internal class Porsche991IIGT3Cup_2017 : ICarSetupConversion
    {
        public CarModels CarModel => CarModels.Porsche_991_II_GT3_Cup_2017;

        CarClasses ICarSetupConversion.CarClass => CarClasses.CUP; // did this for tyre pressures, should be 'GTC' 
        public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHE2020;


        AbstractTyresSetup ICarSetupConversion.TyresSetup => new TyreSetup();
        private class TyreSetup : AbstractTyresSetup
        {
            public override double Camber(Wheel wheel, List<int> rawValue)
            {
                switch (GetPosition(wheel))
                {
                    case Position.Front: return Math.Round(-6.5 + 0.1 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(-5 + 0.1 * rawValue[(int)wheel], 2);
                    default: return -1;
                }
            }

            private readonly double[] casters = new double[] { 7.3, 7.4, 7.5, 7.6, 7.7, 7.8,
                7.9, 8.0, 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 8.8, 8.9, 9.0, 9.1, 9.2,
                9.3, 9.4, 9.5, 9.6, 9.7, 9.8, 9.9, 10.0, 10.1, 10.2, 10.3 };
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
                return rawValue[(int)wheel];
            }

            public int BumpstopRate(List<int> rawValue, Wheel wheel)
            {
                return 1000 + 0 * rawValue[(int)wheel];
            }

            public int PreloadDifferential(int rawValue)
            {
                return 0 + rawValue * 10;
            }

            public double SteeringRatio(int rawValue)
            {
                return Math.Round(11d + rawValue, 2);
            }

            private readonly int[] fronts = new int[] { 0, 1, 2, 3 };
            private readonly int[] rears = new int[] { 0, 1, 2, 3 };
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
                return rawValue + 1;
            }

            public int RideHeight(List<int> rawValue, Position position)
            {
                switch (position)
                {
                    case Position.Front: return 60 + rawValue[0];
                    case Position.Rear: return 80 + rawValue[2];
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
