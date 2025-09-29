using System.Runtime.InteropServices;

namespace RaceElement.Data.Games.WRC_Generations.UDP;

[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
internal struct WRCGenData
{
    /// <summary>
    /// Total Time (not reset after stage restart)
    /// </summary>
    public float TotalTime;

    /// <summary>
    /// Current Lap/Stage Time (starts on Go!)
    /// </summary>
    public float CurrentLapTime;

    /// <summary>
    /// Current Lap/Stage Distance (meters)
    /// </summary>
    public float CurrentLapDistance;

    /// <summary>
    /// ? (starts from 0) - if distance then not equal to above!
    /// </summary>
    public float TotalDistance;

    /// <summary>
    /// World space position X
    /// </summary>
    public float PositionX;

    /// <summary>
    /// World space position Y
    /// </summary>
    public float PositionY;

    /// <summary>
    /// World space position Z
    /// </summary>
    public float PositionZ;

    /// <summary>
    /// Velocity (Speed) [m/s]
    /// </summary>
    public float Speed;

    /// <summary>
    /// Velocity in world space X
    /// </summary>
    public float VelocityX;

    /// <summary>
    /// Velocity in world space Y
    /// </summary>
    public float VelocityY;

    /// <summary>
    /// Velocity in world space Z
    /// </summary>
    public float VelocityZ;

    /// <summary>
    /// World space right direction (Roll Vector X)
    /// </summary>
    public float LocalRightX;

    /// <summary>
    /// World space right direction (Roll Vector Y)
    /// </summary>
    public float LocalRightY;

    /// <summary>
    /// World space right direction (Roll Vector Z)
    /// </summary>
    public float LocalRightZ;

    /// <summary>
    /// World space forward direction (Pitch Vector X)
    /// </summary>
    public float LocalForwardX;

    /// <summary>
    /// World space forward direction (Pitch Vector Y)
    /// </summary>
    public float LocalForwardY;

    /// <summary>
    /// World space forward direction (Pitch Vector Z)
    /// </summary>
    public float LocalForwardZ;

    /// <summary>
    /// Position of Suspension Rear Left
    /// </summary>
    public float SuspensionPositionRearLeft;

    /// <summary>
    /// Position of Suspension Rear Right
    /// </summary>
    public float SuspensionPositionRearRight;

    /// <summary>
    /// Position of Suspension Front Left
    /// </summary>
    public float SuspensionPositionFrontLeft;

    /// <summary>
    /// Position of Suspension Front Right
    /// </summary>
    public float SuspensionPositionFrontRight;

    /// <summary>
    /// Velocity of Suspension Rear Left
    /// </summary>
    public float SuspensionVelocityRearLeft;

    /// <summary>
    /// Velocity of Suspension Rear Right
    /// </summary>
    public float SuspensionVelocityRearRight;

    /// <summary>
    /// Velocity of Suspension Front Left
    /// </summary>
    public float SuspensionVelocityFrontLeft;

    /// <summary>
    /// Velocity of Suspension Front Right
    /// </summary>
    public float SuspensionVelocityFrontRight;

    /// <summary>
    /// Velocity of Wheel Rear Left
    /// </summary>
    public float WheelSpeedRearLeft;

    /// <summary>
    /// Velocity of Wheel Rear Right
    /// </summary>
    public float WheelSpeedRearRight;

    /// <summary>
    /// Velocity of Wheel Front Left
    /// </summary>
    public float WheelSpeedFrontLeft;

    /// <summary>
    /// Velocity of Wheel Front Right
    /// </summary>
    public float WheelSpeedFrontRight;

    /// <summary>
    /// Position Throttle
    /// </summary>
    public float Throttle;

    /// <summary>
    /// Position Steer
    /// </summary>
    public float SteerAngle;

    /// <summary>
    /// Position Brake
    /// </summary>
    public float Brake;

    /// <summary>
    /// Position Clutch
    /// </summary>
    public float Clutch;

    /// <summary>
    /// Gear [0 = Neutral, 1 = 1, 2 = 2, ..., -1 = Reverse]
    /// </summary>
    public float CurrentGear;

    /// <summary>
    /// G-Force Lateral
    /// </summary>
    public float LateralGForce;

    /// <summary>
    /// G-Force Longitudinal
    /// </summary>
    public float LongitudinalGForce;

    /// <summary>
    /// Current Lap (rx only)
    /// </summary>
    public float CurrentLap;

    /// <summary>
    /// Engine Speed [rpm / 10]
    /// </summary>
    public float EngineRpms;

    /// <summary>
    /// SLI Pro support Not used
    /// </summary>
    public float SliProNativeSupport;

    /// <summary>
    /// Car race position Current Position (rx only)
    /// </summary>
    public float CarPosition;

    /// <summary>
    /// Kers energy left Not used
    /// </summary>
    public float KersLevel;

    /// <summary>
    /// Kers maximum energy Not used
    /// </summary>
    public float KersMaxLevel;

    /// <summary>
    /// 0 = off, 1 = on Not used
    /// </summary>
    public float Drs;

    /// <summary>
    /// 0 (off) - 2 (high) Not used
    /// </summary>
    public float TractionControl;

    /// <summary>
    /// 0 (off) - 1 (on) Not used
    /// </summary>
    public float AntiLockBrakes;

    /// <summary>
    /// Current fuel mass Not used
    /// </summary>
    public float FuelInTank;

    /// <summary>
    /// Fuel capacity Not used
    /// </summary>
    public float FuelCapacity;

    /// <summary>
    /// 0 = none, 1 = pitting, 2 = in pit area Not used
    /// </summary>
    public float InPits;

    /// <summary>
    /// 0 = sector1, 1 = sector2, 2 = sector3 Sector
    /// </summary>
    public float CurrentSector;

    /// <summary>
    /// Time of sector1 (or 0) Sector 1 time
    /// </summary>
    public float Sector1Time;

    /// <summary>
    /// Time of sector2 (or 0) Sector 2 time
    /// </summary>
    public float Sector2Time;

    /// <summary>
    /// Brakes temperature (centigrade) Temperature Brake in C (assume FL)
    /// </summary>
    public float BrakeTemp0;

    /// <summary>
    /// Brakes temperature (centigrade) (assume FR)
    /// </summary>
    public float BrakeTemp1;

    /// <summary>
    /// Brakes temperature (centigrade) (assume RL)
    /// </summary>
    public float BrakeTemp2;

    /// <summary>
    /// Brakes temperature (centigrade) (assume RR)
    /// </summary>
    public float BrakeTemp3;

    /// <summary>
    /// Wheels pressure PSI Wheel pressure (assume FL)
    /// </summary>
    public float WheelPressure0;

    /// <summary>
    /// Wheels pressure PSI (assume FR)
    /// </summary>
    public float WheelPressure1;

    /// <summary>
    /// Wheels pressure PSI (assume RL)
    /// </summary>
    public float WheelPressure2;

    /// <summary>
    /// Wheels pressure PSI (assume RR)
    /// </summary>
    public float WheelPressure3;

    /// <summary>
    /// Team ID Not used
    /// </summary>
    public float TeamInfo;

    /// <summary>
    /// Total number of laps in this race Total laps of the race (if SSS)
    /// </summary>
    public float TotalLaps;

    /// <summary>
    /// Not used track size meters
    /// </summary>
    public float TrackSize;

    /// <summary>
    /// Last lap time Last lap time (if SSS)
    /// </summary>
    public float LastLapTime;

    /// <summary>
    /// Cars max RPM, at which point the rev limiter will kick in Max rpm
    /// </summary>
    public float MaxRpms;

    /// <summary>
    /// Cars idle RPM Idle rpm
    /// </summary>
    public float IdleRpms;

    /// <summary>
    /// Maximum number of gears Number of gears
    /// </summary>
    public float MaxGears;

    /// <summary>
    /// 0 = unknown, 1 = practice, 2 = qualifying, 3 = race Not used
    /// </summary>
    public float SessionType;

    /// <summary>
    /// 0 = not allowed, 1 = allowed, -1 = invalid / unknown Not used
    /// </summary>
    public float DrsAllowed;

    /// <summary>
    /// -1 for unknown, 0-21 for tracks Not used
    /// </summary>
    public float TrackNumber;

    /// <summary>
    /// -1 = invalid/unknown, 0 = none, 1 = green, 2 = blue, 3 = yellow, 4 = red
    /// </summary>
    public float VehicleFiaFlags;
}
