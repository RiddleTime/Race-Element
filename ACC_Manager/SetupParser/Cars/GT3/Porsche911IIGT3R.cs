using System;
using System.Collections.Generic;
using static ACCSetupApp.SetupParser.SetupConverter;

namespace ACCSetupApp.SetupParser.Cars.GT3
{
    internal class Porsche911IIGT3R : ICarSetupConversion
    {
        public string CarName => "Porsche 911 II GT3 R 2019";
        public string ParseName => "porsche_991ii_gt3_r";
        CarClasses ICarSetupConversion.CarClass => CarClasses.GT3;

        AbstractTyresSetup ICarSetupConversion.TyresSetup => new TyreSetup();
        private class TyreSetup : AbstractTyresSetup
        {
            public override double Toe(Wheel wheel, List<int> rawValue)
            {
                return Math.Round(-0.4 + 0.01 * rawValue[(int)wheel], 2);
            }

            public override double Camber(Wheel wheel, List<int> rawValue)
            {
                switch (GetPosition(wheel))
                {
                    case Position.Front: return Math.Round(-4 + 0.1 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(-3.5 + 0.1 * rawValue[(int)wheel], 2);
                    default: return -1;
                }
            }

            private readonly double[] casters = new double[] { 4.4, 4.6, 4.8, 5.1, 5.3, 5.5, 5.7, 5.9, 6.1, 6.3, 6.5,
                6.7, 6.9, 7.1, 7.3, 7.5, 7.7, 7.8, 8.0, 8.2, 8.4, 8.6, 8.8, 9.0, 9.2, 9.4, 9.6, 9.8, 10.0, 10.2,
                10.4, 10.6, 10.8, 11.0, 11.2, 11.4, 11.6, 11.8, 12.0, 12.2, 12.4 };
            public override double Caster(int rawValue)
            {
                return Math.Round(casters[rawValue], 2);
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
                return Math.Round(43 + 0.2 * rawValue, 2);
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
                return Math.Round(11d + rawValue, 2);
            }

            private readonly int[] fronts = new int[] { 100500, 110000, 114000, 119000, 127000, 137000, 141500, 146000, 155000, 173500 };
            private readonly int[] rears = new int[] { 137000, 149500, 156000, 162000, 174500, 187000, 193000, 199500, 212000, 237000 };
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
                    case Position.Front: return 53 + rawValue[0];
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
