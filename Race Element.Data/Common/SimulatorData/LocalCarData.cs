using System.Numerics;

namespace RaceElement.Data.Common.SimulatorData
{
    public sealed record LocalCarData
    {
        public CarModelData CarModel { get; internal set; } = new();
        public PhysicsData Physics { get; internal set; } = new();
        public EngineData Engine { get; internal set; } = new();
        public InputsData Inputs { get; internal set; } = new();
        public TyresData Tyres { get; internal set; } = new();
        public BrakesData Brakes { get; internal set; } = new();
        public ElectronicsData Electronics { get; internal set; } = new();

        public RaceData Race { get; internal set; } = new();

        public TimingData Timing { get; internal set; } = new();
    }

    public sealed record CarModelData
    {
        public string GameName { get; internal set; } = string.Empty;
        public int GameId { get; internal set; }
        public string CarClass { get; internal set; }
    }

    public sealed record EngineData
    {
        public bool IsRunning { get; internal set; }
        public bool IsIgnitionOn { get; internal set; }
        public bool IsPitLimiterOn { get; internal set; }

        /// <summary>
        /// Current revolutions per minute
        /// </summary>
        public int Rpm { get; internal set; }

        /// <summary>
        /// Maximum revolutions per minute
        /// </summary>
        public int MaxRpm { get; internal set; }

        // Fuel info
        public float FuelLiters { get; internal set; }
        public float MaxFuelLiters { get; internal set; }
        public float FuelLitersXLap { get; internal set; }
        public float FuelEstimatedLaps { get; internal set; }
    }
    public sealed record TyresData
    {
        /// <summary>
        /// Velocity in kilometers per second.
        /// </summary>
        public float[] Velocity { get; internal set; } = [];
        /// <summary>
        /// Tyre Pressures in Bar (FL, FR, RL, RR).
        /// </summary>
        public float[] Pressure { get; internal set; } = [];
        public float[] CoreTemperature { get; internal set; } = [];
        public float[] SurfaceTemperature { get; internal set; } = [];

        /// <summary>
        /// Wheel slip angle
        /// </summary>
        public float[] SlipAngle { get; internal set; } = [];

        /// <summary>
        /// Wheel Slip Ratio
        /// </summary>
        public float[] SlipRatio { get; internal set; } = [];
    }

    public sealed record BrakesData
    {
        /// <summary>
        /// The temperature in Celsius for each of the brake discs.
        /// </summary>
        public float[] DiscTemperature { get; internal set; } = [];

        /// <summary>
        /// The amount of pressure applied to each of the brake pads
        /// </summary>
        public float[] Pressure { get; internal set; } = [];
    }

    public sealed record PhysicsData
    {
        /// <summary>
        /// The location of the local car. (X,Y,Z)
        /// </summary>
        public Vector3 Location { get; internal set; } = new();

        /// <summary>
        /// The g-forces. (X,Y,Z)
        /// </summary>
        public Vector3 Acceleration { get; internal set; } = new();

        /// <summary>
        /// The global rotation of the car (yaw/pitch/roll)
        /// </summary>
        public Quaternion Rotation { get; internal set; } = Quaternion.Zero;

        /// <summary>
        /// The global rotation of the car (X:yaw/heading, Y:pitch, Z:roll), in euler angles
        /// </summary>
        public Vector3 RotationEuler => Vector3.Transform(Vector3.Zero, Rotation);

        /// <summary>
        /// The speed of the car in kilometers per hour.
        /// </summary>
        public float Velocity { get; internal set; }
    }
    public sealed record InputsData
    {
        public float Throttle { get; internal set; }
        public float Brake { get; internal set; }
        public float HandBrake { get; internal set; }
        public float Clutch { get; internal set; }

        /// <summary>
        /// -1 to 1, 0 is centered
        /// </summary>
        public float Steering { get; internal set; }

        /// <summary>
        /// The maximum steering angle(degrees) lock-to-lock for the current car.
        /// </summary>
        public float MaxSteeringAngle { get; internal set; }

        /// <summary>
        /// Current Gear, 0 = reverse, 1 = neutral etc.
        /// </summary>
        public int Gear { get; internal set; }
    }

    public sealed record ElectronicsData
    {
        public int TractionControlLevel { get; internal set; }
        public int TractionControlCutLevel { get; internal set; }

        /// <summary>
        /// 0 is no activation, 1 is full activation.
        /// </summary>
        public float TractionControlActivation { get; internal set; }

        public int AbsLevel { get; internal set; }

        /// <summary>
        /// 0 is no activation, 1 is full activation.
        /// </summary>
        public float AbsActivation { get; internal set; }
        public float BrakeBias { get; internal set; }
    }

    public sealed record RaceData
    {
        public int LapsDriven { get; internal set; }
        public float LapPositionPercentage { get; internal set; }
        public int CarNumber { get; internal set; }
        public int GlobalPosition { get; internal set; }
        public int ClassPosition { get; internal set; }
    }

    public sealed record TimingData
    {

        /// <summary>
        /// Current laptime, -1 is invalid;
        /// </summary>
        public int CurrentLaptimeMS { get; internal set; } = 0;

        /// <summary>
        /// Delta to best laptime in milliseconds
        /// </summary>
        public int LapTimeDeltaBestMS { get; internal set; } = 0;

        /// <summary>
        /// Current best lap, -1 is invalid.
        /// </summary>
        public int LapTimeBestMs { get; internal set; } = -1;

        public bool HasLapTimeBest { get; internal set; } = false;

        public bool IsLapValid { get; internal set; } = true;
    }
}
