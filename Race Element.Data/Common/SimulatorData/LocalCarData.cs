using System.Numerics;

namespace RaceElement.Data.Common.SimulatorData
{
    public sealed class LocalCarData
    {
        public CarModelData CarModel { get; set; } = new();
        public PhysicsData Physics { get; set; } = new();
        public EngineData Engine { get; set; } = new();
        public InputsData Inputs { get; set; } = new();
        public TyresData Tyres { get; set; } = new();
        public BrakesData Brakes { get; set; } = new();
        public ElectronicsData Electronics { get; set; } = new();
    }

    public class CarModelData
    {
        public string GameName { get; set; } = string.Empty;
        public int GameId { get; set; }
    }

    public sealed class EngineData
    {
        public bool IsRunning { get; set; }
        public bool IsIgnitionOn { get; set; }
        public int RPM { get; set; }
        public int MaxRPM { get; set; }
    }
    public sealed class TyresData
    {
        public float[] Velocity { get; set; } = [];
        public float[] Pressure { get; set; } = [];
        public float[] CoreTemperature { get; set; } = [];
        public float[] SurfaceTemperature { get; set; } = [];
        public float[] SlipAngle { get; set; } = [];
    }

    public sealed class BrakesData
    {
        /// <summary>
        /// The temperature in Celsius for each of the brake discs.
        /// </summary>
        public float[] DiscTemperature { get; set; } = [];

        /// <summary>
        /// The amount of pressure applied to each of the brake pads
        /// </summary>
        public float[] Pressure { get; set; } = [];
    }

    public sealed class PhysicsData
    {
        /// <summary>
        /// The global rotation of the car (yaw/pitch/roll)
        /// </summary>
        public Quaternion Rotation { get; set; }

        /// <summary>
        /// The speed of the car in kilometers per hour.
        /// </summary>
        public float Velocity { get; set; }

        /// <summary>
        /// The g-forces. (X,Y,Z)
        /// </summary>
        public Vector3 Acceleration { get; set; } = new();
    }
    public sealed class InputsData
    {
        public float Throttle { get; set; }
        public float Brake { get; set; }
        public float HandBrake { get; set; }
        public float Clutch { get; set; }

        /// <summary>
        /// -1 to 1, 0 is centered
        /// </summary>
        public float Steering { get; set; }
        public int Gear { get; set; }
    }

    public sealed class ElectronicsData
    {
        public int TractionControlLevel { get; set; }
        public int TractionControlCutLevel { get; set; }
        public float TractionControlActivation { get; set; }
        public int AbsLevel { get; set; }
        public float AbsActivation { get; set; }
        public float BrakeBias { get; set; }
    }
}
