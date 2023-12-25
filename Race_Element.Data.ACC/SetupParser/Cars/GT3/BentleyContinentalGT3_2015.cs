using System;
using System.Collections.Generic;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.Cars.GT3;

internal class BentleyContinentalGT3_2015 : ICarSetupConversion
{
    public CarModels CarModel => CarModels.Bentley_Continental_GT3_2015;

    CarClasses ICarSetupConversion.CarClass => CarClasses.GT3;
    public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHF2023;

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

        private readonly double[] casters = [ 8.3, 8.5, 8.8, 9.0, 9.2, 9.5, 9.7, 10.0,
            10.2, 10.5, 10.7, 10.9, 11.2, 11.4, 11.7, 11.9, 12.1, 12.4, 12.6, 12.9, 13.1, 13.3, 13.6,
            13.8, 14.0, 14.3, 14.5, 14.8, 15.0, 15.2, 15.5 ];
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
            return Math.Round(57 + 0.2 * rawValue, 2);
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
            return Math.Round(10d + rawValue, 2);
        }

        private readonly int[] fronts = [ 115000, 120000, 125000, 130000,
            135000, 140000, 145000, 150000, 155000, 160000, 165000, 170000, 175000,
            180000, 185000 ];
        private readonly int[] rears = [ 95000, 100000, 105000, 110000, 115000,
            120000,125000, 130000, 135000, 140000, 145000, 150000, 155000, 160000, 165000, 170000,
            175000, 180000, 185000, 190000, 195000 ];
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
                case Position.Front: return 54 + rawValue[0];
                case Position.Rear: return 54 + rawValue[2];
                default: return -1;
            }
        }

        public int Splitter(int rawValue)
        {
            return rawValue;
        }
    }
}
