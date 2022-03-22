using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupConverter
{
    public class SetupJson
    {

        public class Root
        {
            public string carName { get; set; }
            public BasicSetup basicSetup { get; set; }
            public AdvancedSetup advancedSetup { get; set; }
            public int trackBopType { get; set; }
        }

        public class AdvancedSetup
        {
            public MechanicalBalance mechanicalBalance { get; set; }
            public Dampers dampers { get; set; }
            public AeroBalance aeroBalance { get; set; }
            public Drivetrain drivetrain { get; set; }
        }

        public class Tyres
        {
            public int tyreCompound { get; set; }
            public List<int> tyrePressure { get; set; }
        }

        public class Alignment
        {
            public List<int> camber { get; set; }
            public List<int> toe { get; set; }
            public List<double> staticCamber { get; set; }
            public List<double> toeOutLinear { get; set; }
            public int casterLF { get; set; }
            public int casterRF { get; set; }
            public int steerRatio { get; set; }
        }

        public class Electronics
        {
            public int tC1 { get; set; }
            public int tC2 { get; set; }
            public int abs { get; set; }
            public int eCUMap { get; set; }
            public int fuelMix { get; set; }
            public int telemetryLaps { get; set; }
        }

        public class PitStrategyItem
        {
            public int fuelToAdd { get; set; }
            public Tyres tyres { get; set; }
            public int tyreSet { get; set; }
            public int frontBrakePadCompound { get; set; }
            public int rearBrakePadCompound { get; set; }
        }

        public class Strategy
        {
            public int fuel { get; set; }
            public int nPitStops { get; set; }
            public int tyreSet { get; set; }
            public int frontBrakePadCompound { get; set; }
            public int rearBrakePadCompound { get; set; }
            public List<PitStrategyItem> pitStrategy { get; set; }
            public double fuelPerLap { get; set; }
        }

        public class BasicSetup
        {
            public Tyres tyres { get; set; }
            public Alignment alignment { get; set; }
            public Electronics electronics { get; set; }
            public Strategy strategy { get; set; }
        }

        public class MechanicalBalance
        {
            public int aRBFront { get; set; }
            public int aRBRear { get; set; }
            public List<int> wheelRate { get; set; }
            public List<int> bumpStopRateUp { get; set; }
            public List<int> bumpStopRateDn { get; set; }
            public List<int> bumpStopWindow { get; set; }
            public int brakeTorque { get; set; }
            public int brakeBias { get; set; }
        }

        public class Dampers
        {
            public List<int> bumpSlow { get; set; }
            public List<int> bumpFast { get; set; }
            public List<int> reboundSlow { get; set; }
            public List<int> reboundFast { get; set; }
        }

        public class AeroBalance
        {
            public List<int> rideHeight { get; set; }
            public List<double> rodLength { get; set; }
            public int splitter { get; set; }
            public int rearWing { get; set; }
            public List<int> brakeDuct { get; set; }
        }

        public class Drivetrain
        {
            public int preload { get; set; }
        }
    }
}
