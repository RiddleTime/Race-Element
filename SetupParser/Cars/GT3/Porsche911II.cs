using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SetupParser.SetupConverter;

namespace SetupParser.Cars.GT3
{
    public class Porsche911II : ICarSetupConversion
    {
        public string CarName => "Porsche 911.2 GT3";
        public string ParseName => "porsche_991ii_gt3_r";
        public CarClasses CarClass => CarClasses.GT3;

        public ITyresSetup TyresSetup => new TyreSetup();
        private class TyreSetup : ITyresSetup
        {
            public double Toe(Wheel wheel, List<int> rawValue)
            {
                return Math.Round(0.01 * rawValue[(int)wheel] - 0.4, 2);
            }

            public double Camber(Wheel wheel, List<int> rawValue)
            {
                Position position;
                switch (wheel)
                {
                    case Wheel.FrontLeft:
                        position = Position.Front;
                        break;
                    case Wheel.FrontRight:
                        position = Position.Front;
                        break;

                    default:
                        position = Position.Rear;
                        break;
                }

                switch (position)
                {
                    case Position.Front: return Math.Round(-6.5 + 0.1 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(-5 + 0.1 * rawValue[(int)wheel], 2);
                    default: return -1;
                }
            }
            public double Caster(int rawValue)
            {
                return Math.Round(4.4 + 0.2 * rawValue, 3);
            }
        }

        public IMechanicalSetup MechanicalSetup => new MechSetup();
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

            public int WheelRate(List<int> rawValue, Wheel wheel)
            {
                int[] front = new int[] { 100500, 110000, 114000, 119000, 127000, 137000, 141500, 146000, 155000, 173500 };
                int[] rear = new int[] { 137000, 149500, 156000, 162000, 174500, 187000, 193000, 199500, 212000, 237000 };

                Position position;
                switch (wheel)
                {
                    case Wheel.FrontLeft:
                        position = Position.Front;
                        break;
                    case Wheel.FrontRight:
                        position = Position.Front;
                        break;

                    default:
                        position = Position.Rear;
                        break;
                }

                switch (position)
                {
                    case Position.Front: return front[rawValue[(int)wheel]];
                    case Position.Rear: return rear[rawValue[(int)wheel]];
                    default: return -1;
                }
            }
        }

        public IAeroBalance AeroBalance => new AeroSetup();
        private class AeroSetup : IAeroBalance
        {
            public int BrakeDucts(int rawValue)
            {
                return rawValue;
            }

            public int RideHeight(List<int> rawValue, Position wheel)
            {
                switch (wheel)
                {
                    case Position.Front: return 53 + rawValue[0];
                    case Position.Rear: return 55 + rawValue[2];
                    default: return -1;
                }
            }
        }

    }
}
