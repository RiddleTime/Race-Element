using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using RaceElement.Data.SharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaEvo.SharedMemory;

public sealed class AcEvoSharedMemory
{
    private readonly string physicsMap = "Local\\acevo_pmf_physics";
    private readonly string graphicsMap = "Local\\acevo_pmf_graphics";
    private readonly string staticMap = "Local\\acevo_pmf_static";

    public SPageFileStaticEvo PageFileStatic { get; private set; }
    public SPageFilePhysicsEvo PageFilePhysics { get; private set; }
    public SPageFileGraphicEvo PageFileGraphic { get; private set; }

    private static AcEvoSharedMemory _instance;
    public static AcEvoSharedMemory Instance
    {
        get
        {
            _instance ??= new AcEvoSharedMemory();
            return _instance;
        }
    }

    private AcEvoSharedMemory()
    {
        ReadStaticPageFile();
        ReadPhysicsPageFile();
        ReadGraphicsPageFile();
    }

    #region Enums

    /// <summary>Current operational state of the simulator.</summary>
    public enum AcEvoStatus : int
    {
        /// <summary>Simulator is not running / no session active</summary>
        AcOff = 0,
        /// <summary>A replay is currently being played back</summary>
        AcReplay = 1,
        /// <summary>Live driving session is active</summary>
        AcLive = 2,
        /// <summary>Session is paused</summary>
        AcPause = 3
    }

    /// <summary>Type of racing session currently loaded.</summary>
    public enum AcEvoSessionType : int
    {
        /// <summary>Session type not yet determined</summary>
        AcUnknown = -1,
        /// <summary>Time attack / qualifying session</summary>
        AcTimeAttack = 0,
        /// <summary>Race session</summary>
        AcRace = 1,
        /// <summary>Hot-stint practice</summary>
        AcHotStint = 2,
        /// <summary>Untimed cruise</summary>
        AcCruise = 3
    }

    /// <summary>Race flag currently shown to the driver.</summary>
    public enum AcEvoFlagType : int
    {
        /// <summary>No flag displayed</summary>
        AcNoFlag = 0,
        /// <summary>Slow vehicle ahead on track</summary>
        AcWhiteFlag = 1,
        /// <summary>Track clear — racing resumed</summary>
        AcGreenFlag = 2,
        /// <summary>Session stopped due to incident or hazard</summary>
        AcRedFlag = 3,
        /// <summary>Lapped car must yield to the race leader</summary>
        AcBlueFlag = 4,
        /// <summary>Hazard present — no overtaking</summary>
        AcYellowFlag = 5,
        /// <summary>Driver disqualified / must pit immediately</summary>
        AcBlackFlag = 6,
        /// <summary>Warning for unsportsmanlike behaviour</summary>
        AcBlackWhiteFlag = 7,
        /// <summary>Session or race has ended</summary>
        AcCheckeredFlag = 8,
        /// <summary>Mechanical problem — car must pit</summary>
        AcOrangeCircleFlag = 9,
        /// <summary>Slippery surface ahead on track</summary>
        AcRedYellowStripesFlag = 10
    }

    /// <summary>Where on the circuit the car is currently positioned.</summary>
    public enum AcEvoCarLocation : int
    {
        /// <summary>Position not yet determined</summary>
        AcevoUnassigned = 0,
        /// <summary>Car is inside the pit lane</summary>
        AcevoPitlane = 1,
        /// <summary>Car is at the pit-lane entry</summary>
        AcevoPitentry = 2,
        /// <summary>Car is at the pit-lane exit</summary>
        AcevoPitexit = 3,
        /// <summary>Car is on the racing circuit</summary>
        AcevoTrack = 4
    }

    /// <summary>Powertrain type of the player car.</summary>
    public enum AcEvoEngineType : int
    {
        /// <summary>Traditional petrol/diesel internal combustion engine</summary>
        AcevoInternalCombustion = 0,
        /// <summary>Fully electric powertrain</summary>
        AcevoElectricMotor = 1
    }

    /// <summary>Initial grip conditions at session start.</summary>
    public enum AcEvoStartingGrip : int
    {
        /// <summary>Track grip at minimum</summary>
        AcevoGreen = 0,
        /// <summary>Track grip in advanced (fast) stage</summary>
        AcevoFast = 1,
        /// <summary>Track conditions starting at optimum grip</summary>
        AcevoOptimum = 2
    }

    #endregion

    #region Fixed-size inner structures

    /// <summary>Complete state of a single tyre corner. Embedded four times in SPageFileGraphicEvo (lf, rf, lr, rr). [256 bytes]</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public readonly struct SmevoTyreState
    {
        /// <summary>Combined tyre slip magnitude</summary>
        public readonly float Slip;
        /// <summary>Tyre is locked under braking (true = locking)</summary>
        public readonly bool Lock;
        /// <summary>Tyre inflation pressure (PSI)</summary>
        public readonly float TyrePression;
        /// <summary>Average tyre carcass temperature in °C</summary>
        public readonly float TyreTemperatureC;
        /// <summary>Brake disc temperature in °C</summary>
        public readonly float BrakeTemperatureC;
        /// <summary>Hydraulic brake pressure applied at this corner</summary>
        public readonly float BrakePressure;
        /// <summary>Inner-edge tyre temperature in °C</summary>
        public readonly float TyreTemperatureLeft;
        /// <summary>Centre-tread tyre temperature in °C</summary>
        public readonly float TyreTemperatureCenter;
        /// <summary>Outer-edge tyre temperature in °C</summary>
        public readonly float TyreTemperatureRight;
        /// <summary>Name of the compound fitted on the front axle</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public readonly string TyreCompoundFront;
        /// <summary>Name of the compound fitted on the rear axle</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public readonly string TyreCompoundRear;
        /// <summary>Pressure as a 0–1 fraction of the target range</summary>
        public readonly float TyreNormalizedPressure;
        /// <summary>Inner-edge temperature as a 0–1 fraction of optimal range</summary>
        public readonly float TyreNormalizedTemperatureLeft;
        /// <summary>Centre temperature as a 0–1 fraction of optimal range</summary>
        public readonly float TyreNormalizedTemperatureCenter;
        /// <summary>Outer-edge temperature as a 0–1 fraction of optimal range</summary>
        public readonly float TyreNormalizedTemperatureRight;
        /// <summary>Brake temperature as a 0–1 fraction of optimal operating range</summary>
        public readonly float BrakeNormalizedTemperature;
        /// <summary>Core tyre temperature as a 0–1 fraction of optimal range</summary>
        public readonly float TyreNormalizedTemperatureCore;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        private readonly byte[] PlaceHolder; // Padding to ensure exactly 256 bytes
    }

    /// <summary>Structural damage level for each body zone of the car (0.0 = undamaged, 1.0 = destroyed). [128 bytes]</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public readonly struct SmevoDamageState
    {
        /// <summary>Damage on the front body / nose</summary>
        public readonly float DamageFront;
        /// <summary>Damage on the rear body / diffuser</summary>
        public readonly float DamageRear;
        /// <summary>Damage on the left side of the body</summary>
        public readonly float DamageLeft;
        /// <summary>Damage on the right side of the body</summary>
        public readonly float DamageRight;
        /// <summary>Damage on the central / underfloor area</summary>
        public readonly float DamageCenter;
        /// <summary>Damage on the front-left suspension</summary>
        public readonly float DamageSuspensionLf;
        /// <summary>Damage on the front-right suspension</summary>
        public readonly float DamageSuspensionRf;
        /// <summary>Damage on the rear-left suspension</summary>
        public readonly float DamageSuspensionLr;
        /// <summary>Damage on the rear-right suspension</summary>
        public readonly float DamageSuspensionRr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 92)]
        private readonly byte[] PlaceHolder; // Padding to ensure exactly 128 bytes
    }

    /// <summary>Status of each pit-stop service action. −1 = will not perform, 0 = completed, 1 = in progress. [64 bytes]</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public readonly struct SmevoPitInfo
    {
        /// <summary>Body-repair action state</summary>
        public readonly sbyte Damage;
        /// <summary>Refuelling action state</summary>
        public readonly sbyte Fuel;
        /// <summary>Front-left tyre change state</summary>
        public readonly sbyte TyresLf;
        /// <summary>Front-right tyre change state</summary>
        public readonly sbyte TyresRf;
        /// <summary>Rear-left tyre change state</summary>
        public readonly sbyte TyresLr;
        /// <summary>Rear-right tyre change state</summary>
        public readonly sbyte TyresRr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 58)]
        private readonly byte[] PlaceHolder; // Padding to ensure exactly 64 bytes
    }

    /// <summary>All driver-adjustable electronic aid and setup settings. [128 bytes]</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public readonly struct SmevoElectronics
    {
        /// <summary>Traction-control level (0 = off, higher = more aggressive)</summary>
        public readonly sbyte TcLevel;
        /// <summary>TC throttle-cut aggressiveness level</summary>
        public readonly sbyte TcCutLevel;
        /// <summary>ABS intervention level (0 = off)</summary>
        public readonly sbyte AbsLevel;
        /// <summary>Electronic stability-control level (0 = off)</summary>
        public readonly sbyte EscLevel;
        /// <summary>Electronic brake-balance adjustment level</summary>
        public readonly sbyte EbbLevel;
        /// <summary>Front brake-bias ratio (e.g. 0.56 = 56 % front)</summary>
        public readonly float BrakeBias;
        /// <summary>Engine map / power mode selection</summary>
        public readonly sbyte EngineMapLevel;
        /// <summary>Turbo wastegate or boost target setting</summary>
        public readonly float TurboLevel;
        /// <summary>ERS power-deployment strategy map</summary>
        public readonly sbyte ErsDeploymentMap;
        /// <summary>ERS recharge aggressiveness setting</summary>
        public readonly float ErsRechargeMap;
        /// <summary>ERS heat-based charging is enabled</summary>
        public readonly bool IsErsHeatChargingOn;
        /// <summary>ERS overtake (maximum-deploy) mode is active</summary>
        public readonly bool IsErsOvertakeModeOn;
        /// <summary>DRS flap is currently open</summary>
        public readonly bool IsDrsOpen;
        /// <summary>Differential lock level under power</summary>
        public readonly sbyte DiffPowerLevel;
        /// <summary>Differential lock level on lift / coast</summary>
        public readonly sbyte DiffCoastLevel;
        /// <summary>Front bump (compression) damper stiffness level</summary>
        public readonly sbyte FrontBumpDamperLevel;
        /// <summary>Front rebound damper stiffness level</summary>
        public readonly sbyte FrontReboundDamperLevel;
        /// <summary>Rear bump (compression) damper stiffness level</summary>
        public readonly sbyte RearBumpDamperLevel;
        /// <summary>Rear rebound damper stiffness level</summary>
        public readonly sbyte RearReboundDamperLevel;
        /// <summary>Ignition switch is on</summary>
        public readonly bool IsIgnitionOn;
        /// <summary>Pit-speed limiter is active</summary>
        public readonly bool IsPitlimiterOn;
        /// <summary>Selected vehicle performance / power mode index</summary>
        public readonly sbyte ActivePerformanceMode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 88)]
        private readonly byte[] PlaceHolder; // Padding to ensure exactly 128 bytes
    }

    /// <summary>Cockpit light, display, and instrumentation panel states. [128 bytes]</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public readonly struct SmevoInstrumentation
    {
        /// <summary>Main exterior light stage (0 = off)</summary>
        public readonly sbyte MainLightStage;
        /// <summary>Auxiliary / special lights level</summary>
        public readonly sbyte SpecialLightStage;
        /// <summary>Interior cockpit illumination level</summary>
        public readonly sbyte CockpitLightStage;
        /// <summary>Windscreen wiper speed (0 = off)</summary>
        public readonly sbyte WiperLevel;
        /// <summary>Rear rain light is on</summary>
        public readonly bool RainLights;
        /// <summary>Left turn indicator is active</summary>
        public readonly bool DirectionLightLeft;
        /// <summary>Right turn indicator is active</summary>
        public readonly bool DirectionLightRight;
        /// <summary>Flashing lights are active</summary>
        public readonly bool FlashingLights;
        /// <summary>Hazard lights are illuminated</summary>
        public readonly bool WarningLights;
        /// <summary>Index of the currently focused display device</summary>
        public readonly sbyte SelectedDisplayIndex;
        /// <summary>Active page index on displays (array of 16 items)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly sbyte[] DisplayCurrentPageIndex;
        /// <summary>Headlights are on and visible to other drivers</summary>
        public readonly bool AreHeadlightsVisible;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 101)]
        private readonly byte[] PlaceHolder; // Padding to ensure exactly 128 bytes
    }

    /// <summary>Server-side session lifecycle information. [256 bytes]</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public readonly struct SmevoSessionState
    {
        /// <summary>Name of the current session phase (e.g. 'Race', 'Qualify')</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public readonly string PhaseName;
        /// <summary>Formatted remaining session time (HH:MM:SS)</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public readonly string TimeLeft;
        /// <summary>Remaining session time in milliseconds</summary>
        public readonly int TimeLeftMs;
        /// <summary>Formatted wait time before session start</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public readonly string WaitTime;
        /// <summary>Total laps scheduled for this session</summary>
        public readonly int TotalLap;
        /// <summary>Current lap number being driven</summary>
        public readonly int CurrentLap;
        /// <summary>Number of starting lights currently illuminated</summary>
        public readonly int LightsOn;
        /// <summary>Starting-light sequence mode identifier</summary>
        public readonly int LightsMode;
        /// <summary>Track lap length in kilometres</summary>
        public readonly float LapLengthKm;
        /// <summary>Non-zero when the session is ending</summary>
        public readonly int EndSessionFlag;
        /// <summary>Formatted countdown to the next session</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public readonly string TimeToNextSession;
        /// <summary>Player has lost connection to the game server</summary>
        public readonly bool DisconnectedFromServer;
        /// <summary>Season restart option is available to the player</summary>
        public readonly bool RestartSeasonEnabled;
        /// <summary>Drive button is enabled in the UI</summary>
        public readonly bool UiEnableDrive;
        /// <summary>Setup screen is accessible from the UI</summary>
        public readonly bool UiEnableSetup;
        /// <summary>Ready-to-proceed indicator is blinking</summary>
        public readonly bool IsReadyToNextBlinking;
        /// <summary>Waiting-for-players lobby screen is shown</summary>
        public readonly bool ShowWaitingForPlayers;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 140)]
        private readonly byte[] PlaceHolder; // Padding to ensure exactly 256 bytes
    }

    /// <summary>Lap timing and delta values displayed on the HUD. [256 bytes]</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public readonly struct SmevoTimingState
    {
        /// <summary>Current lap time as a formatted string</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public readonly string CurrentLaptime;
        /// <summary>Delta vs. current reference lap (formatted)</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public readonly string DeltaCurrent;
        /// <summary>Sign of delta_current: +1 slower, −1 faster, 0 = hidden</summary>
        public readonly int DeltaCurrentP;
        /// <summary>Last completed lap time as a formatted string</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public readonly string LastLaptime;
        /// <summary>Delta vs. last lap (formatted)</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public readonly string DeltaLast;
        /// <summary>Sign of delta_last: +1 slower, −1 faster, 0 = hidden</summary>
        public readonly int DeltaLastP;
        /// <summary>Personal best lap time as a formatted string</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public readonly string BestLaptime;
        /// <summary>Theoretical best lap (sum of best sectors) as a formatted string</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public readonly string IdealLaptime;
        /// <summary>Total elapsed session time as a formatted string</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public readonly string TotalTime;
        /// <summary>Current lap has been invalidated (track-limits violation, etc.)</summary>
        public readonly bool IsInvalid;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 137)]
        private readonly byte[] PlaceHolder; // Padding to ensure exactly 256 bytes
    }

    /// <summary>Driver-assist settings currently active for the player car. [64 bytes]</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public readonly struct SmevoAssistsState
    {
        /// <summary>Automatic gearshift aid level (0 = off)</summary>
        public readonly byte AutoGear;
        /// <summary>Automatic throttle blip on downshift (0 = off)</summary>
        public readonly byte AutoBlip;
        /// <summary>Automatic clutch management (0 = off)</summary>
        public readonly byte AutoClutch;
        /// <summary>Automatic clutch during the rolling start (0 = off)</summary>
        public readonly byte AutoClutchOnStart;
        /// <summary>Manual ignition and electric start required (0 = automatic)</summary>
        public readonly byte ManualIgnitionEStart;
        /// <summary>Pit-speed limiter activates automatically (0 = manual)</summary>
        public readonly byte AutoPitLimiter;
        /// <summary>Standing-start launch assistance active (0 = off)</summary>
        public readonly byte StandingStartAssist;
        /// <summary>Auto-steer correction strength (0.0 = off, 1.0 = maximum)</summary>
        public readonly float AutoSteer;
        /// <summary>Arcade-style stability aid level (0.0 = off, 1.0 = maximum)</summary>
        public readonly float ArcadeStabilityControl;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        private readonly byte[] PlaceHolder; // Padding to ensure exactly 64 bytes
    }

    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly struct StructVector3
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
    }

    /// <summary>Raw physics telemetry updated every simulation step. Contains all low-level vehicle dynamics data.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public sealed class SPageFilePhysicsEvo
    {
        /// <summary>Incrementing counter — detect new data packets by comparing to previous value</summary>
        public int PacketId;
        /// <summary>Throttle pedal position (0.0 = released, 1.0 = full throttle)</summary>
        public float Gas;
        /// <summary>Brake pedal position (0.0 = released, 1.0 = full brake)</summary>
        public float Brake;
        /// <summary>Remaining fuel in litres</summary>
        public float Fuel;
        /// <summary>Engaged gear: 0 = reverse, 1 = neutral, 2+ = forward gears</summary>
        public int Gear;
        /// <summary>Engine speed in revolutions per minute</summary>
        public int Rpms;
        /// <summary>Normalised steering angle (−1.0 = full left, +1.0 = full right)</summary>
        public float SteerAngle;
        /// <summary>Vehicle speed in km/h</summary>
        public float SpeedKmh;
        /// <summary>World-space velocity vector [X, Y, Z] in m/s</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] Velocity;
        /// <summary>Acceleration in G [lateral X, longitudinal Y, vertical Z]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] AccG;
        /// <summary>Tyre slip value per wheel [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] WheelSlip;
        /// <summary>Vertical tyre load in Newtons [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] WheelLoad;
        /// <summary>Tyre inflation pressure in PSI [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] WheelsPressure;
        /// <summary>Wheel rotational speed in rad/s [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] WheelAngularSpeed;
        /// <summary>Tyre wear level (0.0 = new, 1.0 = fully worn) [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreWear;
        /// <summary>Amount of dirt / debris on each tyre surface [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreDirtyLevel;
        /// <summary>Core temperature of each tyre in °C [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreCoreTemperature;
        /// <summary>Wheel camber angle in radians per corner [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] CamberRad;
        /// <summary>Suspension compression travel in metres [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] SuspensionTravel;
        /// <summary>DRS flap state (0.0 = closed, 1.0 = fully open)</summary>
        public float Drs;
        /// <summary>Traction control cut intensity (0.0 = inactive, 1.0 = maximum)</summary>
        public float Tc;
        /// <summary>Vehicle heading relative to world north in radians</summary>
        public float Heading;
        /// <summary>Chassis pitch angle in radians (positive = nose up)</summary>
        public float Pitch;
        /// <summary>Chassis roll angle in radians (positive = right side down)</summary>
        public float Roll;
        /// <summary>Height of the centre of gravity above the ground in metres</summary>
        public float CgHeight;
        /// <summary>Damage level per body zone [front, rear, left, right, centre] (0.0–1.0)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public float[] CarDamage;
        /// <summary>Number of tyres currently outside track limits</summary>
        public int NumberOfTyresOut;
        /// <summary>Pit-speed limiter active (0 = off, 1 = on)</summary>
        public int PitLimiterOn;
        /// <summary>ABS intervention intensity (0.0 = inactive, 1.0 = fully active)</summary>
        public float Abs;
        /// <summary>KERS/ERS battery state of charge (0.0–1.0)</summary>
        public float KersCharge;
        /// <summary>KERS/ERS power delivery level currently being deployed (0.0–1.0)</summary>
        public float KersInput;
        /// <summary>Automatic gearshift aid active (0 = manual, 1 = auto)</summary>
        public int AutoShifterOn;
        /// <summary>Ride height at front and rear axle in metres [front, rear]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public float[] RideHeight;
        /// <summary>Current turbo boost pressure in bar</summary>
        public float TurboBoost;
        /// <summary>Additional ballast added to the car in kg</summary>
        public float Ballast;
        /// <summary>Ambient air density in kg/m³</summary>
        public float AirDensity;
        /// <summary>Ambient air temperature in °C</summary>
        public float AirTemp;
        /// <summary>Road surface temperature in °C</summary>
        public float RoadTemp;
        /// <summary>Angular velocity in the car's local frame [pitch, yaw, roll] in rad/s</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] LocalAngularVel;
        /// <summary>Final force-feedback torque value sent to the wheel (Nm)</summary>
        public float FinalFf;
        /// <summary>Real-time delta vs. best lap (positive = ahead of reference)</summary>
        public float PerformanceMeter;
        /// <summary>Engine-braking setting level (higher = more engine braking)</summary>
        public int EngineBrake;
        /// <summary>ERS energy-recovery intensity level</summary>
        public int ErsRecoveryLevel;
        /// <summary>ERS power-deployment level</summary>
        public int ErsPowerLevel;
        /// <summary>ERS heat-charging mode active (0 = off, 1 = on)</summary>
        public int ErsHeatCharging;
        /// <summary>ERS currently recovering energy (0 = deploying, 1 = charging)</summary>
        public int ErsIsCharging;
        /// <summary>Energy stored in the KERS/ERS battery in kilojoules</summary>
        public float KersCurrentKj;
        /// <summary>DRS can be activated (0 = no, 1 = yes)</summary>
        public int DrsAvailable;
        /// <summary>DRS is open and active (0 = closed, 1 = open)</summary>
        public int DrsEnabled;
        /// <summary>Brake disc temperature per corner in °C [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] BrakeTemp;
        /// <summary>Clutch pedal position (0.0 = engaged, 1.0 = fully disengaged)</summary>
        public float Clutch;
        /// <summary>Tyre inner-edge temperature per wheel in °C [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreTempI;
        /// <summary>Tyre mid-tread temperature per wheel in °C [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreTempM;
        /// <summary>Tyre outer-edge temperature per wheel in °C [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreTempO;
        /// <summary>Car is driven by AI (0 = player, 1 = AI)</summary>
        public int IsAiControlled;
        /// <summary>3-D world-space contact point of each tyre with the road [FL,FR,RL,RR][X,Y,Z]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public StructVector3[] TyreContactPoint;
        /// <summary>Road-surface normal vector at each tyre contact point [FL,FR,RL,RR][X,Y,Z]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public StructVector3[] TyreContactNormal;
        /// <summary>Heading vector at each tyre contact point [FL,FR,RL,RR][X,Y,Z]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public StructVector3[] TyreContactHeading;
        /// <summary>Front brake-bias ratio (e.g. 0.56 = 56 % front)</summary>
        public float BrakeBias;
        /// <summary>Velocity in the car's local reference frame [X, Y, Z] in m/s</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] LocalVelocity;
        /// <summary>Remaining Push-to-Pass activations</summary>
        public int P2pActivations;
        /// <summary>Push-to-Pass status (0 = inactive, 1 = active)</summary>
        public int P2pStatus;
        /// <summary>Current rev-limiter ceiling in RPM</summary>
        public int CurrentMaxRpm;
        /// <summary>Self-aligning tyre torque (Mz) per wheel [FL, FR, RL, RR] in Nm</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] Mz;
        /// <summary>Longitudinal tyre force (Fx) per wheel [FL, FR, RL, RR] in N</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] Fx;
        /// <summary>Lateral tyre force (Fy) per wheel [FL, FR, RL, RR] in N</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] Fy;
        /// <summary>Longitudinal slip ratio per tyre [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] SlipRatio;
        /// <summary>Lateral slip angle per tyre in radians [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] SlipAngle;
        /// <summary>Traction control currently cutting power (0 = no, 1 = yes)</summary>
        public int TcinAction;
        /// <summary>ABS currently modulating brakes (0 = no, 1 = yes)</summary>
        public int AbsInAction;
        /// <summary>Suspension structural damage per corner (0.0–1.0) [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] SuspensionDamage;
        /// <summary>Representative tyre surface temperature per wheel in °C [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] TyreTemp;
        /// <summary>Engine coolant temperature in °C</summary>
        public float WaterTemp;
        /// <summary>Braking torque at each wheel in Nm [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] BrakeTorque;
        /// <summary>Front brake-pad compound identifier</summary>
        public int FrontBrakeCompound;
        /// <summary>Rear brake-pad compound identifier</summary>
        public int RearBrakeCompound;
        /// <summary>Brake-pad remaining life per corner (0.0–1.0) [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] PadLife;
        /// <summary>Brake-disc remaining life per corner (0.0–1.0) [FL, FR, RL, RR]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] DiscLife;
        /// <summary>Ignition switch state (0 = off, 1 = on)</summary>
        public int IgnitionOn;
        /// <summary>Starter motor currently cranking (0 = no, 1 = yes)</summary>
        public int StarterEngineOn;
        /// <summary>Engine is running (0 = stopped, 1 = running)</summary>
        public int IsEngineRunning;
        /// <summary>Vibration intensity transmitted from kerb strikes</summary>
        public float KerbVibration;
        /// <summary>Vibration intensity caused by tyre slip</summary>
        public float SlipVibrations;
        /// <summary>Vibration intensity from road surface texture</summary>
        public float RoadVibrations;
        /// <summary>Vibration intensity generated by ABS pulsing</summary>
        public float AbsVibrations;

        public static readonly int Size = Marshal.SizeOf<SPageFilePhysicsEvo>();
        public static readonly byte[] Buffer = new byte[Size];
    }

    /// <summary>Main HUD and graphics telemetry page. Updated each rendered frame. Contains embedded sub-structs for tyres, damage, electronics, timing, and session state.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public sealed class SPageFileGraphicEvo
    {
        /// <summary>Incrementing counter — detect new frames by comparing to previous value</summary>
        public int PacketId;
        /// <summary>Current simulator operational state (see AcEvoStatus)</summary>
        public AcEvoStatus Status;

        /// <summary>Unique ID of the car currently shown by the camera (low 64 bits)</summary>
        public ulong FocusedCarIdA;
        /// <summary>Unique ID of the car currently shown by the camera (high 64 bits)</summary>
        public ulong FocusedCarIdB;
        /// <summary>Unique ID of the player's own car (low 64 bits)</summary>
        public ulong PlayerCarIdA;
        /// <summary>Unique ID of the player's own car (high 64 bits)</summary>
        public ulong PlayerCarIdB;

        /// <summary>Engine speed in RPM for HUD display</summary>
        public ushort Rpm;

        /// <summary>Rev limiter is cutting fuel / ignition (bouncing off limiter)</summary>
        public bool IsRpmLimiterOn;
        /// <summary>Engine RPM is in the upshift window</summary>
        public bool IsChangeUpRpm;
        /// <summary>Engine RPM is in the downshift window</summary>
        public bool IsChangeDownRpm;
        /// <summary>Traction control is actively intervening this frame</summary>
        public bool TcActive;
        /// <summary>ABS is actively modulating brake pressure this frame</summary>
        public bool AbsActive;
        /// <summary>Electronic stability control is intervening this frame</summary>
        public bool EscActive;
        /// <summary>Launch control system is engaged</summary>
        public bool LaunchActive;
        /// <summary>Ignition switch is on</summary>
        public bool IsIgnitionOn;
        /// <summary>Engine is running</summary>
        public bool IsEngineRunning;
        /// <summary>KERS/ERS battery is currently being charged</summary>
        public bool KersIsCharging;
        /// <summary>Car is travelling in the wrong direction on track</summary>
        public bool IsWrongWay;
        /// <summary>DRS activation is permitted in this section</summary>
        public bool IsDrsAvailable;
        /// <summary>High-voltage battery pack is in charging state</summary>
        public bool BatteryIsCharging;
        /// <summary>Maximum ERS deployment energy for this lap has been consumed</summary>
        public bool IsMaxKjPerLapReached;
        /// <summary>Maximum ERS charge energy for this lap has been stored</summary>
        public bool IsMaxChargeKjPerLapReached;

        /// <summary>Displayed speed in km/h</summary>
        public short DisplaySpeedKmh;
        /// <summary>Displayed speed in mph</summary>
        public short DisplaySpeedMph;
        /// <summary>Displayed speed in m/s</summary>
        public short DisplaySpeedMs;

        /// <summary>Speed delta vs. pit-lane limit (negative = under limit)</summary>
        public float PitspeedingDelta;
        /// <summary>Current gear as an integer (same encoding as physics gear)</summary>
        public short GearInt;

        /// <summary>Engine RPM as a fraction of redline (0.0–1.0)</summary>
        public float RpmPercent;
        /// <summary>Throttle pedal position as a fraction (0.0–1.0)</summary>
        public float GasPercent;
        /// <summary>Brake pressure as a fraction (0.0–1.0)</summary>
        public float BrakePercent;
        /// <summary>Handbrake engagement as a fraction (0.0–1.0)</summary>
        public float HandbrakePercent;
        /// <summary>Clutch disengagement as a fraction (1.0–0.0)</summary>
        public float ClutchPercent;
        /// <summary>Steering wheel position (−1.0 = full left, +1.0 = full right)</summary>
        public float SteeringPercent;

        /// <summary>Global force-feedback output strength</summary>
        public float FfbStrength;
        /// <summary>Per-car force-feedback gain multiplier</summary>
        public float CarFfbMultiplier;

        /// <summary>Coolant temperature as a fraction of optimal operating range</summary>
        public float WaterTemperaturePercent;

        /// <summary>Coolant system pressure in bar</summary>
        public float WaterPressureBar;
        /// <summary>Fuel system pressure in bar</summary>
        public float FuelPressureBar;

        /// <summary>Coolant temperature in °C</summary>
        public sbyte WaterTemperatureC;
        /// <summary>Ambient air temperature in °C</summary>
        public sbyte AirTemperatureC;
        /// <summary>Engine oil temperature in °C</summary>
        public float OilTemperatureC;
        /// <summary>Engine oil pressure in bar</summary>
        public float OilPressureBar;
        /// <summary>Exhaust gas temperature in °C</summary>
        public float ExhaustTemperatureC;

        /// <summary>Lateral G-force (positive = rightward)</summary>
        public float GForcesX;
        /// <summary>Longitudinal G-force (positive = under acceleration)</summary>
        public float GForcesY;
        /// <summary>Vertical G-force (positive = upward)</summary>
        public float GForcesZ;

        /// <summary>Absolute turbo boost pressure in bar</summary>
        public float TurboBoost;
        /// <summary>Current boost stage or map level</summary>
        public float TurboBoostLevel;
        /// <summary>Turbo boost as a fraction of maximum (0.0–1.0)</summary>
        public float TurboBoostPerc;

        /// <summary>Steering wheel rotation in degrees from centre</summary>
        public int SteerDegrees;
        /// <summary>Distance driven in the current session in km</summary>
        public float CurrentKm;
        /// <summary>Total odometer / career distance in km</summary>
        public uint TotalKm;
        /// <summary>Total driving time accumulated in seconds</summary>
        public uint TotalDrivingTimeS;

        /// <summary>In-game time of day — hours (0–23)</summary>
        public int TimeOfDayHours;
        /// <summary>In-game time of day — minutes (0–59)</summary>
        public int TimeOfDayMinutes;
        /// <summary>In-game time of day — seconds (0–59)</summary>
        public int TimeOfDaySeconds;

        /// <summary>Delta vs. reference lap in milliseconds (signed)</summary>
        public int DeltaTimeMs;
        /// <summary>Current lap time in milliseconds</summary>
        public int CurrentLapTimeMs;
        /// <summary>Predicted final lap time in milliseconds</summary>
        public int PredictedLapTimeMs;

        /// <summary>Fuel remaining in the tank in litres</summary>
        public float FuelLiterCurrentQuantity;
        /// <summary>Fuel remaining as a fraction of tank capacity</summary>
        public float FuelLiterCurrentQuantityPercent;
        /// <summary>Average fuel consumption rate in litres per km</summary>
        public float FuelLiterPerKm;
        /// <summary>Average fuel economy in km per litre</summary>
        public float KmPerFuelLiter;

        /// <summary>Engine output torque in Nm</summary>
        public float CurrentTorque;
        /// <summary>Engine output power in brake horsepower</summary>
        public int CurrentBhp;

        /// <summary>Full tyre state for the front-left corner</summary>
        public SmevoTyreState TyreLf;
        /// <summary>Full tyre state for the front-right corner</summary>
        public SmevoTyreState TyreRf;
        /// <summary>Full tyre state for the rear-left corner</summary>
        public SmevoTyreState TyreLr;
        /// <summary>Full tyre state for the rear-right corner</summary>
        public SmevoTyreState TyreRr;

        /// <summary>Normalised track position (0.0 = start/finish line, 1.0 = one full lap)</summary>
        public float Npos;

        /// <summary>KERS/ERS charge level as a fraction (0.0–1.0)</summary>
        public float KersChargePerc;
        /// <summary>KERS/ERS power currently being deployed as a fraction</summary>
        public float KersCurrentPerc;

        /// <summary>Seconds driver input remains locked (e.g. after collision penalty)</summary>
        public float ControlLockTime;

        /// <summary>Damage levels for each body zone of the car</summary>
        public SmevoDamageState CarDamage;

        /// <summary>Current track zone the car occupies (see AcEvoCarLocation)</summary>
        public AcEvoCarLocation CarLocation;

        /// <summary>Status of each pit-stop service item</summary>
        public SmevoPitInfo PitInfo;

        /// <summary>Fuel consumed since session start in litres</summary>
        public float FuelLiterUsed;
        /// <summary>Average fuel consumed per lap in litres</summary>
        public float FuelLiterPerLap;
        /// <summary>Estimated number of laps achievable with remaining fuel</summary>
        public float LapsPossibleWithFuel;

        /// <summary>High-voltage battery temperature in °C</summary>
        public float BatteryTemperature;
        /// <summary>High-voltage battery pack voltage in V</summary>
        public float BatteryVoltage;

        /// <summary>Instantaneous fuel consumption in litres per km</summary>
        public float InstantaneousFuelLiterPerKm;
        /// <summary>Instantaneous fuel economy in km per litre</summary>
        public float InstantaneousKmPerFuelLiter;

        /// <summary>How well current RPM suits the engaged gear (1.0 = ideal window)</summary>
        public float GearRpmWindow;

        /// <summary>Current state of all cockpit lights and displays</summary>
        public SmevoInstrumentation Instrumentation;
        /// <summary>Minimum allowed setting for each instrumentation item</summary>
        public SmevoInstrumentation InstrumentationMinLimit;
        /// <summary>Maximum allowed setting for each instrumentation item</summary>
        public SmevoInstrumentation InstrumentationMaxLimit;

        /// <summary>Current electronic aid and setup values</summary>
        public SmevoElectronics Electronics;
        /// <summary>Minimum allowed value for each electronics setting</summary>
        public SmevoElectronics ElectronicsMinLimit;
        /// <summary>Maximum allowed value for each electronics setting</summary>
        public SmevoElectronics ElectronicsMaxLimit;
        /// <summary>Flags which electronics fields the driver can adjust in-session</summary>
        public SmevoElectronics ElectronicsIsModifiable;

        /// <summary>Total laps completed in the session</summary>
        public int TotalLapCount;
        /// <summary>Current race position (1 = leader)</summary>
        public uint CurrentPos;
        /// <summary>Total number of cars in the session</summary>
        public uint TotalDrivers;

        /// <summary>Last completed lap time in milliseconds</summary>
        public int LastLaptimeMs;
        /// <summary>Personal best lap time in milliseconds</summary>
        public int BestLaptimeMs;

        /// <summary>Flag shown specifically to this driver</summary>
        public AcEvoFlagType Flag;
        /// <summary>Flag shown to all drivers on track</summary>
        public AcEvoFlagType GlobalFlag;

        /// <summary>Number of forward gears the car has</summary>
        public uint MaxGears;
        /// <summary>Powertrain type of the car (see AcEvoEngineType)</summary>
        public AcEvoEngineType EngineType;
        /// <summary>Car is equipped with a KERS/ERS system</summary>
        public bool HasKers;
        /// <summary>This is the final scheduled lap of the race</summary>
        public bool IsLastLap;

        /// <summary>Display name of the active vehicle performance / power mode</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string PerformanceModeName;

        /// <summary>Raw differential coast-lock value from setup</summary>
        public float DiffCoastRawValue;
        /// <summary>Raw differential power-lock value from setup</summary>
        public float DiffPowerRawValue;

        /// <summary>Cumulative time penalty from track-limit cuts in ms</summary>
        public int RaceCutGainedTimeMs;
        /// <summary>Distance to the penalty trigger in metres</summary>
        public int DistanceToDeadline;
        /// <summary>Running delta time accrued from track-limit violations</summary>
        public float RaceCutCurrentDelta;

        /// <summary>Session lifecycle and countdown information</summary>
        public SmevoSessionState SessionState;
        /// <summary>HUD lap times and delta display values</summary>
        public SmevoTimingState TimingState;

        /// <summary>Network round-trip ping to the server in ms</summary>
        public int PlayerPing;
        /// <summary>Measured network latency in ms</summary>
        public int PlayerLatency;
        /// <summary>Client CPU usage in percent</summary>
        public int PlayerCpuUsage;
        /// <summary>Average client CPU usage in percent</summary>
        public int PlayerCpuUsageAvg;
        /// <summary>Network Quality-of-Service score</summary>
        public int PlayerQos;
        /// <summary>Average QoS score over the session</summary>
        public int PlayerQosAvg;
        /// <summary>Current rendered frames per second</summary>
        public int PlayerFps;
        /// <summary>Average FPS over the session</summary>
        public int PlayerFpsAvg;

        /// <summary>Driver's first name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string DriverName;
        /// <summary>Driver's surname</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string DriverSurname;
        /// <summary>Identifier or display name of the car model</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string CarModel;

        /// <summary>Car is stationary inside its assigned pit box</summary>
        public bool IsInPitBox;
        /// <summary>Car is anywhere within the pit lane</summary>
        public bool IsInPitLane;
        /// <summary>Current lap is valid and counts for timing</summary>
        public bool IsValidLap;

        /// <summary>World-space position of up to 60 cars [car_index][X, Y, Z]</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        public StructVector3[] CarCoordinates;

        /// <summary>Time gap to the car immediately ahead in seconds</summary>
        public float GapAhead;
        /// <summary>Time gap to the car immediately behind in seconds</summary>
        public float GapBehind;

        /// <summary>Number of cars actively participating in the session</summary>
        public byte ActiveCars;
        /// <summary>Target fuel consumption per lap in litres</summary>
        public float FuelPerLap;
        /// <summary>Estimated laps remaining with current fuel</summary>
        public float FuelEstimatedLaps;

        /// <summary>All driver-assist levels currently active</summary>
        public SmevoAssistsState AssistsState;

        /// <summary>Maximum fuel tank capacity of the car in litres</summary>
        public float MaxFuel;
        /// <summary>Maximum turbo boost pressure in bar</summary>
        public float MaxTurboBoost;
        /// <summary>Car is restricted to a single tyre compound for both axles</summary>
        public bool UseSingleCompound;

        public static readonly int Size = Marshal.SizeOf<SPageFileGraphicEvo>();
        public static readonly byte[] Buffer = new byte[Size];
    }

    /// <summary>Static session metadata. Written once when a session loads and does not change while driving.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public sealed class SPageFileStaticEvo
    {
        /// <summary>Shared-memory interface version string</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string SmVersion;

        /// <summary>AC Evo game build version string</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string AcEvoVersion;

        /// <summary>Type of the current session (see AcEvoSessionType)</summary>
        public AcEvoSessionType Session;

        /// <summary>Human-readable session name (e.g. 'Race 1')</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string SessionName;

        /// <summary>Unique identifier of the event within the championship</summary>
        public byte EventId;
        /// <summary>Unique identifier of this session within the event</summary>
        public byte SessionId;

        /// <summary>Tyre grip condition at session start (see AcEvoStartingGrip)</summary>
        public AcEvoStartingGrip StartingGrip;
        /// <summary>Ambient air temperature at session start in °C</summary>
        public float StartingAmbientTemperatureC;
        /// <summary>Road surface temperature at session start in °C</summary>
        public float StartingGroundTemperatureC;

        /// <summary>Weather is fixed and will not change during the session</summary>
        public bool IsStaticWeather;
        /// <summary>Session ends by elapsed time rather than lap count</summary>
        public bool IsTimedRace;
        /// <summary>Session is an online multiplayer event</summary>
        public bool IsOnline;
        /// <summary>Total sessions in this event (e.g. 3 = practice + qualify + race)</summary>
        public int NumberOfSessions;

        /// <summary>Country / nation name associated with the event or track</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string Nation;

        /// <summary>Geographic longitude of the track location in decimal degrees</summary>
        public float Longitude;
        /// <summary>Geographic latitude of the track location in decimal degrees</summary>
        public float Latitude;

        /// <summary>Track identifier or name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string Track;

        /// <summary>Track layout variant or configuration name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string TrackConfiguration;

        /// <summary>Total lap length of the track in metres</summary>
        public float TrackLengthM;

        public static readonly int Size = Marshal.SizeOf<SPageFileStaticEvo>();
        public static readonly byte[] Buffer = new byte[Size];
    }

    #region Read methods

    /// <summary>Reads the graphics (HUD) shared memory page.</summary>
    public SPageFileGraphicEvo ReadGraphicsPageFile(bool fromCache = false)
    {
        if (fromCache) return PageFileGraphic;
        return PageFileGraphic = StructExtension.ToStruct<SPageFileGraphicEvo>(
            MemoryMappedFile.CreateOrOpen(graphicsMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite),
            SPageFileGraphicEvo.Buffer);
    }

    /// <summary>Reads the static session metadata shared memory page.</summary>
    public SPageFileStaticEvo ReadStaticPageFile(bool fromCache = false)
    {
        if (fromCache) return PageFileStatic;
        return PageFileStatic = StructExtension.ToStruct<SPageFileStaticEvo>(
            MemoryMappedFile.CreateOrOpen(staticMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite),
            SPageFileStaticEvo.Buffer);
    }

    /// <summary>Reads the raw physics telemetry shared memory page.</summary>
    public SPageFilePhysicsEvo ReadPhysicsPageFile(bool fromCache = false)
    {
        if (fromCache) return PageFilePhysics;
        return PageFilePhysics = StructExtension.ToStruct<SPageFilePhysicsEvo>(
            MemoryMappedFile.CreateOrOpen(physicsMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite),
            SPageFilePhysicsEvo.Buffer);
    }

    #endregion
}