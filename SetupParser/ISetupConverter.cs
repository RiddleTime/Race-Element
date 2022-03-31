using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupParser
{
    public class SetupConverter
    {
        public enum CarClasses
        {
            GT3,
            GT4
        }

        public enum Wheel
        {
            FrontLeft,
            FrontRight,
            RearLeft,
            RearRight
        }

        public enum Position
        {
            Front,
            Rear
        }

        public enum TyreCompound
        {
            Dry,
            Wet
        }

        public interface ICarSetupConversion
        {
            public string CarName { get; }
            public string ParseName { get; }
            public CarClasses CarClass { get; }

            public ITyresSetup TyresSetup { get; }

            public IMechanicalSetup MechanicalSetup { get; }

            public IAeroBalance AeroBalance { get; }
        }

        public interface IMechanicalSetup
        {
            public int AntiRollBarFront(int rawValue);
            public int AntiRollBarRear(int rawValue);
            public int PreloadDifferential(int rawValue);

            public double BrakeBias(int rawValue);

            /**
             * The brake power in %
             */
            public int BrakePower(int rawValue);

            public double SteeringRatio(int rawValue);
            public int WheelRate(List<int> rawValue, Wheel wheel);

            public int BumpstopRate(List<int> rawValue, Wheel wheel);
            public int BumpstopRange(List<int> rawValue, Wheel wheel);
        }

        public interface IAeroBalance
        {
            public int RideHeight(List<int> rawValue, Position positon);
            public int BrakeDucts(int rawValue);
        }

        public interface IElectronicsSetup
        {

            public int TractionControl { get; }
            public int ABS { get; }
            public int EcuMap { get; }
            public int TractionControl2 { get; }
        }

        public interface ITyresSetup
        {
            public TyreCompound Compound(int rawValue)
            {
                switch (rawValue)
                {
                    case 0: return TyreCompound.Dry;
                    case 1: return TyreCompound.Wet;

                    default: return TyreCompound.Dry;
                }
            }

            public double TirePressure(CarClasses carClass, Wheel wheel, List<int> rawValue)
            {
                int value = rawValue[(int)wheel];

                switch (carClass)
                {
                    case CarClasses.GT3: return Math.Round(20.3f + 0.1f * value, 2);
                    case CarClasses.GT4: return Math.Round(17f + 0.1f * value, 2);

                    default: return -1;
                }
            }
            public double Toe(Wheel wheel, List<int> rawValue);
            public double Camber(Wheel wheel, List<int> rawValue);
            public double Caster(int rawValue);
        }
    }

}

