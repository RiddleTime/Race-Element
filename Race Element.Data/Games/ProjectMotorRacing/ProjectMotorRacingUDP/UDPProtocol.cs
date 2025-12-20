namespace RaceElement.Data.Games.ProjectMotorRacing.ProjectMotorRacingUDP;

//
// packet type
enum UDPPacketType
{
    RaceInfo = 0,
    ParticipantRaceState = 1,
    ParticipantVehicleTelemetry = 2
};


///////////////////////////////////////////////////////////////////////////////////////
// RACE INFO PACKET
///////////////////////////////////////////////////////////////////////////////////////

// Session State
public enum UDPRaceSessionState
{
    Inactive = 0,
    Active = 1,
    Complete = 2
};

public sealed class UDPRaceInfo
{
    public static UDPRaceInfo Decode(ref byte[] data, int startIdx)
    {
        UDPRaceInfo p = new UDPRaceInfo();

        int dataIdx = startIdx;
        ConversionHelper.ConvertUInt16(ref data, ref dataIdx, out p.m_packetVersion);

        if (p.m_packetVersion == m_expectedPacketVersion)
        {
            ConversionHelper.ConvertString(ref data, ref dataIdx, out p.m_track);
            ConversionHelper.ConvertString(ref data, ref dataIdx, out p.m_layout);
            ConversionHelper.ConvertString(ref data, ref dataIdx, out p.m_season);
            ConversionHelper.ConvertString(ref data, ref dataIdx, out p.m_weather);
            ConversionHelper.ConvertString(ref data, ref dataIdx, out p.m_session);
            ConversionHelper.ConvertString(ref data, ref dataIdx, out p.m_gameMode);

            ConversionHelper.ConvertFloat(ref data, ref dataIdx, out p.m_layoutLength);
            ConversionHelper.ConvertFloat(ref data, ref dataIdx, out p.m_duration);
            ConversionHelper.ConvertFloat(ref data, ref dataIdx, out p.m_overtime);
            ConversionHelper.ConvertFloat(ref data, ref dataIdx, out p.m_ambientTemperature);
            ConversionHelper.ConvertFloat(ref data, ref dataIdx, out p.m_trackTemperature);

            ConversionHelper.ConvertBool(ref data, ref dataIdx, out p.m_isLaps);

            byte state = 0;
            ConversionHelper.ConvertByte(ref data, ref dataIdx, out state);
            p.m_state = (UDPRaceSessionState)state;

            ConversionHelper.ConvertByte(ref data, ref dataIdx, out p.m_numParticipants);
        }
        else
        {
            Console.WriteLine("Failed version check! Expected version: {0} - Got Version: {1}", m_expectedPacketVersion, p.m_packetVersion);
        }

        return p;
    }

    public override string ToString()
    {
        return string.Format("Track\t\t: {0}\r\n Layout\t\t: {1}\r\n Weather\t\t: {2}\r\n Season\t\t: {3}\r\n Session\t\t: {4}\r\n Game Mode\t: {5}\r\n Layout Length\t: {6} m\r\n Duration\t\t: {7} s\r\n Overtime\t\t: {8} s\r\n Ambient Temp\t: {9} °C\r\n Track Temp\t: {10} °C\r\n IsLaps\t\t: {11}\r\n State\t\t: {12}\r\n Num Participants\t: {13}\r\n",
                                m_track, m_layout, m_weather, m_season, m_session, m_gameMode, m_layoutLength, m_duration, m_overtime, m_ambientTemperature, m_trackTemperature, m_isLaps, m_state, m_numParticipants);
    }

    ushort m_packetVersion = 0;
    string m_track = string.Empty;
    string m_layout = string.Empty;
    string m_season = string.Empty;
    string m_weather = string.Empty;
    string m_session = string.Empty;
    string m_gameMode = string.Empty;
    float m_layoutLength = 0.0f;
    float m_duration = 0.0f;
    float m_overtime = 0.0f;
    float m_ambientTemperature = 0.0f;
    float m_trackTemperature = 0.0f;
    bool m_isLaps = false;
    public UDPRaceSessionState m_state;
    public byte m_numParticipants = 0;

    public static ushort m_expectedPacketVersion = 1;
};


///////////////////////////////////////////////////////////////////////////////////////
// PARTICIPANT RACE STATE PACKET
///////////////////////////////////////////////////////////////////////////////////////
public sealed class UDPParticipantRaceState : IComparable<UDPParticipantRaceState>
{
    public UDPParticipantRaceState()
    {
    }

    public UDPParticipantRaceState(UDPParticipantRaceState other)
    {
        m_packetVersion = other.m_packetVersion;
        m_vehicleId = other.m_vehicleId;
        m_isPlayer = other.m_isPlayer;
        m_vehicleName = other.m_vehicleName;
        m_driverName = other.m_driverName;
        m_liveryId = other.m_liveryId;
        m_vehicleClass = other.m_vehicleClass;

        m_racePos = other.m_racePos;
        m_currentLap = other.m_currentLap;

        m_currentLapTime = other.m_currentLapTime;
        m_bestLapTime = other.m_bestLapTime;

        m_inPits = other.m_inPits;
        m_sessionFinished = other.m_sessionFinished;
        m_dq = other.m_dq;
        m_flags = other.m_flags;

        m_currentSector = other.m_currentSector;
        m_sectorTimes = new List<float>(other.m_sectorTimes);
        m_bestSectorTimes = new List<float>(other.m_bestSectorTimes);
    }


    public static UDPParticipantRaceState Decode(ref byte[] data, int startIdx)
    {
        UDPParticipantRaceState p = new UDPParticipantRaceState();

        int dataIdx = startIdx;
        ConversionHelper.ConvertUInt16(ref data, ref dataIdx, out p.m_packetVersion);

        if (m_expectedPacketVersion == p.m_packetVersion)
        {
            ConversionHelper.ConvertInt32(ref data, ref dataIdx, out p.m_vehicleId);
            ConversionHelper.ConvertBool(ref data, ref dataIdx, out p.m_isPlayer);
            ConversionHelper.ConvertString(ref data, ref dataIdx, out p.m_vehicleName);
            ConversionHelper.ConvertString(ref data, ref dataIdx, out p.m_driverName);
            ConversionHelper.ConvertString(ref data, ref dataIdx, out p.m_liveryId);
            ConversionHelper.ConvertString(ref data, ref dataIdx, out p.m_vehicleClass);

            ConversionHelper.ConvertInt32(ref data, ref dataIdx, out p.m_racePos);
            ConversionHelper.ConvertInt32(ref data, ref dataIdx, out p.m_currentLap);

            ConversionHelper.ConvertFloat(ref data, ref dataIdx, out p.m_currentLapTime);
            ConversionHelper.ConvertFloat(ref data, ref dataIdx, out p.m_bestLapTime);
            ConversionHelper.ConvertFloat(ref data, ref dataIdx, out p.m_lapProgress);

            ConversionHelper.ConvertInt32(ref data, ref dataIdx, out p.m_currentSector);

            {
                byte numSectors = 0;
                ConversionHelper.ConvertByte(ref data, ref dataIdx, out numSectors);
                for (byte i = 0; i < numSectors; ++i)
                {
                    float f = 0.0f;
                    ConversionHelper.ConvertFloat(ref data, ref dataIdx, out f);
                    p.m_sectorTimes.Add(f);
                }
            }

            {
                byte numSectors = 0;
                ConversionHelper.ConvertByte(ref data, ref dataIdx, out numSectors);
                for (byte i = 0; i < numSectors; ++i)
                {
                    float f = 0.0f;
                    ConversionHelper.ConvertFloat(ref data, ref dataIdx, out f);
                    p.m_bestSectorTimes.Add(f);
                }
            }

            ConversionHelper.ConvertBool(ref data, ref dataIdx, out p.m_inPits);
            ConversionHelper.ConvertBool(ref data, ref dataIdx, out p.m_sessionFinished);
            ConversionHelper.ConvertBool(ref data, ref dataIdx, out p.m_dq);
            ConversionHelper.ConvertUInt32(ref data, ref dataIdx, out p.m_flags);
        }
        else
        {
            Console.WriteLine("Failed version check! Expected version: {0} - Got Version: {1}", m_expectedPacketVersion, p.m_packetVersion);
        }

        return p;
    }

    public sealed override string ToString()
    {
        if (string.IsNullOrEmpty(m_driverName))
        {
            return string.Empty;
        }

        return string.Format(" {0}. {1}", m_racePos, m_driverName);
    }

    public int CompareTo(UDPParticipantRaceState other) => m_racePos.CompareTo(other.m_racePos);

    public ushort m_packetVersion = 0;
    public int m_vehicleId = -1;
    public bool m_isPlayer = false;

    public string m_vehicleName = string.Empty;
    public string m_driverName = string.Empty;
    public string m_liveryId = string.Empty;
    public string m_vehicleClass = string.Empty;

    public int m_racePos;
    public int m_currentLap;

    public float m_currentLapTime;
    public float m_bestLapTime;
    public float m_lapProgress;

    public int m_currentSector;
    public List<float> m_sectorTimes = [];
    public List<float> m_bestSectorTimes = [];

    public bool m_inPits = false;
    public bool m_sessionFinished = false;
    public bool m_dq = false;
    public uint m_flags;

    public static ushort m_expectedPacketVersion = 1;
};




///////////////////////////////////////////////////////////////////////////////////////
// PARTICIPANT VEHICLE TELEMETRY
///////////////////////////////////////////////////////////////////////////////////////

//
// structure for a 3d vector
public sealed class UDPVec3
{
    public float x = 0.0f;
    public float y = 0.0f;
    public float z = 0.0f;
};


//
// structure for a quaternion
public sealed class UDPQuat
{
    public float x = 0.0f;
    public float y = 0.0f;
    public float z = 0.0f;
    public float w = 0.0f;
};

//
// structure for a single telemetry wheel data
public sealed class UDPVehicleTelemetryWheel
{
    // contact data
    int m_contactMaterialHash = 0;

    // wheel data
    float m_angVel = 0.0f;
    float m_linearSpeed = 0.0f;

    // tyre data
    UDPVec3 m_slideLS;
    UDPVec3 m_forceLS;
    UDPVec3 m_momentLS;
    float m_contactRadius = 0.0f;
    public float m_pressure = 0.0f;
    float m_inclination = 0.0f;
    public float m_slipRatio = 0.0f;
    public float m_slipAngle = 0.0f;

    // thermodynamics data
    UDPVec3 m_tread;
    float m_carcass = 0.0f;
    public float m_internalAir = 0.0f;
    public float m_wellAir = 0.0f;
    public float m_rim = 0.0f;
    public float m_brake = 0.0f;

    // suspension data
    float m_springStrain = 0.0f;
    float m_damperVelocity = 0.0f;

    // drivetrain data
    float m_hubTorque = 0.0f;
    float m_hubPower = 0.0f;
    float m_wheelTorque = 0.0f;
    float m_wheelPower = 0.0f;

    internal static UDPVehicleTelemetryWheel Decode(ref byte[] data, ref int dataIdx)
    {
        UDPVehicleTelemetryWheel wheel = new UDPVehicleTelemetryWheel();

        // contact
        ConversionHelper.ConvertInt32(ref data, ref dataIdx, out wheel.m_contactMaterialHash);

        // wheel
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_angVel);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_linearSpeed);

        // tyre
        ConversionHelper.ConvertVec(ref data, ref dataIdx, out wheel.m_slideLS);
        ConversionHelper.ConvertVec(ref data, ref dataIdx, out wheel.m_forceLS);
        ConversionHelper.ConvertVec(ref data, ref dataIdx, out wheel.m_momentLS);

        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_contactRadius);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_pressure);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_inclination);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_slipRatio);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_slipAngle);

        // thermodynamics data
        ConversionHelper.ConvertVec(ref data, ref dataIdx, out wheel.m_tread);

        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_carcass);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_internalAir);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_wellAir);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_rim);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_brake);

        // suspension data
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_springStrain);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_damperVelocity);


        // drivetrain data
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_hubTorque);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_hubPower);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_wheelTorque);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out wheel.m_wheelPower);

        return wheel;
    }
};

//
// structure for the telemetry chassis data
public sealed class UDPVehicleTelemetryChassis
{
    public UDPVec3 m_posWS;
    public UDPQuat m_quat;
    public UDPVec3 m_angularVelocityWS;
    public UDPVec3 m_angularVelocityLS;
    public UDPVec3 m_velocityWS;
    public UDPVec3 m_velocityLS;
    public UDPVec3 m_accelerationWS;
    public UDPVec3 m_accelerationLS;
    public float m_overallSpeed = 0.0f;
    public float m_forwardSpeed = 0.0f;
    public float m_sideslip = 0.0f;

    internal static UDPVehicleTelemetryChassis Decode(ref byte[] data, ref int dataIdx)
    {
        UDPVehicleTelemetryChassis c = new UDPVehicleTelemetryChassis();
        ConversionHelper.ConvertVec(ref data, ref dataIdx, out c.m_posWS);
        ConversionHelper.ConvertQuat(ref data, ref dataIdx, out c.m_quat);
        ConversionHelper.ConvertVec(ref data, ref dataIdx, out c.m_angularVelocityWS);
        ConversionHelper.ConvertVec(ref data, ref dataIdx, out c.m_angularVelocityLS);
        ConversionHelper.ConvertVec(ref data, ref dataIdx, out c.m_velocityWS);
        ConversionHelper.ConvertVec(ref data, ref dataIdx, out c.m_velocityLS);
        ConversionHelper.ConvertVec(ref data, ref dataIdx, out c.m_accelerationWS);
        ConversionHelper.ConvertVec(ref data, ref dataIdx, out c.m_accelerationLS);

        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_overallSpeed);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_forwardSpeed);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_sideslip);

        return c;
    }
};

//
// structure for the telemetry drivetrain data
public sealed class UDPVehicleTelemetryGear
{
    // ICE
    public float m_upshiftRPM = 0.0f;
    public float m_downshiftRPM = 0.0f;

    internal static UDPVehicleTelemetryGear Decode(ref byte[] data, ref int dataIdx)
    {
        UDPVehicleTelemetryGear gear = new UDPVehicleTelemetryGear();

        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out gear.m_upshiftRPM);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out gear.m_downshiftRPM);

        return gear;
    }
};

//
// structure for the telemetry drivetrain data
public sealed class UDPVehicleTelemetryDrivetrain
{
    // ICE
    public float m_engineRPM = 0.0f;
    public float m_engineRevRatio = 0.0f;
    public float m_engineTorque = 0.0f;
    public float m_enginePower = 0.0f;
    public float m_engineLoad = 0.0f;
    public float m_engineTurboRPM = 0.0f;
    public float m_engineTurboBoostPressure = 0.0f;
    public float m_fuelRemaining = 0.0f;
    public float m_fuelUseRate = 0.0f;
    public float m_engineOilPressure = 0.0f;
    public float m_engineOilTemperature = 0.0f;
    public float m_engineCoolantTemperature = 0.0f;
    public float m_exhaustGasTemperature = 0.0f;

    //MGU
    public float m_motorRPM = 0.0f;
    public float m_batteryRemaining = 0.0f;
    public float m_batteryUseRate = 0.0f;

    // drivetrain
    public float m_transmissionRPM = 0.0f;
    public float m_gearboxInputRPM = 0.0f;
    public float m_gearboxOutputRPM = 0.0f;
    public float m_gearboxTorque = 0.0f;
    public float m_gearboxPower = 0.0f;
    public float m_gearboxLoadIn = 0.0f;
    public float m_gearboxLoadOut = 0.0f;
    public float m_timeSinceShift = 0.0f;
    public float m_estDrivenSpeed = 0.0f;
    public float m_outputTorque = 0.0f;
    public float m_outputPower = 0.0f;
    public float m_outputEfficiency = 0.0f;

    public bool m_starterActive = false;
    public bool m_engineRunning = false;
    public bool m_engineFanRunning = false;
    public bool m_revLimiterActive = false;
    public bool m_tractionControlActive = false;
    public bool m_speedLimiterEnabled = false;
    public bool m_speedLimiterActive = false;

    public List<UDPVehicleTelemetryGear> m_gears = new List<UDPVehicleTelemetryGear>();

    internal static UDPVehicleTelemetryDrivetrain Decode(ref byte[] data, ref int dataIdx)
    {
        UDPVehicleTelemetryDrivetrain drivetrain = new UDPVehicleTelemetryDrivetrain();

        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_engineRPM);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_engineRevRatio);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_engineTorque);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_enginePower);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_engineLoad);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_engineTurboRPM);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_engineTurboBoostPressure);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_fuelRemaining);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_fuelUseRate);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_engineOilPressure);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_engineOilTemperature);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_engineCoolantTemperature);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_exhaustGasTemperature);

        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_motorRPM);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_batteryRemaining);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_batteryUseRate);

        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_transmissionRPM);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_gearboxInputRPM);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_gearboxOutputRPM);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_gearboxTorque);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_gearboxPower);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_gearboxLoadIn);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_gearboxLoadOut);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_timeSinceShift);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_estDrivenSpeed);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_outputTorque);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_outputPower);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out drivetrain.m_outputEfficiency);

        ConversionHelper.ConvertBool(ref data, ref dataIdx, out drivetrain.m_starterActive);
        ConversionHelper.ConvertBool(ref data, ref dataIdx, out drivetrain.m_engineRunning);
        ConversionHelper.ConvertBool(ref data, ref dataIdx, out drivetrain.m_engineFanRunning);
        ConversionHelper.ConvertBool(ref data, ref dataIdx, out drivetrain.m_revLimiterActive);
        ConversionHelper.ConvertBool(ref data, ref dataIdx, out drivetrain.m_tractionControlActive);
        ConversionHelper.ConvertBool(ref data, ref dataIdx, out drivetrain.m_speedLimiterEnabled);
        ConversionHelper.ConvertBool(ref data, ref dataIdx, out drivetrain.m_speedLimiterActive);

        byte numGears = 0;
        ConversionHelper.ConvertByte(ref data, ref dataIdx, out numGears);

        for (byte i = 0; i < numGears; ++i)
        {
            UDPVehicleTelemetryGear g = UDPVehicleTelemetryGear.Decode(ref data, ref dataIdx);
            drivetrain.m_gears.Add(g);
        }

        return drivetrain;
    }
};


//
// structure for the telemetry suspension data
public sealed class UDPVehicleTelemetrySuspension
{
    List<float> m_avgLoads = new List<float>();
    float m_loadBias = 0.0f;

    internal static UDPVehicleTelemetrySuspension Decode(ref byte[] data, ref int dataIdx)
    {
        UDPVehicleTelemetrySuspension suspension = new UDPVehicleTelemetrySuspension();
        byte num = 0;
        ConversionHelper.ConvertByte(ref data, ref dataIdx, out num);
        for (byte i = 0; i < num; ++i)
        {
            float f = 0.0f;
            ConversionHelper.ConvertFloat(ref data, ref dataIdx, out f);

            suspension.m_avgLoads.Add(f);
        }

        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out suspension.m_loadBias);

        return suspension;
    }
};


//
// structure for the telemetry input data
public sealed class UDPVehicleTelemetryInput
{
    public float m_steering = 0.0f;
    public float m_accelerator = 0.0f;
    public float m_brake = 0.0f;
    public float m_clutch = 0.0f;
    public float m_handbrake = 0.0f;
    public int m_gear = 0;

    internal static UDPVehicleTelemetryInput Decode(ref byte[] data, ref int dataIdx)
    {
        UDPVehicleTelemetryInput i = new UDPVehicleTelemetryInput();
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out i.m_steering);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out i.m_accelerator);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out i.m_brake);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out i.m_clutch);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out i.m_handbrake);
        ConversionHelper.ConvertInt32(ref data, ref dataIdx, out i.m_gear);

        return i;
    }
};


//
// structure for the telemetry input data
public sealed class UDPVehicleTelemetrySetup
{
    public float m_brakeBias = 0.0f;
    public float m_frontAntiRollStiffness = 0.0f;
    public float m_rearAntiRollStiffness = 0.0f;
    public float m_regenLimit = 0.0f;
    public float m_deployLimit = 0.0f;
    public byte m_absLevel = 0;
    public byte m_tcsLevel = 0;

    internal static UDPVehicleTelemetrySetup Decode(ref byte[] data, ref int dataIdx)
    {
        UDPVehicleTelemetrySetup s = new UDPVehicleTelemetrySetup();
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out s.m_brakeBias);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out s.m_frontAntiRollStiffness);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out s.m_rearAntiRollStiffness);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out s.m_regenLimit);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out s.m_deployLimit);
        ConversionHelper.ConvertByte(ref data, ref dataIdx, out s.m_absLevel);
        ConversionHelper.ConvertByte(ref data, ref dataIdx, out s.m_tcsLevel);

        return s;
    }
};


//
// structure for the general telemetry vehicle data
public sealed class UDPVehicleTelemetryGeneral
{
    public UDPVec3 m_centerOfGravity;
    public float m_steeringWheelAngle = 0.0f;
    public float m_totalMass = 0.0f;
    public float m_drivenWheelAngVel = 0.0f;
    public float m_nonDrivenWheelAngVel = 0.0f;
    public float m_estRollingSpeed = 0.0f;
    public float m_estLinearSpeed = 0.0f;
    public float m_totalBrakeForce = 0.0f;
    public bool m_absActive = false;

    internal static UDPVehicleTelemetryGeneral Decode(ref byte[] data, ref int dataIdx)
    {
        UDPVehicleTelemetryGeneral g = new UDPVehicleTelemetryGeneral();

        ConversionHelper.ConvertVec(ref data, ref dataIdx, out g.m_centerOfGravity);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out g.m_steeringWheelAngle);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out g.m_totalMass);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out g.m_drivenWheelAngVel);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out g.m_nonDrivenWheelAngVel);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out g.m_estRollingSpeed);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out g.m_estLinearSpeed);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out g.m_totalBrakeForce);
        ConversionHelper.ConvertBool(ref data, ref dataIdx, out g.m_absActive);

        return g;
    }
};


//
// structure for the general telemetry vehicle data
public sealed class UDPVehicleTelemetryConstant
{
    public UDPVec3 m_chassisBBMin;
    public UDPVec3 m_chassisBBMax;
    public float m_starterIdleRPM;
    public float m_engineTorquePeakRPM;
    public float m_enginePowerPeakRPM;
    public float m_engineMaxRPM;
    public float m_engineMaxTorque;
    public float m_engineMaxPower;
    public float m_engineMaxBoost;
    public float m_fuelCapacity;
    public float m_batteryCapacity;
    public float m_trackWidthFront;
    public float m_trackWidthRear;
    public float m_wheelbase;
    public byte m_numberOfWheels;
    public byte m_numberOfForwardGears;
    public byte m_numberOfReverseGears;
    public bool m_isHybrid;

    internal static UDPVehicleTelemetryConstant Decode(ref byte[] data, ref int dataIdx)
    {
        UDPVehicleTelemetryConstant c = new UDPVehicleTelemetryConstant();

        ConversionHelper.ConvertVec(ref data, ref dataIdx, out c.m_chassisBBMin);
        ConversionHelper.ConvertVec(ref data, ref dataIdx, out c.m_chassisBBMax);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_starterIdleRPM);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_engineTorquePeakRPM);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_enginePowerPeakRPM);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_engineMaxRPM);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_engineMaxTorque);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_engineMaxPower);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_engineMaxBoost);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_fuelCapacity);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_batteryCapacity);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_trackWidthFront);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_trackWidthRear);
        ConversionHelper.ConvertFloat(ref data, ref dataIdx, out c.m_wheelbase);
        ConversionHelper.ConvertByte(ref data, ref dataIdx, out c.m_numberOfWheels);
        ConversionHelper.ConvertByte(ref data, ref dataIdx, out c.m_numberOfForwardGears);
        ConversionHelper.ConvertByte(ref data, ref dataIdx, out c.m_numberOfReverseGears);
        ConversionHelper.ConvertBool(ref data, ref dataIdx, out c.m_isHybrid);

        return c;
    }
};


//
// structure for the telemetry of a single vehicle
public sealed class UDPVehicleTelemetry
{
    ushort m_packetVersion = 0;
    public int m_vehicleId = -1;

    public List<UDPVehicleTelemetryWheel> m_wheels = new List<UDPVehicleTelemetryWheel>();
    public UDPVehicleTelemetryChassis m_chassis = new UDPVehicleTelemetryChassis();
    public UDPVehicleTelemetryDrivetrain m_drivetrain = new UDPVehicleTelemetryDrivetrain();
    public UDPVehicleTelemetrySuspension m_suspension = new UDPVehicleTelemetrySuspension();
    public UDPVehicleTelemetryInput m_input = new UDPVehicleTelemetryInput();
    public UDPVehicleTelemetrySetup m_setup = new UDPVehicleTelemetrySetup();
    public UDPVehicleTelemetryGeneral m_general = new UDPVehicleTelemetryGeneral();
    public UDPVehicleTelemetryConstant m_constant = new UDPVehicleTelemetryConstant();

    internal static UDPVehicleTelemetry Decode(ref byte[] data, int startIdx)
    {
        UDPVehicleTelemetry p = new UDPVehicleTelemetry();

        int dataIdx = startIdx;
        ConversionHelper.ConvertUInt16(ref data, ref dataIdx, out p.m_packetVersion);

        if (m_expectedPacketVersion == p.m_packetVersion)
        {
            ConversionHelper.ConvertInt32(ref data, ref dataIdx, out p.m_vehicleId);

            byte numWheels = 0;
            ConversionHelper.ConvertByte(ref data, ref dataIdx, out numWheels);

            for (byte i = 0; i < numWheels; ++i)
            {
                UDPVehicleTelemetryWheel w = UDPVehicleTelemetryWheel.Decode(ref data, ref dataIdx);
                p.m_wheels.Add(w);
            }

            p.m_chassis = UDPVehicleTelemetryChassis.Decode(ref data, ref dataIdx);
            p.m_drivetrain = UDPVehicleTelemetryDrivetrain.Decode(ref data, ref dataIdx);
            p.m_suspension = UDPVehicleTelemetrySuspension.Decode(ref data, ref dataIdx);
            p.m_input = UDPVehicleTelemetryInput.Decode(ref data, ref dataIdx);
            p.m_setup = UDPVehicleTelemetrySetup.Decode(ref data, ref dataIdx);
            p.m_general = UDPVehicleTelemetryGeneral.Decode(ref data, ref dataIdx);
            p.m_constant = UDPVehicleTelemetryConstant.Decode(ref data, ref dataIdx);
        }
        else
        {
            Console.WriteLine("Failed version check! Expected version: {0} - Got Version: {1}", m_expectedPacketVersion, p.m_packetVersion);
        }

        return p;
    }

    public override string ToString()
    {
        string outputString;
        outputString = string.Format("Engine\r\n\tSpeed\t\t: {0:0} rpm\r\n\tRev Ratio\t\t: {1:0.00}\r\n\tTorque\t\t: {2:0} Nm\r\n\tPower\t\t: {3:0} kW" +
            "\r\n\tOil Temp\t\t: {4:0.0} °C\r\n\tCoolant Temp\t: {5:0.0} °C\r\n\tExhaust Gas Temp\t: {6:0} °C\r\n",
            m_drivetrain.m_engineRPM, m_drivetrain.m_engineRevRatio, m_drivetrain.m_engineTorque, m_drivetrain.m_enginePower,
            m_drivetrain.m_engineOilTemperature, m_drivetrain.m_engineCoolantTemperature, m_drivetrain.m_exhaustGasTemperature);

        if (m_constant.m_isHybrid)
        {
            outputString += string.Format("MGU\r\n\tSped\t\t: {0:0} rpm\r\n\tBattery\t\t: {1:2} kWh\r\n",
            m_drivetrain.m_motorRPM, m_drivetrain.m_batteryRemaining);
        }
        outputString += string.Format("Gearbox\r\n\tInput\t\t: {0:0} rpm\r\n\tOutput\t\t: {1:0} rpm\r\n\tPower\t\t: {2:0} kW\r\n\tTorque\t\t: {3:0} Nm\r\n",
            m_drivetrain.m_gearboxInputRPM, m_drivetrain.m_gearboxOutputRPM, m_drivetrain.m_gearboxPower, m_drivetrain.m_gearboxTorque);
        outputString += string.Format("Flags\r\n\tStarter Active\t: {0:0}\r\n\tEngine Running\t: {1:0}\r\n\tEngine Fan Running : {2:0}\r\n\tRev Limiter Active\t: {3:0}" +
            "\r\n\tTCS Active\t: {4:0}\r\n\tSpeed Limiter Enabled : {5:0}\r\n\tSpeed Limiter Active\t: {6:0}\r\n\tABS Active\t: {7:0}\r\n",
            m_drivetrain.m_starterActive, m_drivetrain.m_engineRunning, m_drivetrain.m_engineFanRunning, m_drivetrain.m_revLimiterActive,
            m_drivetrain.m_tractionControlActive, m_drivetrain.m_speedLimiterEnabled, m_drivetrain.m_speedLimiterActive, m_general.m_absActive);
        outputString += string.Format("Chassis\r\n\tForward Speed\t: {0:0.0} m/s\r\n\tSide Slip\t\t: {1:0.0} °\r\n",
            m_chassis.m_forwardSpeed, m_chassis.m_sideslip * 180.0f / 3.14159f);
        outputString += string.Format("Inputs\r\n\tAccelerator\t: {0:0.00}\r\n\tBrake\t\t: {1:0.00}\r\n\tClutch\t\t: {2:0.00}\r\n\tGear\t\t: {3:0.00}\r\n",
            m_input.m_accelerator, m_input.m_brake, m_input.m_clutch, m_input.m_gear);
        outputString += "\r\nShift Points";
        for (byte g = 0; g < m_drivetrain.m_gears.Count(); ++g)
        {
            outputString += string.Format("\r\n\t{0:0} Shift Up\t\t: {1:0} rpm", g + 1, m_drivetrain.m_gears[g].m_upshiftRPM);
        }
        return outputString;
    }

    public static ushort m_expectedPacketVersion = 2;
}

