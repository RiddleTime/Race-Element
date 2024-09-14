using System.Numerics;

namespace RaceElement.Data.Common.SimulatorData
{
    public sealed record LocalCarData
    {
        public CarModelData CarModel { get; set; } = new();
        public PhysicsData Physics { get; set; } = new();
        public EngineData Engine { get; set; } = new();
        public InputsData Inputs { get; set; } = new();
        public TyresData Tyres { get; set; } = new();
        public BrakesData Brakes { get; set; } = new();
        public ElectronicsData Electronics { get; set; } = new();

        public RaceData Race { get; set; } = new();
    }

    public sealed record CarModelData
    {
        public string GameName { get; set; } = string.Empty;
        public int GameId { get; set; }
        public string CarClass { get; set; }
    }

    public sealed record EngineData
    {
        public bool IsRunning { get; set; }
        public bool IsIgnitionOn { get; set; }
        public bool IsPitLimiterOn { get; set; }

        /// <summary>
        /// Current revolutions per minute
        /// </summary>
        public int Rpm { get; set; }

        /// <summary>
        /// Maximum revolutions per minute
        /// </summary>
        public int MaxRpm { get; set; }

        // Fuel info
        public float FuelLiters { get; set; }
        public float MaxFuelLiters { get; set; }
        public float FuelLitersXLap { get; set; }
        public float FuelEstimatedLaps { get; set; }
    }
    public sealed record TyresData
    {
        public float[] Velocity { get; set; } = [];
        /// <summary>
        /// Tyre Pressures in Bar (FL, FR, RL, RR).
        /// </summary>
        public float[] Pressure { get; set; } = [];
        public float[] CoreTemperature { get; set; } = [];
        public float[] SurfaceTemperature { get; set; } = [];
        public float[] SlipAngle { get; set; } = [];
    }

    public sealed record BrakesData
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

    public sealed record PhysicsData
    {
        /// <summary>
        /// The location of the local car. (X,Y,Z)
        /// </summary>
        public Vector3 Location { get; set; } = new();

        /// <summary>
        /// The g-forces. (X,Y,Z)
        /// </summary>
        public Vector3 Acceleration { get; set; } = new();

        /// <summary>
        /// The global rotation of the car (yaw/pitch/roll)
        /// </summary>
        public Quaternion Rotation { get; set; } = Quaternion.Zero;

        /// <summary>
        /// The global rotation of the car (X:yaw/heading, Y:pitch, Z:roll), in euler angles
        /// </summary>
        public Vector3 RotationEuler => Vector3.Transform(new(), Rotation);

        /// <summary>
        /// The speed of the car in kilometers per hour.
        /// </summary>
        public float Velocity { get; set; }
    }
    public sealed record InputsData
    {
        public float Throttle { get; set; }
        public float Brake { get; set; }
        public float HandBrake { get; set; }
        public float Clutch { get; set; }

        /// <summary>
        /// -1 to 1, 0 is centered
        /// </summary>
        public float Steering { get; set; }

        /// <summary>
        /// The maximum steering angle(degrees) lock-to-lock for the current car.
        /// </summary>
        public float MaxSteeringAngle { get; set; }
        public int Gear { get; set; }
    }

    public sealed record ElectronicsData
    {
        public int TractionControlLevel { get; set; }
        public int TractionControlCutLevel { get; set; }

        /// <summary>
        /// 0 is no activation, 1 is full activation.
        /// </summary>
        public float TractionControlActivation { get; set; }

        public int AbsLevel { get; set; }

        /// <summary>
        /// 0 is no activation, 1 is full activation.
        /// </summary>
        public float AbsActivation { get; set; }
        public float BrakeBias { get; set; }
    }

    public sealed record RaceData
    {
        public int LapsDriven { get; set; }
        public float LapPositionPercentage { get; set; }
        public int CarNumber { get; set; }
        public int GlobalPosition { get; set; }
        public int ClassPosition { get; set; }
    }
}
