using System.Runtime.InteropServices;

namespace RaceElement.Data.Games.RaceRoom.SharedMemory;

// https://github.com/sector3studios/r3e-api


[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct RaceDuration<T>
{
    public T Race1;
    public T Race2;
    public T Race3;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Vector3<T>
{
    public T X;
    public T Y;
    public T Z;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Orientation<T>
{
    public T Pitch;
    public T Yaw;
    public T Roll;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SectorStarts<T>
{
    public T Sector1;
    public T Sector2;
    public T Sector3;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PlayerData
{
    /// <summary>
    /// Virtual physics time
    /// Unit: Ticks (1 tick = 1/400th of a second)
    /// </summary>
    public Int32 GameSimulationTicks;

    /// <summary>
    /// Virtual physics time
    /// Unit: Seconds
    /// </summary>
    public Double GameSimulationTime;

    /// <summary>
    /// Car world-space position
    /// </summary>
    public Vector3<Double> Position;

    /// <summary>
    /// Car world-space velocity
    /// Unit: Meter per second (m/s)
    /// </summary>
    public Vector3<Double> Velocity;

    /// <summary>
    /// Car local-space velocity
    /// Unit: Meter per second (m/s)
    /// </summary>
    public Vector3<Double> LocalVelocity;

    /// <summary>
    /// Car world-space acceleration
    /// Unit: Meter per second squared (m/s^2)
    /// </summary>
    public Vector3<Double> Acceleration;

    /// <summary>
    /// Car local-space acceleration
    /// Unit: Meter per second squared (m/s^2)
    /// </summary>
    public Vector3<Double> LocalAcceleration;

    /// <summary>
    /// Car body orientation
    /// Unit: Euler angles
    /// </summary>
    public Vector3<Double> Orientation;

    /// <summary>
    /// Car body rotation
    /// </summary>
    public Vector3<Double> Rotation;

    /// <summary>
    /// Car body angular acceleration (torque divided by inertia)
    /// </summary>
    public Vector3<Double> AngularAcceleration;

    /// <summary>
    /// Car world-space angular velocity
    /// Unit: Radians per second
    /// </summary>
    public Vector3<Double> AngularVelocity;

    /// <summary>
    /// Car local-space angular velocity
    /// Unit: Radians per second
    /// </summary>
    public Vector3<Double> LocalAngularVelocity;

    /// <summary>
    /// Driver g-force local to car
    /// </summary>
    public Vector3<Double> LocalGforce;

    // Total steering force coming through steering bars
    public Double SteeringForce;
    public Double SteeringForcePercentage;

    /// <summary>
    /// Current engine torque
    /// </summary>
    public Double EngineTorque;

    /// <summary>
    /// Current downforce
    /// Unit: Newtons (N)
    /// </summary>
    public Double CurrentDownforce;

    // Currently unused
    public Double Voltage;
    public Double ErsLevel;
    public Double PowerMguH;
    public Double PowerMguK;
    public Double TorqueMguK;

    // Car setup (radians, meters, meters per second)
    public TireData<Double> SuspensionDeflection;
    public TireData<Double> SuspensionVelocity;
    public TireData<Double> Camber;
    public TireData<Double> RideHeight;
    public Double FrontWingHeight;
    public Double FrontRollAngle;
    public Double RearRollAngle;
    public Double ThirdSpringSuspensionDeflectionFront;
    public Double ThirdSpringSuspensionVelocityFront;
    public Double ThirdSpringSuspensionDeflectionRear;
    public Double ThirdSpringSuspensionVelocityRear;

    /// <summary>
    /// Reserved data
    /// </summary>
    public Double Unused1;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Flags
{
    // Whether yellow flag is currently active
    // -1 = no data
    //  0 = not active
    //  1 = active
    public Int32 Yellow;

    // Whether yellow flag was caused by current slot
    // -1 = no data
    //  0 = didn't cause it
    //  1 = caused it
    public Int32 YellowCausedIt;

    // Whether overtake of car in front by current slot is allowed under yellow flag
    // -1 = no data
    //  0 = not allowed
    //  1 = allowed
    public Int32 YellowOvertake;

    // Whether you have gained positions illegaly under yellow flag to give back
    // -1 = no data
    //  0 = no positions gained
    //  n = number of positions gained
    public Int32 YellowPositionsGained;

    // Yellow flag for each sector; -1 = no data, 0 = not active, 1 = active
    public Sectors<Int32> SectorYellow;

    // Distance into track for closest yellow, -1.0 if no yellow flag exists
    // Unit: Meters (m)
    public Single ClosestYellowDistanceIntoTrack;

    // Whether blue flag is currently active
    // -1 = no data
    //  0 = not active
    //  1 = active
    public Int32 Blue;

    // Whether black flag is currently active
    // -1 = no data
    //  0 = not active
    //  1 = active
    public Int32 Black;

    // Whether green flag is currently active
    // -1 = no data
    //  0 = not active
    //  1 = active
    public Int32 Green;

    // Whether checkered flag is currently active
    // -1 = no data
    //  0 = not active
    //  1 = active
    public Int32 Checkered;

    // Whether white flag is currently active
    // -1 = no data
    //  0 = not active
    //  1 = active
    public Int32 White;

    // Whether black and white flag is currently active and reason
    // -1 = no data
    //  0 = not active
    //  1 = blue flag 1st warnings
    //  2 = blue flag 2nd warnings
    //  3 = wrong way
    //  4 = cutting track
    public Int32 BlackAndWhite;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct CarDamage
{
    // Range: 0.0 - 1.0
    // Note: -1.0 = N/A
    public Single Engine;

    // Range: 0.0 - 1.0
    // Note: -1.0 = N/A
    public Single Transmission;

    // Range: 0.0 - 1.0
    // Note: A bit arbitrary at the moment. 0.0 doesn't necessarily mean completely destroyed.
    // Note: -1.0 = N/A
    public Single Aerodynamics;

    // Range: 0.0 - 1.0
    // Note: -1.0 = N/A
    public Single Suspension;

    // Reserved data
    public Single Unused1;
    public Single Unused2;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct TireData<T>
{
    public T FrontLeft;
    public T FrontRight;
    public T RearLeft;
    public T RearRight;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PitMenuState
{
    // Pit menu preset
    public Int32 Preset;

    // Pit menu actions
    public Int32 Penalty;
    public Int32 Driverchange;
    public Int32 Fuel;
    public Int32 FrontTires;
    public Int32 RearTires;
    public Int32 FrontWing;
    public Int32 RearWing;
    public Int32 Suspension;

    // Pit menu buttons
    public Int32 ButtonTop;
    public Int32 ButtonBottom;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct CutTrackPenalties
{
    public Int32 DriveThrough;
    public Int32 StopAndGo;
    public Int32 PitStop;
    public Int32 TimeDeduction;
    public Int32 SlowDown;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DRS
{
    // If DRS is equipped and allowed
    // 0 = No, 1 = Yes, -1 = N/A
    public Int32 Equipped;
    // Got DRS activation left
    // 0 = No, 1 = Yes, -1 = N/A
    public Int32 Available;
    // Number of DRS activations left this lap
    // Note: In sessions with 'endless' amount of drs activations per lap this value starts at int32::max
    // -1 = N/A
    public Int32 NumActivationsLeft;
    // DRS engaged
    // 0 = No, 1 = Yes, -1 = N/A
    public Int32 Engaged;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PushToPass
{
    public Int32 Available;
    public Int32 Engaged;
    public Int32 AmountLeft;
    public Single EngagedTimeLeft;
    public Single WaitTimeLeft;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct TireTempInformation
{
    public TireTemperature<Single> CurrentTemp;
    public Single OptimalTemp;
    public Single ColdTemp;
    public Single HotTemp;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct BrakeTemp
{
    public Single CurrentTemp;
    public Single OptimalTemp;
    public Single ColdTemp;
    public Single HotTemp;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct TireTemperature<T>
{
    public T Left;
    public T Center;
    public T Right;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct AidSettings
{
    /// <summary>
    /// ABS; -1 = N/A, 0 = off, 1 = on, 5 = currently active
    /// </summary>
    public Int32 Abs;
    /// <summary>
    /// TC; -1 = N/A, 0 = off, 1 = on, 5 = currently active
    /// </summary>
    public Int32 Tc;
    /// <summary>
    /// ESP; -1 = N/A, 0 = off, 1 = on low, 2 = on medium, 3 = on high, 5 = currently active
    /// </summary>
    public Int32 Esp;
    /// <summary>
    /// Countersteer; -1 = N/A, 0 = off, 1 = on, 5 = currently active
    /// </summary>
    public Int32 Countersteer;
    /// <summary>
    /// Cornering; -1 = N/A, 0 = off, 1 = on, 5 = currently active
    /// </summary>
    public Int32 Cornering;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Sectors<T>
{
    public T Sector1;
    public T Sector2;
    public T Sector3;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DriverInfo
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] Name; // UTF-8
    public Int32 CarNumber;
    public Int32 ClassId;
    public Int32 ModelId;
    public Int32 TeamId;
    public Int32 LiveryId;
    public Int32 ManufacturerId;
    public Int32 UserId;
    public Int32 SlotId;
    public Int32 ClassPerformanceIndex;
    // Note: See the EngineType enum
    public Int32 EngineType;
    public Single CarWidth;
    public Single CarLength;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DriverData
{
    public DriverInfo DriverInfo;

    /// <summary>
    /// <see cref="Constants.FinishStatus"/>
    /// </summary>
    public Int32 FinishStatus;
    public Int32 Place;

    /// <summary>
    /// Based on performance index
    /// </summary>
    public Int32 PlaceClass;
    public Single LapDistance;
    public Vector3<Single> Position;
    public Int32 TrackSector;
    public Int32 CompletedLaps;
    public Int32 CurrentLapValid;
    public Single LapTimeCurrentSelf;
    public Sectors<Single> SectorTimeCurrentSelf;
    public Sectors<Single> SectorTimePreviousSelf;
    public Sectors<Single> SectorTimeBestSelf;
    public Single TimeDeltaFront;
    public Single TimeDeltaBehind;

    /// <summary>
    /// <see cref="Constants.PitStopStatus"/>
    /// </summary>
    public Int32 PitStopStatus;
    public Int32 InPitlane;

    public Int32 NumPitstops;

    public CutTrackPenalties Penalties;

    public Single CarSpeed;
    // Note: See the R3E.Constant.TireType enum
    public Int32 TireTypeFront;
    public Int32 TireTypeRear;
    // Note: See the R3E.Constant.TireSubtype enum
    public Int32 TireSubtypeFront;
    public Int32 TireSubtypeRear;

    public Single BasePenaltyWeight;
    public Single AidPenaltyWeight;

    /// <summary>
    /// -1 unavailable, 0 = not engaged, 1 = engaged
    /// </summary>
    public Int32 DrsState;
    public Int32 PtpState;

    /// <summary>
    /// -1 unavailable, DriveThrough = 0, StopAndGo = 1, Pitstop = 2, Time = 3, Slowdown = 4, Disqualify = 5,
    /// </summary>
    public Int32 PenaltyType;

    // Based on the PenaltyType you can assume the reason is:

    // DriveThroughPenaltyInvalid = 0,
    // DriveThroughPenaltyCutTrack = 1,
    // DriveThroughPenaltyPitSpeeding = 2,
    // DriveThroughPenaltyFalseStart = 3,
    // DriveThroughPenaltyIgnoredBlue = 4,
    // DriveThroughPenaltyDrivingTooSlow = 5,
    // DriveThroughPenaltyIllegallyPassedBeforeGreen = 6,
    // DriveThroughPenaltyIllegallyPassedBeforeFinish = 7,
    // DriveThroughPenaltyIllegallyPassedBeforePitEntrance = 8,
    // DriveThroughPenaltyIgnoredSlowDown = 9,
    // DriveThroughPenaltyMax = 10

    // StopAndGoPenaltyInvalid = 0,
    // StopAndGoPenaltyCutTrack1st = 1,
    // StopAndGoPenaltyCutTrackMult = 2,
    // StopAndGoPenaltyYellowFlagOvertake = 3,
    // StopAndGoPenaltyMax = 4

    // PitstopPenaltyInvalid = 0,
    // PitstopPenaltyIgnoredPitstopWindow = 1,
    // PitstopPenaltyMax = 2

    // ServableTimePenaltyInvalid = 0,
    // ServableTimePenaltyServedMandatoryPitstopLate = 1,
    // ServableTimePenaltyIgnoredMinimumPitstopDuration = 2,
    // ServableTimePenaltyMax = 3

    // SlowDownPenaltyInvalid = 0,
    // SlowDownPenaltyCutTrack1st = 1,
    // SlowDownPenaltyCutTrackMult = 2,
    // SlowDownPenaltyMax = 3

    // DisqualifyPenaltyInvalid = -1,
    // DisqualifyPenaltyFalseStart = 0,
    // DisqualifyPenaltyPitlaneSpeeding = 1,
    // DisqualifyPenaltyWrongWay = 2,
    // DisqualifyPenaltyEnteringPitsUnderRed = 3,
    // DisqualifyPenaltyExitingPitsUnderRed = 4,
    // DisqualifyPenaltyFailedDriverChange = 5,
    // DisqualifyPenaltyThreeDriveThroughsInLap = 6,
    // DisqualifyPenaltyLappedFieldMultipleTimes = 7,
    // DisqualifyPenaltyIgnoredDriveThroughPenalty = 8,
    // DisqualifyPenaltyIgnoredStopAndGoPenalty = 9,
    // DisqualifyPenaltyIgnoredPitStopPenalty = 10,
    // DisqualifyPenaltyIgnoredTimePenalty = 11,
    // DisqualifyPenaltyExcessiveCutting = 12,
    // DisqualifyPenaltyIgnoredBlueFlag = 13,
    // DisqualifyPenaltyMax = 14
    public Int32 PenaltyReason;

    /// <summary>
    /// -1 unavailable, 0 = ignition off, 1 = ignition on but not running, 2 = ignition on and running
    /// </summary>
    public Int32 EngineState;

    /// <summary>
    /// Car body orientation
    /// Unit: Euler angles
    /// </summary>
    public Vector3<Single> Orientation;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Shared
{
    //////////////////////////////////////////////////////////////////////////
    // Version
    //////////////////////////////////////////////////////////////////////////
    public Int32 VersionMajor;
    public Int32 VersionMinor;
    public Int32 AllDriversOffset; // Offset to NumCars variable
    public Int32 DriverDataSize; // Size of DriverData

    //////////////////////////////////////////////////////////////////////////
    // Game State
    //////////////////////////////////////////////////////////////////////////

    public Int32 GamePaused;
    public Int32 GameInMenus;
    public Int32 GameInReplay;
    public Int32 GameUsingVr;

    public Int32 GameUnused1;

    //////////////////////////////////////////////////////////////////////////
    // High Detail
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// High precision data for player's vehicle only
    /// </summary>
    public PlayerData Player;

    //////////////////////////////////////////////////////////////////////////
    // Event And Session
    //////////////////////////////////////////////////////////////////////////

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] TrackName; // UTF-8
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] LayoutName; // UTF-8

    public Int32 TrackId;
    public Int32 LayoutId;

    // Layout length in meters
    public Single LayoutLength;
    public SectorStarts<Single> SectorStartFactors;

    // Race session durations
    // Note: Index 0-2 = race 1-3
    // Note: Value -1 = N/A
    // Note: If both laps and minutes are more than 0, race session starts with minutes then adds laps
    public RaceDuration<Int32> RaceSessionLaps;
    public RaceDuration<Int32> RaceSessionMinutes;

    // The current race event index, for championships with multiple events
    // Note: 0-indexed, -1 = N/A
    public Int32 EventIndex;

    // Which session the player is in (practice, qualifying, race, etc.)
    // Note: See the R3E.Constant.Session enum
    public Int32 SessionType;

    // The current iteration of the current type of session (second qualifying session, etc.)
    // Note: 1 = first, 2 = second etc, -1 = N/A
    public Int32 SessionIteration;

    // If the session is time based, lap based or time based with an extra lap at the end
    public Int32 SessionLengthFormat;

    // Unit: Meter per second (m/s)
    public Single SessionPitSpeedLimit;

    // Which phase the current session is in (gridwalk, countdown, green flag, etc.)
    // Note: See the R3E.Constant.SessionPhase enum
    public Int32 SessionPhase;

    // Which phase start lights are in; -1 = unavailable, 0 = off, 1-5 = redlight on and counting down, 6 = greenlight on
    // Note: See the r3e_session_phase enum
    public Int32 StartLights;

    // -1 = no data available
    //  0 = not active
    //  1 = active
    //  2 = 2x
    //  3 = 3x
    //  4 = 4x
    public Int32 TireWearActive;

    // -1 = no data
    //  0 = not active
    //  1 = active
    //  2 = 2x
    //  3 = 3x
    //  4 = 4x
    public Int32 FuelUseActive;

    /// <summary>
    /// Total number of laps in the race, or -1 if player is not in race mode (practice, test mode, etc.)
    /// </summary>
    public Int32 NumberOfLaps;

    // Amount of time and time remaining for the current session
    // Note: Only available in time-based sessions, -1.0 = N/A
    // Units: Seconds
    public Single SessionTimeDuration;
    public Single SessionTimeRemaining;

    /// <summary>
    /// Server max incident points, -1 = N/A
    /// </summary>
    public Int32 MaxIncidentPoints;

    /// <summary>
    /// Reserved data
    /// </summary>
    public Single EventUnused2;

    //////////////////////////////////////////////////////////////////////////
    // Pit
    //////////////////////////////////////////////////////////////////////////

    // Current status of the pit stop
    // Note: See the R3E.Constant.PitWindow enum
    public Int32 PitWindowStatus;

    // The minute/lap from which you're obligated to pit (-1 = N/A)
    // Unit: Minutes in time-based sessions, otherwise lap
    public Int32 PitWindowStart;

    // The minute/lap into which you need to have pitted (-1 = N/A)
    // Unit: Minutes in time-based sessions, otherwise lap
    public Int32 PitWindowEnd;

    /// <summary>
    /// If current vehicle is in pitline (-1 = N/A)
    /// </summary>
    public Int32 InPitlane;

    // What is currently selected in pit menu, and array of states (preset/buttons: -1 = not selectable, 1 = selectable) (actions: -1 = N/A, 0 = unmarked for fix, 1 = marked for fix)
    public Int32 PitMenuSelection;
    public PitMenuState PitMenuState;

    /// <summary>
    /// Current vehicle pit state (-1 = N/A, 0 = None, 1 = Requested stop, 2 = Entered pitlane heading for pitspot, 3 = Stopped at pitspot, 4 = Exiting pitspot heading for pit exit)
    /// </summary>
    public Int32 PitState;

    // Current vehicle pitstop actions duration
    public Single PitTotalDuration;
    public Single PitElapsedTime;

    /// <summary>
    /// Current vehicle pit action (-1 = N/A, 0 = None, 1 = Preparing, (combination of 2 = Penalty serve, 4 = Driver change, 8 = Refueling, 16 = Front tires, 32 = Rear tires, 64 = Body, 128 = Front wing, 256 = Rear wing, 512 = Suspension))
    /// </summary>
    public Int32 PitAction;

    /// <summary>
    /// Number of pitstops the current vehicle has performed (-1 = N/A)
    /// </summary>
    public Int32 NumPitstopsPerformed;

    public Single PitMinDurationTotal;
    public Single PitMinDurationLeft;

    //////////////////////////////////////////////////////////////////////////
    // Scoring & Timings
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// The current state of each type of flag
    /// </summary>
    public Flags Flags;

    /// <summary>
    /// Current position (1 = first place)
    /// </summary>
    public Int32 Position;
    /// <summary>
    /// Based on performance index
    /// </summary>
    public Int32 PositionClass;

    /// <summary>
    /// <see cref="Constants.FinishStatus"/>
    /// </summary>
    public Int32 FinishStatus;

    /// <summary>
    /// Total number of cut track warnings (-1 = N/A)
    /// </summary>
    public Int32 CutTrackWarnings;

    /// <summary>
    /// The number of penalties the car currently has pending of each type (-1 = N/A)
    /// </summary>
    public CutTrackPenalties Penalties;
    // Total number of penalties pending for the car
    // Note: See the 'penalties' field
    public Int32 NumPenalties;

    /// <summary>
    /// How many laps the player has completed. If this value is 6, the player is on his 7th lap. -1 = n/a
    /// </summary>
    public Int32 CompletedLaps;
    public Int32 CurrentLapValid;
    public Int32 TrackSector;
    public Single LapDistance;
    /// <summary>
    /// fraction of lap completed, 0.0-1.0, -1.0 = N/A
    /// </summary>
    public Single LapDistanceFraction;

    /// <summary>
    /// The current best lap time for the leader of the session (-1.0 = N/A)
    /// </summary>
    public Single LapTimeBestLeader;
    /// <summary>
    /// The current best lap time for the leader of the player's class in the current session (-1.0 = N/A)
    /// </summary>
    public Single LapTimeBestLeaderClass;
    // Sector times of fastest lap by anyone in session
    // Unit: Seconds (-1.0 = N/A)
    public Sectors<Single> SectorTimesSessionBestLap;
    // Unit: Seconds (-1.0 = none)
    public Single LapTimeBestSelf;
    public Sectors<Single> SectorTimesBestSelf;
    // Unit: Seconds (-1.0 = none)
    public Single LapTimePreviousSelf;
    public Sectors<Single> SectorTimesPreviousSelf;
    // Unit: Seconds (-1.0 = none)
    public Single LapTimeCurrentSelf;
    public Sectors<Single> SectorTimesCurrentSelf;
    // The time delta between the player's time and the leader of the current session (-1.0 = N/A)
    public Single LapTimeDeltaLeader;
    // The time delta between the player's time and the leader of the player's class in the current session (-1.0 = N/A)
    public Single LapTimeDeltaLeaderClass;
    // Time delta between the player and the car placed in front (-1.0 = N/A)
    // Units: Seconds
    public Single TimeDeltaFront;
    // Time delta between the player and the car placed behind (-1.0 = N/A)
    // Units: Seconds
    public Single TimeDeltaBehind;
    // Time delta between this car's current laptime and this car's best laptime
    // Unit: Seconds (-1000.0 = N/A)
    public Single TimeDeltaBestSelf;
    // Best time for each individual sector no matter lap
    // Unit: Seconds (-1.0 = N/A)
    public Sectors<Single> BestIndividualSectorTimeSelf;
    public Sectors<Single> BestIndividualSectorTimeLeader;
    public Sectors<Single> BestIndividualSectorTimeLeaderClass;
    public Int32 IncidentPoints;

    // -1 = N/A, 0 = this and next lap valid, 1 = this lap invalid, 2 = this and next lap invalid
    public Int32 LapValidState;

    /// <summary>
    /// Reserved data
    /// </summary>
    public Single ScoreUnused1;
    /// <summary>
    /// Reserved data
    /// </summary>
    public Single ScoreUnused2;

    //////////////////////////////////////////////////////////////////////////
    // Vehicle information
    //////////////////////////////////////////////////////////////////////////

    public DriverInfo VehicleInfo;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] PlayerName; // UTF-8

    //////////////////////////////////////////////////////////////////////////
    // Vehicle State
    //////////////////////////////////////////////////////////////////////////

    // Which controller is currently controlling the player's car (AI, player, remote, etc.)
    // Note: See the R3E.Constant.Control enum
    public Int32 ControlType;

    /// <summary>
    /// Unit: Meter per second (m/s)
    /// </summary>
    public Single CarSpeed;

    /// <summary>
    /// Unit: Radians per second (rad/s)
    /// </summary>
    public Single EngineRps;
    /// <summary>
    /// Unit: Radians per second (rad/s)
    /// </summary>
    public Single MaxEngineRps;
    /// <summary>
    /// Unit: Radians per second (rad/s)
    /// </summary>
    public Single UpshiftRps;

    /// <summary>
    /// -2 = N/A, -1 = reverse, 0 = neutral, 1 = first gear, ...
    /// </summary>
    public Int32 Gear;
    /// <summary>
    /// -1 = N/A
    /// </summary>
    public Int32 NumGears;

    /// <summary>
    /// Physical location of car's center of gravity in world space (X, Y, Z) (Y = up)
    /// </summary>
    public Vector3<Single> CarCgLocation;
    /// <summary>
    /// Pitch, yaw, roll
    /// Unit: Radians (rad)
    /// </summary>
    public Orientation<Single> CarOrientation;
    /// <summary>
    /// Acceleration in three axes (X, Y, Z) of car body in local-space.
    /// From car center, +X=left, +Y=up, +Z=back.
    /// Unit: Meter per second squared (m/s^2)
    /// </summary>
    public Vector3<Single> LocalAcceleration;

    // Unit: Kilograms (kg)
    // Note: Car + penalty weight + fuel
    public Single TotalMass;
    // Unit: Liters (l)
    // Note: Fuel per lap show estimation when not enough data, then max recorded fuel per lap
    // Note: Not valid for remote players
    public Single FuelLeft;
    public Single FuelCapacity;
    public Single FuelPerLap;
    // Unit: Celsius (C)
    // Note: Not valid for AI or remote players
    public Single EngineWaterTemp;
    public Single EngineOilTemp;
    // Unit: Kilopascals (KPa)
    // Note: Not valid for AI or remote players
    public Single FuelPressure;
    // Unit: Kilopascals (KPa)
    // Note: Not valid for AI or remote players
    public Single EngineOilPressure;

    // Unit: (Bar)
    // Note: Not valid for AI or remote players (-1.0 = N/A)
    public Single TurboPressure;

    // How pressed the throttle pedal is
    // Range: 0.0 - 1.0 (-1.0 = N/A)
    // Note: Not valid for AI or remote players
    public Single Throttle;
    public Single ThrottleRaw;
    // How pressed the brake pedal is
    // Range: 0.0 - 1.0 (-1.0 = N/A)
    // Note: Not valid for AI or remote players
    public Single Brake;
    public Single BrakeRaw;
    // How pressed the clutch pedal is
    // Range: 0.0 - 1.0 (-1.0 = N/A)
    // Note: Not valid for AI or remote players
    public Single Clutch;
    public Single ClutchRaw;
    // How much the steering wheel is turned
    // Range: -1.0 - 1.0
    // Note: Not valid for AI or remote players
    public Single SteerInputRaw;
    // How many degrees in steer lock (center to full lock)
    // Note: Not valid for AI or remote players
    public Int32 SteerLockDegrees;
    // How many degrees in wheel range (degrees full left to rull right)
    // Note: Not valid for AI or remote players
    public Int32 SteerWheelRangeDegrees;

    /// <summary>
    /// Aid settings
    /// </summary>
    public AidSettings AidSettings;

    /// <summary>
    /// DRS data
    /// </summary>
    public DRS Drs;

    /// <summary>
    /// Pit limiter (-1 = N/A, 0 = inactive, 1 = active)
    /// </summary>
    public Int32 PitLimiter;

    /// <summary>
    /// Push to pass data
    /// </summary>
    public PushToPass PushToPass;

    // How much the vehicle's brakes are biased towards the back wheels (0.3 = 30%, etc.) (-1.0 = N/A)
    // Note: Not valid for AI or remote players
    public Single BrakeBias;

    // DRS activations available in total (-1 = N/A or endless)
    public Int32 DrsNumActivationsTotal;
    // PTP activations available in total (-1 = N/A, or there's no restriction per lap, or endless)
    public Int32 PtpNumActivationsTotal;

    // Battery state of charge
    // Range: 0.0 - 100.0 (-1.0 = N/A)
    public Single BatterySoC;

    // Brake water tank (-1.0 = N/A)
    // Unit: Liters (l)
    public Single WaterLeft;

    /// <summary>
    /// -1.0 = N/A
    /// </summary>
    public Int32 AbsSetting;

    /// <summary>
    /// -1 = N/A, 0 = off, 1 = on, 2 = strobing
    /// </summary>
    public Int32 HeadLights;

    /// <summary>
    /// Reserved data
    /// </summary>
    public Single VehicleUnused1;

    //////////////////////////////////////////////////////////////////////////
    // Tires
    //////////////////////////////////////////////////////////////////////////

    // Which type of tires the player's car has (option, prime, etc.)
    // Note: See the R3E.Constant.TireType enum, deprecated - use the values further down instead
    public Int32 TireType;

    // Rotation speed
    // Uint: Radians per second
    public TireData<Single> TireRps;
    // Wheel speed
    // Uint: Meters per second
    public TireData<Single> TireSpeed;
    // Range: 0.0 - 1.0 (-1.0 = N/A)
    public TireData<Single> TireGrip;
    // Range: 0.0 - 1.0 (-1.0 = N/A)
    public TireData<Single> TireWear;
    // (-1 = N/A, 0 = false, 1 = true)
    public TireData<Int32> TireFlatspot;
    // Unit: Kilopascals (KPa) (-1.0 = N/A)
    // Note: Not valid for AI or remote players
    public TireData<Single> TirePressure;
    // Percentage of dirt on tire (-1.0 = N/A)
    // Range: 0.0 - 1.0
    public TireData<Single> TireDirt;

    // Current temperature of three points across the tread of the tire (-1.0 = N/A)
    // Optimum temperature
    // Cold temperature
    // Hot temperature
    // Unit: Celsius (C)
    // Note: Not valid for AI or remote players
    public TireData<TireTempInformation> TireTemp;

    // Which type of tires the car has (option, prime, etc.)
    // Note: See the R3E.Constant.TireType enum
    public Int32 TireTypeFront;
    public Int32 TireTypeRear;
    // Which subtype of tires the car has
    // Note: See the R3E.Constant.TireSubtype enum
    public Int32 TireSubtypeFront;
    public Int32 TireSubtypeRear;
    /// <summary>
    /// Current brake temperature (-1.0 = N/A)
    /// Optimum temperature
    /// Cold temperature
    /// Hot temperature
    /// Unit: Celsius (C)
    /// Note: Not valid for AI or remote players
    /// </summary>
    public TireData<BrakeTemp> BrakeTemp;

    /// <summary>
    /// Brake pressure (-1.0 = N/A)
    /// Unit: Kilo Newtons (kN) /// Note: Not valid for AI or remote players
    /// </summary>
    public TireData<Single> BrakePressure;

    /// <summary>
    /// -1.0 = N/A
    /// </summary>
    public Int32 TractionControlSetting;
    /// <summary>
    /// -1.0 = N/A
    /// </summary>
    public Int32 EngineMapSetting;
    /// <summary>
    /// -1.0 = N/A
    /// </summary>
    public Int32 EngineBrakeSetting;

    /// <summary>
    /// -1.0 = N/A, 0.0 -> 100.0 percent
    /// </summary>
    public Single TractionControlPercent;

    /// <summary>
    /// Which type of material under player car tires (tarmac, gravel, etc.)
    /// <see cref="Constants.MtrlType"/>
    /// </summary>
    public TireData<Int32> TireOnMtrl;

    /// <summary>
    /// Tire load (N)
    /// -1.0 = N/A
    /// </summary>
    public TireData<Single> TireLoad;

    //////////////////////////////////////////////////////////////////////////
    // Damage
    //////////////////////////////////////////////////////////////////////////

    // The current state of various parts of the car
    // Note: Not valid for AI or remote players
    public CarDamage CarDamage;

    //////////////////////////////////////////////////////////////////////////
    // Driver Info
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Number of cars (including the player) in the race
    /// </summary>
    public Int32 NumCars;

    /// <summary>
    /// Contains name and basic vehicle info for all drivers in place order
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
    public DriverData[] DriverData;

    public static readonly int Size = Marshal.SizeOf(typeof(Shared));
    public static readonly byte[] Buffer = new byte[Size];
}
