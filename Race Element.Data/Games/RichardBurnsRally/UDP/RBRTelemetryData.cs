using System.Runtime.InteropServices;

namespace RaceElement.Data.Games.RichardBurnsRally.UDP;

/// <summary>
/// RBR NGP (Next Generation Physics) UDP telemetry packet structure.
/// Requires NGP plugin with udpTelemetry=1 in RichardBurnsRally.ini
/// Port: 6776 (default)
/// Structure based on tmp/Adaptive_Trigger_RBR.py TelemetryData and NGP documentation.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RBRTelemetryData
{
    // Offset 0
    public uint TotalSteps;

    // Stage - Offset 4
    public int StageIndex;
    public float StageProgress;
    public float StageRaceTime;
    public float StageDriveLineLocation;
    public float StageDistanceToEnd;

    // Control - Offset 24
    public float ControlSteering;
    public float ControlThrottle;
    public float ControlBrake;
    public float ControlHandbrake;
    public float ControlClutch;
    public int ControlGear;
    public float ControlFootbrakePressure;
    public float ControlHandbrakePressure;

    // Car - Offset 56
    public int CarIndex;
    public float CarSpeed;

    // Position - Offset 64
    public float CarPositionX;
    public float CarPositionY;
    public float CarPositionZ;

    // Rotation - Offset 76
    public float CarRoll;
    public float CarPitch;
    public float CarYaw;

    // Velocities (Surge, Sway, Heave, Roll, Pitch, Yaw) - Offset 88
    public float VelSurge;
    public float VelSway;
    public float VelHeave;
    public float VelRoll;
    public float VelPitch;
    public float VelYaw;

    // Accelerations - Offset 112
    public float AccSurge;
    public float AccSway;
    public float AccHeave;
    public float AccRoll;
    public float AccPitch;
    public float AccYaw;

    // Engine - Offset 136
    public float EngineRpm;
    public float EngineRadiatorCoolantTemp;
    public float EngineCoolantTemp;
    public float EngineTemp;
}
