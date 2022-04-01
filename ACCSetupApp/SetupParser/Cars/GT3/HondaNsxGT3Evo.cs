using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.SetupParser.SetupConverter;

namespace ACCSetupApp.SetupParser.Cars.GT3
{
    public class HondaNsxGT3Evo : ICarSetupConversion
    {
        public string CarName => "Honda NSX GT3 Evo";
        public string ParseName => "honda_nsx_gt3_evo";
        public CarClasses CarClass => CarClasses.GT3;

        public AbstractTyresSetup TyresSetup => new TyreSetup();
        private class TyreSetup : AbstractTyresSetup
        {
            public override double Camber(Wheel wheel, List<int> rawValue)
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
                    case Position.Front: return Math.Round(-5 + 0.1 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(-5 + 0.1 * rawValue[(int)wheel], 2);
                    default: return -1;
                }
            }

            public override double Caster(int rawValue)
            {
                double[] casters = new double[] { 7.2, 7.4, 7.6, 7.8, 8.0, 8.2, 8.4, 8.6, 8.8, 8.9, 9.1 };

                return Math.Round(casters[rawValue], 2);
            }

            public override double Toe(Wheel wheel, List<int> rawValue)
            {
                return Math.Round(-0.4 + 0.01 * rawValue[(int)wheel], 2);
            }
        }

        public IMechanicalSetup MechanicalSetup => new MechSetup();
        private class MechSetup : IMechanicalSetup
        {
            public int AntiRollBarFront(int rawValue)
            {
                return 1 + rawValue;
            }

            public int AntiRollBarRear(int rawValue)
            {
                return 1 + rawValue;
            }

            public double BrakeBias(int rawValue)
            {
                return Math.Round(44 + 0.2 * rawValue, 2);
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
                return 200 + 50 * rawValue[(int)wheel];
            }

            public int PreloadDifferential(int rawValue)
            {
                return 20 + rawValue * 10;
            }

            public double SteeringRatio(int rawValue)
            {
                return Math.Round(10d + rawValue, 2);
            }

            public int WheelRate(List<int> rawValue, Wheel wheel)
            {
                int[] front = new int[] { 73000, 79080, 85160, 91240, 97320, 103400, 109480, 115560, 121640,
                    127720, 133800, 139880, 145960, 152040, 158120, 164200, 170280 };
                int[] rear = new int[] { 126800, 134700, 142600, 150500, 158400, 166300, 174200, 182100,
                    190000, 197900, 205800, 213700, 221600, 229500, 237400, 245300, 253200 };

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

            public int RearWing(int rawValue)
            {
                throw new NotImplementedException();
            }

            public int RideHeight(List<int> rawValue, Position position)
            {
                switch (position)
                {
                    case Position.Front: return 54 + rawValue[0];
                    case Position.Rear: return 54 + rawValue[2];
                    default: return -1;
                }
            }

            public int Splitter(int rawValue)
            {
                throw new NotImplementedException();
            }
        }
    }
}
