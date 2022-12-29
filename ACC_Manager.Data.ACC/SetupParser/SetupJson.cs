using System.Collections.Generic;

namespace RaceElement.Data
{
    public class SetupJson
    {
        public class Root
        {
            public string CarName { get; set; }
            public BasicSetup BasicSetup { get; set; }
            public AdvancedSetup AdvancedSetup { get; set; }
            public int TrackBopType { get; set; }
        }

        public class AdvancedSetup
        {
            public MechanicalBalance MechanicalBalance { get; set; }
            public Dampers Dampers { get; set; }
            public AeroBalance AeroBalance { get; set; }
            public Drivetrain Drivetrain { get; set; }
        }

        public class Tyres
        {
            public int TyreCompound { get; set; }
            public List<int> TyrePressure { get; set; }
        }

        public class Alignment
        {
            public List<int> Camber { get; set; }
            public List<int> Toe { get; set; }
            public List<double> StaticCamber { get; set; }
            public List<double> ToeOutLinear { get; set; }
            public int CasterLF { get; set; }
            public int CasterRF { get; set; }
            public int SteerRatio { get; set; }
        }

        public class Electronics
        {
            public int TC1 { get; set; }
            public int TC2 { get; set; }
            public int Abs { get; set; }
            public int ECUMap { get; set; }
            public int FuelMix { get; set; }
            public int TelemetryLaps { get; set; }
        }

        public class PitStrategyItem
        {
            public int FuelToAdd { get; set; }
            public Tyres Tyres { get; set; }
            public int TyreSet { get; set; }
            public int FrontBrakePadCompound { get; set; }
            public int RearBrakePadCompound { get; set; }
        }

        public class Strategy
        {
            public int Fuel { get; set; }
            public int NPitStops { get; set; }
            public int TyreSet { get; set; }
            public int FrontBrakePadCompound { get; set; }
            public int RearBrakePadCompound { get; set; }
            public List<PitStrategyItem> PitStrategy { get; set; }
            public double FuelPerLap { get; set; }
        }

        public class BasicSetup
        {
            public Tyres Tyres { get; set; }
            public Alignment Alignment { get; set; }
            public Electronics Electronics { get; set; }
            public Strategy Strategy { get; set; }
        }

        public class MechanicalBalance
        {
            public int ARBFront { get; set; }
            public int ARBRear { get; set; }
            public List<int> WheelRate { get; set; }
            public List<int> BumpStopRateUp { get; set; }
            public List<int> BumpStopRateDn { get; set; }
            public List<int> BumpStopWindow { get; set; }
            public int BrakeTorque { get; set; }
            public int BrakeBias { get; set; }
        }

        public class Dampers
        {
            public List<int> BumpSlow { get; set; }
            public List<int> BumpFast { get; set; }
            public List<int> ReboundSlow { get; set; }
            public List<int> ReboundFast { get; set; }
        }

        public class AeroBalance
        {
            public List<int> RideHeight { get; set; }
            public List<double> RodLength { get; set; }
            public int Splitter { get; set; }
            public int RearWing { get; set; }
            public List<int> BrakeDuct { get; set; }
        }

        public class Drivetrain
        {
            public int Preload { get; set; }
        }
    }
}
