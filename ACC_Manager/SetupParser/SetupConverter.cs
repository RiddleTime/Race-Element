using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.SetupParser.ConversionFactory;

namespace ACCSetupApp.SetupParser
{
    public class SetupConverter
    {
        public enum CarClasses
        {
            GT3,
            GT4,
            TCX,
            GTC
        }

        public enum Wheel : int
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

        public static Position GetPosition(Wheel wheel)
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
            return position;
        }

        public interface ICarSetupConversion
        {
            CarModels CarModel { get; }
            CarClasses CarClass { get; }

            AbstractTyresSetup TyresSetup { get; }

            IDamperSetup DamperSetup { get; }

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

        public interface IDamperSetup
        {
            int BumpSlow(List<int> rawValue, Wheel wheel);
            int BumpFast(List<int> rawValue, Wheel wheel);
            int ReboundSlow(List<int> rawValue, Wheel wheel);
            int ReboundFast(List<int> rawValue, Wheel wheel);
        }

        internal static IDamperSetup DefaultDamperSetup = new DefaultDamperSetupImplementation();
        private class DefaultDamperSetupImplementation : IDamperSetup
        {
            int IDamperSetup.BumpFast(List<int> rawValue, Wheel wheel)
            {
                return rawValue[(int)wheel];
            }

            int IDamperSetup.BumpSlow(List<int> rawValue, Wheel wheel)
            {
                return rawValue[(int)wheel];
            }

            int IDamperSetup.ReboundFast(List<int> rawValue, Wheel wheel)
            {
                return rawValue[(int)wheel];
            }

            int IDamperSetup.ReboundSlow(List<int> rawValue, Wheel wheel)
            {
                return rawValue[(int)wheel];
            }
        }

        public interface IAeroBalance
        {
            int RideHeight(List<int> rawValue, Position position);
            int BrakeDucts(int rawValue);
            int RearWing(int rawValue);
            int Splitter(int rawValue);
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
                switch (carClass)
                {
                    case CarClasses.GT3: return Math.Round(20.3f + 0.1f * rawValue[(int)wheel], 2);
                    case CarClasses.GTC:
                    case CarClasses.GT4: return Math.Round(17.0f + 0.1f * rawValue[(int)wheel], 2);

                    default: return -1;
                }
            }
            public abstract double Toe(Wheel wheel, List<int> rawValue);
            public abstract double Camber(Wheel wheel, List<int> rawValue);
            public abstract double Caster(int rawValue);
        }
    }

}

