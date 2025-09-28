using System.Runtime.InteropServices;

namespace RaceElement.Data.Games.WRC_Generations.UDP;

/// <summary>
/// Contains the memory structures for WRC Generations UDP telemetry packets, based on the Codemasters/Dirt Rally 2.0 format.
/// All structures are packed sequentially with no alignment padding for direct binary parsing.
/// </summary>
sealed class WrcGenerationsMemoryStructs
{
    /// <summary>
    /// The common header for all UDP telemetry packets.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct PacketHeader
    {
        /// <summary>
        /// The packet format version (always 2018 for this format).
        /// </summary>
        public ushort PacketFormat;

        /// <summary>
        /// The game version, calculated as major version * 1000 + minor version.
        /// </summary>
        public ushort GameVersion;

        /// <summary>
        /// The version of this specific packet structure (starts from 1).
        /// </summary>
        public byte PacketVersion;

        /// <summary>
        /// The ID of the packet type: 0=Motion, 1=Session, 2=Lap, 3=Event, 4=Participants, 5=CarStatus, 6=CarDamage, 7=TyreSets.
        /// </summary>
        public byte PacketId;

        /// <summary>
        /// A unique identifier for the session.
        /// </summary>
        public uint SessionUID;

        /// <summary>
        /// The current session time in seconds.
        /// </summary>
        public float SessionTime;

        /// <summary>
        /// The frame identifier for this packet.
        /// </summary>
        public uint FrameIdentifier;

        /// <summary>
        /// The index of the player's car in the packet arrays.
        /// </summary>
        public byte PlayerCarIndex;

        /// <summary>
        /// The index of the secondary player car (for splitscreen mode).
        /// </summary>
        public byte SecondaryPlayerCarIndex;

        /// <summary>
        /// Padding to ensure the header is exactly 24 bytes.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private byte[] Padding;
    }

    /// <summary>
    /// Represents motion data for a single car, including position, velocity, acceleration, and orientation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct CarMotionData
    {
        /// <summary>
        /// The world position of the car in meters [X, Y, Z].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] WorldPosition;

        /// <summary>
        /// The world velocity of the car in m/s [X, Y, Z].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] WorldVelocity;

        /// <summary>
        /// The local velocity of the car in m/s [right, forward, down].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] LocalVelocity;

        /// <summary>
        /// The angular velocity of the car in rad/s [X, Y, Z].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] AngularVelocity;

        /// <summary>
        /// The local acceleration of the car in m/s² [right, forward, down].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] LocalAcceleration;

        /// <summary>
        /// The world acceleration of the car in m/s² [X, Y, Z].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] WorldAcceleration;

        /// <summary>
        /// The angular acceleration of the car in rad/s² [X, Y, Z].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] AngularAcceleration;

        /// <summary>
        /// The yaw orientation of the car in radians.
        /// </summary>
        public float Yaw;

        /// <summary>
        /// The pitch orientation of the car in radians.
        /// </summary>
        public float Pitch;

        /// <summary>
        /// The roll orientation of the car in radians.
        /// </summary>
        public float Roll;
    }

    /// <summary>
    /// The Motion packet containing motion data for up to 20 cars, plus additional player-specific wheel and tyre data.
    /// Total size: Header (24 bytes) + 20 cars * 33 floats (1320 bytes) + player extras (96 bytes) ≈ 1440 bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct MotionPacket
    {
        /// <summary>
        /// The packet header.
        /// </summary>
        public PacketHeader Header;

        /// <summary>
        /// Motion data for up to 20 cars (20 * 33 floats = 1320 bytes).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public CarMotionData[] CarMotionData;

        // Player car extra data (additional 24 floats = 96 bytes)

        /// <summary>
        /// Suspension position for the 4 wheels in meters [front-left, rear-left, rear-right, front-right].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] SuspensionPosition;

        /// <summary>
        /// Suspension velocity for the 4 wheels in m/s.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] SuspensionVelocity;

        /// <summary>
        /// Suspension ride height for the 4 wheels in meters.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] SuspensionRideHeight;

        /// <summary>
        /// Wheel speed for the 4 wheels (tangential speed in m/s).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] WheelSpeed;

        /// <summary>
        /// Longitudinal wheel slip ratio for the 4 wheels.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] WheelSlip;

        /// <summary>
        /// Lateral wheel slip angle for the 4 wheels in radians.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] WheelSlipAngle;

        /// <summary>
        /// Surface type index for the 4 wheels.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public short[] WheelSurface;

        /// <summary>
        /// Wheel camber angle for the 4 wheels in degrees.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public short[] WheelCamber;

        /// <summary>
        /// Wheel toe angle for the 4 wheels in degrees.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] WheelToe;

        /// <summary>
        /// Local velocity for the player car in m/s [right, forward, down].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] LocalVelocityPlayer;

        /// <summary>
        /// Angular velocity for the player car in rad/s [X, Y, Z].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] AngularVelocityPlayer;

        /// <summary>
        /// Local acceleration for the player car in m/s² [right, forward, down].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] LocalAccelerationPlayer;

        /// <summary>
        /// World acceleration for the player car in m/s² [X, Y, Z].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] WorldAccelerationPlayer;

        /// <summary>
        /// Angular acceleration for the player car in rad/s² [X, Y, Z].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] AngularAccelerationPlayer;

        /// <summary>
        /// Yaw, pitch, roll for the player car in radians.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] YawPitchRollPlayer;

        /// <summary>
        /// Tyre slip ratio for the 4 wheels.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] TyreSlipRatio;

        /// <summary>
        /// Tyre slip angle for the player car in radians [4 wheels].
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] TyreSlipAnglePlayer;

        /// <summary>
        /// Tyre surface temperature for the 4 wheels in Celsius.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] TyreSurfaceTemp;

        /// <summary>
        /// Inner tyre temperature for the 4 wheels in Celsius.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] TyreInnerTemp;

        /// <summary>
        /// Middle tyre temperature for the 4 wheels in Celsius.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] TyreMiddleTemp;

        /// <summary>
        /// Outer tyre temperature for the 4 wheels in Celsius.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] TyreOuterTemp;

        /// <summary>
        /// Tyre pressure for the 4 wheels in kPa.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] TyrePressure;
    }

    /// <summary>
    /// The Session packet containing session state, weather, and track information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SessionPacket
    {
        /// <summary>
        /// The packet header.
        /// </summary>
        public PacketHeader Header;

        /// <summary>
        /// The current weather condition (0=clear, 1=light cloud, etc.).
        /// </summary>
        public uint Weather;

        /// <summary>
        /// The track temperature in Celsius.
        /// </summary>
        public short TrackTemperature;

        /// <summary>
        /// The air temperature in Celsius.
        /// </summary>
        public short AirTemperature;

        /// <summary>
        /// The total number of laps in the session.
        /// </summary>
        public byte TotalLaps;

        /// <summary>
        /// The track ID (255=unknown).
        /// </summary>
        public byte TrackId;

        /// <summary>
        /// The formula/car class (0=F1 modern, etc.).
        /// </summary>
        public byte Formula;

        /// <summary>
        /// A link identifier for the session.
        /// </summary>
        public ushort SessionLinkIdentifier;

        /// <summary>
        /// The session type (0=unknown, 1=practice, 2=qualifying, 3=race).
        /// </summary>
        public uint SessionType;

        /// <summary>
        /// The time remaining in the session in seconds.
        /// </summary>
        public float TimeLeftInSession;

        /// <summary>
        /// A link identifier for the season.
        /// </summary>
        public ushort SeasonLinkIdentifier;

        /// <summary>
        /// A link identifier for the weekend.
        /// </summary>
        public byte WeekendLinkIdentifier;

        /// <summary>
        /// Flag indicating if this is the first lap (0/1).
        /// </summary>
        public byte FirstLap;

        /// <summary>
        /// The number of laps in this session.
        /// </summary>
        public byte NumLapsInSession;

        /// <summary>
        /// The current lap number.
        /// </summary>
        public byte CurrentLap;

        /// <summary>
        /// The number of completed laps.
        /// </summary>
        public byte NumCompletedLaps;

        /// <summary>
        /// Flag indicating if spectating (0/1).
        /// </summary>
        public byte IsSpectating;

        /// <summary>
        /// Flag indicating if the player is a participant (0/1).
        /// </summary>
        public byte IsParticipant;

        /// <summary>
        /// The index of the current actor.
        /// </summary>
        public byte CurrentActorIdx;

        /// <summary>
        /// Flag indicating if this is the first session (0/1).
        /// </summary>
        public byte IsFirstSession;
    }

    /// <summary>
    /// Represents lap data for a single car, including times and status.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct LapData
    {
        /// <summary>
        /// The current lap time in seconds.
        /// </summary>
        public float CurrentLapTime;

        /// <summary>
        /// The last lap time in seconds.
        /// </summary>
        public float LastLapTime;

        /// <summary>
        /// The best lap time in seconds.
        /// </summary>
        public float BestLapTime;

        /// <summary>
        /// The current sector time in seconds.
        /// </summary>
        public float CurrentSectorTime;

        /// <summary>
        /// The last sector time in seconds.
        /// </summary>
        public float LastSectorTime;

        /// <summary>
        /// The best sector time in seconds.
        /// </summary>
        public float BestSectorTime;

        /// <summary>
        /// The time delta to the best lap in seconds.
        /// </summary>
        public float TimeDelta;

        /// <summary>
        /// The total number of laps (purpose unclear in this context).
        /// </summary>
        public ushort TotalLaps;

        /// <summary>
        /// The current lap number.
        /// </summary>
        public byte CurrentLapNum;

        /// <summary>
        /// The pit status (0=pitting, 1=in pit, 2=out of pit).
        /// </summary>
        public byte PitStatus;

        /// <summary>
        /// The current sector (1-3).
        /// </summary>
        public byte Sector;

        /// <summary>
        /// Flag indicating if the current lap is valid (0/1).
        /// </summary>
        public byte CurrentLapValid;
    }

    /// <summary>
    /// The Lap packet containing lap data for up to 20 cars.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct LapPacket
    {
        /// <summary>
        /// The packet header.
        /// </summary>
        public PacketHeader Header;

        /// <summary>
        /// Lap data for up to 20 cars.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public LapData[] LapData;
    }

    /// <summary>
    /// The Event packet containing information about a session event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct EventPacket
    {
        /// <summary>
        /// The packet header.
        /// </summary>
        public PacketHeader Header;

        /// <summary>
        /// The event type code (e.g., 0=session started).
        /// </summary>
        public uint EventTypeCode;

        /// <summary>
        /// The time of the event in seconds.
        /// </summary>
        public float EventTime;

        /// <summary>
        /// A string describing the event (e.g., "PIT_STOP").
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)]
        public string EventString;
    }

    /// <summary>
    /// Represents data for a single participant/car in the session.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ParticipantData
    {
        /// <summary>
        /// The index of the participant (0=player).
        /// </summary>
        public uint Index;

        /// <summary>
        /// The name of the participant.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)]
        public string Name;

        /// <summary>
        /// The nationality of the participant.
        /// </summary>
        public uint Nationality;

        /// <summary>
        /// The team ID of the participant.
        /// </summary>
        public uint TeamId;

        /// <summary>
        /// The car class of the participant.
        /// </summary>
        public uint CarClass;

        /// <summary>
        /// Flag indicating if this is the player's team (0/1).
        /// </summary>
        public uint MyTeam;

        /// <summary>
        /// Flag indicating if this is the player (0/1).
        /// </summary>
        public byte IsPlayer;
    }

    /// <summary>
    /// The Participants packet containing data for all participants in the session.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ParticipantsPacket
    {
        /// <summary>
        /// The packet header.
        /// </summary>
        public PacketHeader Header;

        /// <summary>
        /// The number of participants.
        /// </summary>
        public uint NumParticipants;

        /// <summary>
        /// Data for up to 20 participants.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public ParticipantData[] Participants;
    }

    /// <summary>
    /// Represents the status data for a single car, including assists, fuel, and tyres.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct CarStatusData
    {
        /// <summary>
        /// Traction control setting (0=off, 1=medium, 2=high).
        /// </summary>
        public byte TractionControl;

        /// <summary>
        /// Anti-lock brakes setting (0=off, 1=on).
        /// </summary>
        public byte AntiLockBrakes;

        /// <summary>
        /// Fuel mix setting (0=lean, 1=standard).
        /// </summary>
        public byte FuelMix;

        /// <summary>
        /// ERS mode (0=off, 1=medium, 2=hotlap, 3=attack).
        /// </summary>
        public byte ErsMode;

        /// <summary>
        /// Stored ERS energy in Joules.
        /// </summary>
        public float ErsStoreEnergy;

        /// <summary>
        /// ERS deployment mode (0=none, etc.).
        /// </summary>
        public uint ErsDeployMode;

        /// <summary>
        /// ERS energy deployed this lap in MegaJoules.
        /// </summary>
        public float ErsDeployedThisLap;

        /// <summary>
        /// Current fuel level as percentage.
        /// </summary>
        public byte FuelLevel;

        /// <summary>
        /// Fuel capacity as percentage.
        /// </summary>
        public byte FuelCapacity;

        /// <summary>
        /// Estimated fuel remaining in laps.
        /// </summary>
        public byte FuelRemainingLaps;

        /// <summary>
        /// RSB allowed (0=off, 1=on; rear suspension bias).
        /// </summary>
        public uint RsbAllowed;

        /// <summary>
        /// RSB brake bias as percentage.
        /// </summary>
        public byte RsbBrakeBias;

        /// <summary>
        /// Pit limiter status (0=off, 1=on).
        /// </summary>
        public byte PitLimiterStatus;

        /// <summary>
        /// Fuel to add in pit in liters.
        /// </summary>
        public float FuelInPit;

        /// <summary>
        /// ERS to add in pit as percentage.
        /// </summary>
        public byte ErsInPit;

        /// <summary>
        /// Current tyre compound index.
        /// </summary>
        public byte TyreCompound;

        /// <summary>
        /// Tyre life left as percentage.
        /// </summary>
        public byte TyreLifeLeft;

        /// <summary>
        /// Tyre age in laps.
        /// </summary>
        public byte TyreAgeLaps;

        /// <summary>
        /// RSB in pit (0=off, 1=on).
        /// </summary>
        public uint RsbInPit;

        /// <summary>
        /// Fuel mix in pit (0=lean, 1=standard).
        /// </summary>
        public uint FuelMixInPit;

        /// <summary>
        /// ERS mode in pit (0=off, etc.).
        /// </summary>
        public byte ErsInPitMode;

        /// <summary>
        /// Tyre compound in pit index.
        /// </summary>
        public byte TyreCompoundInPit;
    }

    /// <summary>
    /// The CarStatus packet containing status data for up to 20 cars.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct CarStatusPacket
    {
        /// <summary>
        /// The packet header.
        /// </summary>
        public PacketHeader Header;

        /// <summary>
        /// Status data for up to 20 cars.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public CarStatusData[] CarStatusData;
    }

    /// <summary>
    /// Represents damage data for a single car.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct CarDamageData
    {
        /// <summary>
        /// Suspension damage percentage for the 4 wheels.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] SuspensionDamage;

        /// <summary>
        /// Front left wing damage percentage.
        /// </summary>
        public float FrontLeftWingDamage;

        /// <summary>
        /// Front right wing damage percentage.
        /// </summary>
        public float FrontRightWingDamage;

        /// <summary>
        /// Rear wing damage percentage.
        /// </summary>
        public float RearWingDamage;

        /// <summary>
        /// Floor damage percentage.
        /// </summary>
        public float FloorDamage;

        /// <summary>
        /// Engine damage percentage.
        /// </summary>
        public float EngineDamage;

        /// <summary>
        /// Gearbox damage percentage.
        /// </summary>
        public float GearBoxDamage;

        /// <summary>
        /// Exhaust damage percentage.
        /// </summary>
        public float ExhaustDamage;

        /// <summary>
        /// Vehicle FIA flags (-1=invalid, 0=none, etc.).
        /// </summary>
        public byte VehicleFiaFlags;
    }

    /// <summary>
    /// The CarDamage packet containing damage data for up to 20 cars.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct CarDamagePacket
    {
        /// <summary>
        /// The packet header.
        /// </summary>
        public PacketHeader Header;

        /// <summary>
        /// Damage data for up to 20 cars.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public CarDamageData[] CarDamageData;
    }

    /// <summary>
    /// Represents data for a single tyre set.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TyreSetData
    {
        /// <summary>
        /// The index of the tyre set (0-23).
        /// </summary>
        public byte Index;

        /// <summary>
        /// The tyre compound index.
        /// </summary>
        public byte Compound;

        /// <summary>
        /// The age of the tyre set in laps.
        /// </summary>
        public byte Age;

        /// <summary>
        /// Wear percentage for the 4 tyres.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] Wear;

        /// <summary>
        /// Flag indicating if the tyre set is available (0=used, 1=available).
        /// </summary>
        public byte Available;

        /// <summary>
        /// Flag indicating if the tyre set is in pit (0/1).
        /// </summary>
        public byte InPit;
    }

    /// <summary>
    /// The TyreSet packet containing data for up to 20 tyre sets.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TyreSetPacket
    {
        /// <summary>
        /// The packet header.
        /// </summary>
        public PacketHeader Header;

        /// <summary>
        /// Data for up to 20 tyre sets.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public TyreSetData[] TyreSets;
    }
}