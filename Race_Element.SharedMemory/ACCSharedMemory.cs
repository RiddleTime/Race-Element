using RaceElement.Controls.Telemetry.SharedMemory;
using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace RaceElement;

/// <summary>
/// Used certain shared memory from https://github.com/gro-ove/actools
/// </summary>
public sealed unsafe class ACCSharedMemory
{
    private readonly string physicsMap = "Local\\acpmf_physics";
    private readonly string graphicsMap = "Local\\acpmf_graphics";
    private readonly string staticMap = "Local\\acpmf_static";

    public SPageFileStatic PageFileStatic { get; private set; }
    public SPageFilePhysics PageFilePhysics { get; private set; }
    public SPageFileGraphic PageFileGraphic { get; private set; }

    private static ACCSharedMemory _instance;
    public static ACCSharedMemory Instance
    {
        get
        {
            _instance ??= new ACCSharedMemory();

            return _instance;
        }
    }

    private ACCSharedMemory()
    {
        ReadStaticPageFile();
        ReadPhysicsPageFile();
        ReadGraphicsPageFile();
    }

    public enum AcStatus : int
    {
        AC_OFF,
        AC_REPLAY,
        AC_LIVE,
        AC_PAUSE,
    }

    public enum AcSessionType : int
    {
        AC_UNKNOWN = -1,
        AC_PRACTICE = 0,
        AC_QUALIFY = 1,
        AC_RACE = 2,
        AC_HOTLAP = 3,
        AC_TIME_ATTACK = 4,
        AC_DRIFT = 5,
        AC_DRAG = 6,
        AC_HOTSTINT = 7,
        AC_HOTLAPSUPERPOLE = 8
    }


    public static string SessionTypeToString(AcSessionType sessionType) => sessionType switch
    {
        AcSessionType.AC_UNKNOWN => "Unknown",
        AcSessionType.AC_PRACTICE => "Practice",
        AcSessionType.AC_QUALIFY => "Qualify",
        AcSessionType.AC_RACE => "Race",
        AcSessionType.AC_HOTLAP => "Hotlap",
        AcSessionType.AC_TIME_ATTACK => "Time attack",
        AcSessionType.AC_DRIFT => "Drift",
        AcSessionType.AC_DRAG => "Drag",
        AcSessionType.AC_HOTSTINT => "Hotstint",
        AcSessionType.AC_HOTLAPSUPERPOLE => "Hotlap superpole",
        _ => sessionType.ToString(),
    };

    public enum AcFlagType : int
    {
        AC_NO_FLAG,
        AC_BLUE_FLAG,
        AC_YELLOW_FLAG,
        AC_BLACK_FLAG,
        AC_WHITE_FLAG,
        AC_CHECKERED_FLAG,
        AC_PENALTY_FLAG,
        AC_GREEN_FLAG,
        AC_BLACK_FLAG_WITH_ORANGE_CIRCLE,

    }

    public static string FlagTypeToString(AcFlagType flagType) => flagType switch
    {
        AcFlagType.AC_NO_FLAG => "Green",
        AcFlagType.AC_BLUE_FLAG => "Blue",
        AcFlagType.AC_YELLOW_FLAG => "Yellow",
        AcFlagType.AC_BLACK_FLAG => "Black",
        AcFlagType.AC_WHITE_FLAG => "White",
        AcFlagType.AC_CHECKERED_FLAG => "Checkered",
        AcFlagType.AC_PENALTY_FLAG => "Penalty",
        AcFlagType.AC_GREEN_FLAG => "Green",
        AcFlagType.AC_BLACK_FLAG_WITH_ORANGE_CIRCLE => "Orange",
        _ => flagType.ToString(),
    };

    public enum PenaltyShortcut : int
    {
        None,
        DriveThrough_Cutting,
        StopAndGo_10_Cutting,
        StopAndGo_20_Cutting,
        StopAndGo_30_Cutting,
        Disqualified_Cutting,
        RemoveBestLaptime_Cutting,

        DriveThrough_PitSpeeding,
        StopAndGo_10_PitSpeeding,
        StopAndGo_20_PitSpeeding,
        StopAndGo_30_PitSpeeding,
        Disqualified_PitSpeeding,
        RemoveBestLaptime_PitSpeeding,

        Disqualified_IgnoredMandatoryPit,

        PostRaceTime,
        Disqualified_Trolling,
        Disqualified_PitEntry,
        Disqualified_PitExit,
        Disqualified_WrongWay,

        DriveThrough_IgnoredDriverStint,
        Disqualified_IgnoredDriverStint,

        Disqualified_ExceededDriverStintLimit,
    };

    public enum AcTrackGripStatus : int
    {
        Green,
        Fast,
        Optimum,
        Greasy,
        Damp,
        Wet,
        Flooded
    };

    public enum AcRainIntensity : int
    {
        No_Rain,
        Dew,
        Light_Rain,
        Medium_Rain,
        Heavy_Rain,
        Thunderstorm,
    }

    public static string AcRainIntensityToString(AcRainIntensity intensity) => intensity switch
    {
        AcRainIntensity.No_Rain => "Dry",
        AcRainIntensity.Dew => "Dew",
        AcRainIntensity.Light_Rain => "Light",
        AcRainIntensity.Medium_Rain => "Medium",
        AcRainIntensity.Heavy_Rain => "Heavy",
        AcRainIntensity.Thunderstorm => "Thunder",
        _ => string.Empty,
    };


    /// <summary>The following members are updated at each graphical step. They mostly refer to player’s car except for carCoordinates and carID, which refer to the cars currently on track.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode), Serializable]
    public sealed class SPageFileGraphic
    {
        /// <summary>Current step index</summary>
        public int PacketId;

        /// <summary>See enums AcStatus</summary>
        public AcStatus Status;

        /// <summary>See enums AcSessionType</summary>
        public AcSessionType SessionType;

        /// <summary>Current lap time in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string CurrentTime;

        /// <summary>Last lap time in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string LastTime;

        /// <summary>Best lap time in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string BestTime;

        /// <summary>Last split time in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string Split;

        /// <summary>Number of completed laps</summary>
        public int CompletedLaps;

        /// <summary>Current player position</summary>
        public int Position;

        /// <summary>Current lap time in milliseconds</summary>
        public int CurrentTimeMs;

        /// <summary>Last lap time in milliseconds</summary>
        public int LastTimeMs;

        /// <summary>Best lap time in milliseconds</summary>
        public int BestTimeMs;

        /// <summary>Session time lef</summary>
        public float SessionTimeLeft;

        /// <summary>Distance travelled in the current stin</summary>
        public float DistanceTraveled;

        /// <summary>Car is pitting</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool IsInPits;

        /// <summary>Current track sector</summary>
        public int CurrentSectorIndex;

        /// <summary>Last sector time in milliseconds</summary>
        public int LastSectorTime;

        /// <summary>Number of completed laps</summary>
        public int NumberOfLaps;

        /// <summary>Tyre compound used in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string TyreCompound;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public float ReplayTimeMultiplier;

        /// <summary>Car position on track spline (0.0 start to 1.0 finish)</summary>
        public float NormalizedCarPosition;

        /// <summary>Number of cars on track</summary>
        public int ActiveCars;

        /// <summary>Coordinates of cars on track</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)] public StructVector3[] CarCoordinates;

        /// <summary>Car IDs of cars on track</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)] public int[] CarIds;

        /// <summary>Player Car ID</summary>
        public int PlayerCarID;

        /// <summary>Penalty time to wait</summary>
        public float PenaltyTime;

        /// <summary>See enums AcFlagType</summary>
        public AcFlagType Flag;

        /// <summary>See enums PenaltyShortcut</summary>
        public PenaltyShortcut PenaltyType;

        /// <summary>Ideal line on</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool IdealLineOn;

        /// <summary>Car is in pit lane</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool IsInPitLane;

        /// <summary>Ideal line friction coefficient</summary>
        public float SurfaceGrip;

        /// <summary>Mandatory pit is completed</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool MandatoryPitDone;

        /// <summary>Wind speed in m/s</summary>
        public float WindSpeed;

        /// <summary>wind direction in radians</summary>
        public float WindDirection;

        /// <summary>Car is working on setup</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool IsSetupMenuVisible;

        /// <summary>Current car main display index</summary>
        public int MainDisplayIndex;

        /// <summary>Current car secondary display index</summary>
        public int SecondaryDisplayIndex;

        /// <summary>Traction control level</summary>
        public int TC;

        /// <summary>Traction control cut level</summary>
        public int TCCut;

        /// <summary>Current engine map</summary>
        public int EngineMap;

        /// <summary>ABS level</summary>
        public int ABS;

        /// <summary>Average fuel consumed per lap in liters</summary>
        public float FuelXLap;

        /// <summary>Rain lights on</summary>
        public int RainLights;

        /// <summary>Flashing lights on</summary>
        public int FlashingLights;

        /// <summary>Current lights stage</summary>
        public int LightsStage;

        /// <summary>Exhaust temperature</summary>
        public float ExhaustTemperature;

        /// <summary>Current wiper stage</summary>
        public int WiperLV;

        /// <summary>Time the driver is allowed to drive/race (ms)</summary>
        public int DriverStintTotalTimeLeft;

        /// <summary>Time the driver is allowed to drive/stint (ms)</summary>
        public int DriverStintTimeLeft;

        /// <summary>Are rain tyres equipped</summary>
        public int RainTyres;

        /// <summary>[No info given by ACC docs on it]</summary>
        public int SessionIndex;

        /// <summary>Used fuel since last time refueling</summary>
        public float UsedFuelSinceRefuel;

        /// <summary>Delta time in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string DeltaLapTime;

        /// <summary>Delta time in milliseconds</summary>
        public int DeltaLapTimeMillis;

        /// <summary>Estimated lap time in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string EstimatedLapTime;

        /// <summary>Estimated lap time in milliseconds</summary>
        public int EstimatedLapTimeMillis;

        /// <summary>Delta positive (1) or negative (0)</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool IsDeltaPositive;

        /// <summary>Last split time in milliseconds</summary>
        public int SplitTimeMillis;

        /// <summary>Check if Lap is valid for timing</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool IsValidLap;

        /// <summary>Laps possible with current fuel level</summary>
        public float FuelEstimatedLaps;

        /// <summary>Status of track in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string TrackStatus;

        /// <summary>Mandatory pitstops the player still has to do</summary>
        public int MandatoryPitStopsLeft;

        /// <summary>Time of day in seconds</summary>
        public float ClockTimeDaySeconds;

        /// <summary>Is Blinker left on</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool BlinkerLeftOn;

        /// <summary>Is Blinker right on</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool BlinkerRightOn;

        /// <summary>Yellow Flag is out?</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool GlobalYellow;

        /// <summary>Yellow Flag in Sector 1 is out?</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool GlobalYellowSector1;

        /// <summary>Yellow Flag in Sector 2 is out?</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool GlobalYellowSector2;

        /// <summary>Yellow Flag in Sector 3 is out?</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool GlobalYellowSector3;

        /// <summary>White Flag is out?</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool GlobalWhite;

        /// <summary>Green Flag is out?</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool GreenFlag;

        /// <summary>Checkered Flag is out?</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool GlobalChequered;

        /// <summary>Red Flag is out?</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool GlobalRed;

        /// <summary># of tyre set on the MFD</summary>
        public int mfdTyreSet;

        /// <summary>How much fuel to add on the MFD</summary>
        public float mfdFuelToAdd;

        /// <summary>Tyre pressure left front on the MFD</summary>
        public float mfdTyrePressureLF;

        /// <summary>Tyre pressure right front on the MFD</summary>
        public float mfdTyrePressureRF;

        /// <summary>Tyre pressure left rear on the MFD</summary>
        public float mfdTyrePressureLR;

        /// <summary>Tyre pressure right rear on the MFD</summary>
        public float mfdTyrePressureRR;

        /// <summary>See enums AcTrackGripStatus</summary>
        public AcTrackGripStatus trackGripStatus;

        /// <summary>See enums AcRainIntensity</summary>
        public AcRainIntensity rainIntensity;

        /// <summary>See enums AcRainIntensity</summary>
        public AcRainIntensity rainIntensityIn10min;

        /// <summary>See enums AcRainIntensity</summary>
        public AcRainIntensity rainIntensityIn30min;

        /// <summary>Tyre Set currently in use</summary>
        public int currentTyreSet;

        /// <summary>Next Tyre set per strategy</summary>
        public int strategyTyreSet;

        /// <summary>Distance in ms to car in front</summary>
        public int gapAheadMillis;

        /// <summary>Distance in ms to car behind</summary>
        public int gapBehindMillis;

        public static readonly int Size = Marshal.SizeOf(typeof(SPageFileGraphic));
        public static readonly byte[] Buffer = new byte[Size];
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode), Serializable]
    public struct StructVector3
    {
        public float X;
        public float Y;
        public float Z;

        public override string ToString() => $"X: {X}, Y: {Y}, Z: {Z}";
    }

    /// <summary>The following members change at each graphic step. They all refer to the player’s car</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode), Serializable]
    public sealed class SPageFilePhysics
    {
        /// <summary>Current step index</summary>
        public int PacketId;

        /// <summary>Gas pedal input value (from -0 to 1.0)</summary>
        public float Gas;

        /// <summary>Brake pedal input value (from -0 to 1.0)</summary>
        public float Brake;

        /// <summary>Amount of fuel remaining in kg</summary>
        public float Fuel;

        /// <summary>Current gear</summary>
        public int Gear;

        /// <summary>Engine revolutions per minute</summary>
        public int Rpms;

        /// <summary>Steering input value (from -1.0 to 1.0)</summary>
        public float SteerAngle;

        /// <summary>Car speed in km/h</summary>
        public float SpeedKmh;

        /// <summary>Car velocity vector in global coordinates</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] Velocity;

        /// <summary>Car acceleration vector in global coordinates</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] AccG;

        /// <summary>Tyre slip for each tyre [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] WheelSlip;

        /// <summary>Wheel load for each tyre [FL, FR, RL, RR]</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] WheelLoad;

        /// <summary>Tyre pressure [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] WheelPressure;

        /// <summary>Wheel angular speed in rad/s [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] WheelAngularSpeed;

        /// <summary>Tyre wear [FL, FR, RL, RR]</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreWear;

        /// <summary>Dirt accumulated on tyre surface [FL, FR, RL, RR]</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreDirtyLevel;

        /// <summary>Tyre rubber core temperature [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreCoreTemperature;

        /// <summary>Wheels camber in radians [FL, FR, RL, RR]</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] CamberRad;

        /// <summary>Suspension travel [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] SuspensionTravel;

        /// <summary>DRS on</summary>
        [Obsolete] public float Drs;

        /// <summary>TC in action</summary>
        public float TC;

        /// <summary>Car yaw orientation</summary>
        public float Heading;

        /// <summary>Car pitch orientation</summary>
        public float Pitch;

        /// <summary>Car roll orientation</summary>
        public float Roll;

        /// <summary>Centre of gravity height</summary>
        [Obsolete] public float CgHeight;

        /// <summary>Car damage: front 0, rear 1, left 2, right 3, centre 4</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public float[] CarDamage;

        /// <summary>Number of tyres out of track</summary>
        [Obsolete] public int NumberOfTyresOut;

        /// <summary>Pit limiter is on</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool PitLimiterOn;

        /// <summary>ABS in action</summary>
        public float Abs;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public float KersCharge;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public float KersInput;

        /// <summary>Automatic transmission on</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool AutoShifterOn;

        /// <summary>Ride height: 0 front, 1 rear</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public float[] RideHeight;

        /// <summary>Car turbo level</summary>
        public float TurboBoost;

        /// <summary>Car ballast in kg / Not implemented</summary>
        [Obsolete] public float Ballast;

        /// <summary>Air density</summary>
        [Obsolete] public float AirDensity;

        /// <summary>Air temperature</summary>
        public float AirTemp;

        /// <summary>Road temperature</summary>
        public float RoadTemp;

        /// <summary>Car angular velocity vector in local coordinates</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] LocalAngularVelocity;

        /// <summary>Force feedback signal</summary>
        public float finalFF;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public float PerformanceMeter;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public int EngineBrake;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public int ErsRecoveryLevel;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public int ErsPowerLevel;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public int ErsHeatCharging;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public int ErsIsCharging;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public float KersCurrentKJ;

        /// <summary>Not used in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.Bool)] public bool DrsAvailable;

        /// <summary>Not used in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.Bool)] public bool DrsEnabled;

        /// <summary>Brake discs temperatures</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] BrakeTemperature;

        /// <summary>Clutch pedal input value (from -0 to 1.0)</summary>
        public float Clutch;

        /// <summary>Not shown in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreTempI;

        /// <summary>Not shown in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreTempM;

        /// <summary>Not shown in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreTempO;

        /// <summary>Car is controlled by the AI</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool IsAiControlled;

        /// <summary>Tyre contact point global coordinates [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public StructVector3[] TyreContactPoint;

        /// <summary>Tyre contact normal [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public StructVector3[] TyreContactNormal;

        /// <summary>Tyre contact heading [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public StructVector3[] TyreContactHeading;

        /// <summary>Front brake bias</summary>
        public float BrakeBias;

        /// <summary>Car velocity vector in local coordinates</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] LocalVelocity;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public int P2PActivations;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public int P2PStatus;

        /// <summary>Maximum engine rpm</summary>
        [Obsolete] public int CurrentMaxRpm;

        /// <summary>Not shown in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] mz;

        /// <summary>Not shown in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] fx;

        /// <summary>Not shown in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] fy;

        /// <summary>Tyre slip ratio [FL, FR, RL, RR] in radians</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] SlipRatio;

        /// <summary>Tyre slip angle [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] SlipAngle;

        /// <summary>TC in action</summary>
        [Obsolete] public int TcinAction;

        /// <summary>ABS in action</summary>
        [Obsolete] public int AbsInAction;

        /// <summary>Suspensions damage levels [FL, FR, RL, RR] (obsolete?)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] SuspensionDamage;

        /// <summary>Tyres core temperatures [FL, FR, RL, RR] (obsolete?)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreTemp;

        /// <summary>Water Temperature</summary>
        public float WaterTemp;

        /// <summary>Brake pressure [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] brakePressure;

        /// <summary>Brake pad compund front</summary>
        public int frontBrakeCompound;

        /// <summary>Brake pad compund rear</summary>
        public int rearBrakeCompound;

        /// <summary>Brake pad wear [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] PadLife;

        /// <summary>Brake disk wear [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] DiscLife;

        /// <summary>Ignition switch set to on?</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool IgnitionOn;

        /// <summary>Starter Switch set to on?</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool StarterEngineOn;

        /// <summary>Engine running?</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool IsEngineRunning;

        /// <summary>Vibrations sent to the FFB, could be used for motion rigs</summary>
        public float KerbVibration;

        /// <summary>Vibrations sent to the FFB, could be used for motion rigs</summary>
        public float SlipVibrations;

        /// <summary>Vibrations sent to the FFB, could be used for motion rigs</summary>
        public float Gvibrations;

        /// <summary>Vibrations sent to the FFB, could be used for motion rigs</summary>
        public float AbsVibrations;

        public static readonly int Size = Marshal.SizeOf(typeof(SPageFilePhysics));
        public static readonly byte[] Buffer = new byte[Size];
    };

    /// <summary>The following members are initialized when the instance starts and never changes until the instance is closed</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode), Serializable]
    public sealed class SPageFileStatic
    {
        /// <summary>Shared memory version in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string SharedMemoryVersion;

        /// <summary>Assetto Corsa version in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string AssettoCorsaVersion;

        /// <summary>Number of sessions</summary>
        public int NumberOfSessions;

        /// <summary>Number of cars</summary>
        public int NumberOfCars;

        /// <summary>Player car model in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string CarModel;

        /// <summary>Track name in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string Track;

        /// <summary>Player name in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string PlayerName;

        /// <summary>Player surname in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string PlayerSurname;

        /// <summary>Player nickname in wide character</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string PlayerNickname;

        /// <summary>Number of sectors</summary>
        public int SectorCount;

        /// <summary>Not shown in ACC</summary>
        [Obsolete] public float MaxTorque;

        /// <summary>Not shown in ACC</summary>
        [Obsolete] public float MaxPower;

        /// <summary>Maximum rpm</summary>
        public int MaxRpm;

        /// <summary>Maximum fuel tank capacity</summary>
        public float MaxFuel;

        /// <summary>Not shown in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] SuspensionMaxTravel;

        /// <summary>Not shown in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreRadius;

        /// <summary>Maximum turbo boost (obsolete?)</summary>
        public float MaxTurboBoost;

        /// <summary>[]</summary>
        [Obsolete] public float AirTemperature;

        /// <summary>[]</summary>
        [Obsolete] public float RoadTemperature;

        /// <summary>Penalties enabled?</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool PenaltiesEnabled;

        /// <summary>Fuel consumption rate</summary>
        public float AidFuelRate;

        /// <summary>Tyre wear rate</summary>
        public float AidTireRate;

        /// <summary>Mechanical damage rate</summary>
        public float AidMechanicalDamage;

        /// <summary>Not allowed in Blancpain endurance series</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool AidAllowTyreBlankets;

        /// <summary>Stability control used</summary>
        public float AidStability;

        /// <summary>Auto clutch used</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool AidAutoClutch;

        /// <summary>Always true in ACC</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool AidAutoBlip;

        /// <summary></summary>
        [Obsolete][MarshalAs(UnmanagedType.Bool)] public bool HasDRS;

        /// <summary></summary>
        [Obsolete][MarshalAs(UnmanagedType.Bool)] public bool HasERS;

        /// <summary></summary>
        [Obsolete][MarshalAs(UnmanagedType.Bool)] public bool HasKERS;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public float KersMaxJoules;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public int EngineBrakeSettingsCount;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public int ErsPowerControllerCount;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public float TrackSplineLength;

        /// <summary>Not used in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string TrackConfiguration;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public float ErsMaxJ;

        /// <summary>Not used in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.Bool)] public bool IsTimedRace;

        /// <summary>Not used in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.Bool)] public bool HasExtraLap;

        /// <summary>Not used in ACC</summary>
        [Obsolete][MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string CarSkin;

        /// <summary>Not used in ACC</summary>
        [Obsolete] public int ReversedGridPositions;

        /// <summary>Pit window opening time</summary>
        public int PitWindowStart;

        /// <summary>Pit windows closing time</summary>
        public int PitWindowEnd;

        /// <summary>If is a multiplayer session</summary>
        [MarshalAs(UnmanagedType.Bool)] public bool isOnline;

        /// <summary>Name of the dry tyres</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string DryTyresName;

        /// <summary>Name of the wet tyres</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string WetTyresName;

        public static readonly int Size = Marshal.SizeOf(typeof(SPageFileStatic));
        public static readonly byte[] Buffer = new byte[Size];
    };

    public SPageFileGraphic ReadGraphicsPageFile(bool fromCache = false)
    {
        if (fromCache) return PageFileGraphic;
        return PageFileGraphic = StructExtension.ToStruct<SPageFileGraphic>(MemoryMappedFile.CreateOrOpen(graphicsMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite), SPageFileGraphic.Buffer);
    }

    public SPageFileStatic ReadStaticPageFile(bool fromCache = false)
    {
        if (fromCache) return PageFileStatic;
        return PageFileStatic = StructExtension.ToStruct<SPageFileStatic>(MemoryMappedFile.CreateOrOpen(staticMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite), SPageFileStatic.Buffer);
    }

    public SPageFilePhysics ReadPhysicsPageFile(bool fromCache = false)
    {
        if (fromCache) return PageFilePhysics;
        return PageFilePhysics = StructExtension.ToStruct<SPageFilePhysics>(MemoryMappedFile.CreateOrOpen(physicsMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite), SPageFilePhysics.Buffer);
    }
}
