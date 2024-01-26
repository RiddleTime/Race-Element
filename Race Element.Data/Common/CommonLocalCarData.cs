using System.Numerics;

namespace RaceElement.Data.Common
{
    public class CommonLocalCarData
    {
        /// <summary>
        /// Provides information about the car Model
        /// </summary>
        public CommonModelData Model { get; set; } = new();
        public CommonPhysicsData Physics { get; set; } = new();
        public CommonEngineData Engine { get; set; } = new();
        public CommonInputsData Inputs { get; set; } = new();
        public CommonWheelData Tyres { get; set; } = new();
        public CommonBrakesData BrakesData { get; set; } = new();
        public CommonElectronicsData Electronics { get; set; } = new();
    }

    public class CommonModelData
    {
        public string GameName { get; set; } = string.Empty;
        public int GameId { get; set; }
    }

    public class CommonEngineData
    {
        public int RPM { get; set; }
        public int MaxRPM { get; set; }
    }
    public class CommonWheelData
    {
        public float[] Velocity { get; set; } = [];
        public float[] Pressure { get; set; } = [];
        public float[] CoreTemperature { get; set; } = [];
        public float[] SurfaceTemperature { get; set; } = [];
        public float[] SlipAngle { get; set; } = [];
    }

    public class CommonBrakesData
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

    public class CommonPhysicsData
    {
        /// <summary>
        /// The speed of the car in kilometers per hour.
        /// </summary>
        public float Velocity { get; set; }

        /// <summary>
        /// The g-forces. (X,Y,Z)
        /// </summary>
        public Vector3 Acceleration { get; set; } = new();
    }
    public class CommonInputsData
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

    public class CommonElectronicsData
    {
        public int TractionControlLevel { get; set; }
        public int TractionControlCutLevel { get; set; }
        public float TractionControlActivation { get; set; }
        public int AbsLevel { get; set; }
        public float AbsActivation { get; set; }
        public float BrakeBias { get; set; }
    }
}
