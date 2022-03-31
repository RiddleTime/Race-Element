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
            string CarName { get; }
            string ParseName { get; }
            CarClasses CarClass { get; }

            AbstractTyresSetup TyresSetup { get; }

            IMechanicalSetup MechanicalSetup { get; }

            IAeroBalance AeroBalance { get; }
        }

        public interface IMechanicalSetup
        {
            int AntiRollBarFront(int rawValue);
            int AntiRollBarRear(int rawValue);
            int PreloadDifferential(int rawValue);

            double BrakeBias(int rawValue);

            /**
             * The brake power in %
             */
            int BrakePower(int rawValue);

            double SteeringRatio(int rawValue);
            int WheelRate(List<int> rawValue, Wheel wheel);

            int BumpstopRate(List<int> rawValue, Wheel wheel);
            int BumpstopRange(List<int> rawValue, Wheel wheel);
        }

        public interface IAeroBalance
        {
            int RideHeight(List<int> rawValue, Position positon);
            int BrakeDucts(int rawValue);
        }

        public interface IElectronicsSetup
        {

            int TractionControl { get; }
            int ABS { get; }
            int EcuMap { get; }
            int TractionControl2 { get; }
        }

        public abstract class AbstractTyresSetup
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
            public abstract double Toe(Wheel wheel, List<int> rawValue);
            public abstract double Camber(Wheel wheel, List<int> rawValue);
            public abstract double Caster(int rawValue);
        }
    }

}

