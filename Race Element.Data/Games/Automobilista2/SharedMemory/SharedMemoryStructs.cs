using System.Runtime.InteropServices;
namespace RaceElement.Data.Games.Automobilista2.SharedMemory;

[StructLayout(LayoutKind.Sequential)]
internal struct StringData
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)Constants.Restrictions.STRING_LENGTH_MAX)]
    public string Data;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CompoundName
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)Constants.Restrictions.TYRE_COMPOUND_NAME_LENGTH_MAX)]
    public string Data;
}

[StructLayout(LayoutKind.Sequential)]
internal struct Vector3
{
    public float X;
    public float Y;
    public float Z;

    public System.Numerics.Vector3 ToVector3()
    {
        return new System.Numerics.Vector3(X, Y, Z);
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct Tyres<T>
{
    public T FL;
    public T FR;
    public T RL;
    public T RR;
}

[StructLayout(LayoutKind.Sequential)]
internal struct ParticipantInfo // (Type#13) ParticipantInfo struct  (to be used with 'mParticipantInfo')
{
    [MarshalAs(UnmanagedType.I1)] public bool mIsActive;
    public StringData mName;                // [ string ]
    public Vector3 mWorldPosition;          // [ UNITS = World Space  X  Y  Z ]
    public Single mCurrentLapDistance;      // [ UNITS = Metres ]   [ RANGE = 0.0f->... ]    [ UNSET = 0.0f ]
    public UInt32 mRacePosition;            // [ RANGE = 1->... ]   [ UNSET = 0 ]
    public UInt32 mLapsCompleted;           // [ RANGE = 0->... ]   [ UNSET = 0 ]
    public UInt32 mCurrentLap;              // [ RANGE = 0->... ]   [ UNSET = 0 ]
    public Int32 mCurrentSector;            // [ RANGE = 0->... ]   [ UNSET = -1 ]
}

[StructLayout(LayoutKind.Sequential)]
internal struct Shared
{
    // Version Number
    public UInt32 mVersion;                                     // [ RANGE = 0->... ]
    public UInt32 mBuildVersionNumber;                          // [ RANGE = 0->... ]   [ UNSET = 0 ]

    // Game States
    public UInt32 mGameState;                                   // [ enum (Type#1) Game state ]
    public UInt32 mSessionState;                                // [ enum (Type#2) Session state ]
    public UInt32 mRaceState;                                   // [ enum (Type#3) Race State ]

    // Participant Info
    public Int32 mViewedParticipantIndex;                       // [ RANGE = 0->STORED_PARTICIPANTS_MAX ]   [ UNSET = -1 ]
    public Int32 mNumParticipants;                              // [ RANGE = 0->STORED_PARTICIPANTS_MAX ]   [ UNSET = -1 ]

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)]
    public ParticipantInfo[] mParticipantInfo;                  // [ struct (Type#13) ParticipantInfo struct ]

    // Unfiltered Input
    public Single mUnfilteredThrottle;                          // [ RANGE = 0.0f->1.0f ]
    public Single mUnfilteredBrake;                             // [ RANGE = 0.0f->1.0f ]
    public Single mUnfilteredSteering;                          // [ RANGE = -1.0f->1.0f ]
    public Single mUnfilteredClutch;                            // [ RANGE = 0.0f->1.0f ]

    // Vehicle information
    public StringData mCarName;                                 // [ string ]
    public StringData mCarClassName;                            // [ string ]

    // Event information
    public UInt32 mLapsInEvent;                                 // [ RANGE = 0->... ]   [ UNSET = 0 ]
    public StringData mTrackLocation;                           // [ string ] - untranslated shortened English name
    public StringData mTrackVariation;                          // [ string ] - untranslated shortened English variation description
    public Single mTrackLength;                                 // [ UNITS = Metres ]   [ RANGE = 0.0f->... ]    [ UNSET = 0.0f ]

    // Timings
    public Int32 mNumSectors;                                   // [ RANGE = 0->... ]   [ UNSET = -1 ]
    [MarshalAs(UnmanagedType.I1)] public bool mLapInvalidated;  // [ UNITS = boolean ]   [ RANGE = false->true ]   [ UNSET = false ]
    public Single mBestLapTime;                                 // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mLastLapTime;                                 // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
    public Single mCurrentTime;                                 // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
    public Single mSplitTimeAhead;                              // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mSplitTimeBehind;                             // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mSplitTime;                                   // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
    public Single mEventTimeRemaining;                          // [ UNITS = milli-seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mPersonalFastestLapTime;                      // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mWorldFastestLapTime;                         // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mCurrentSector1Time;                          // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mCurrentSector2Time;                          // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mCurrentSector3Time;                          // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mFastestSector1Time;                          // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mFastestSector2Time;                          // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mFastestSector3Time;                          // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mPersonalFastestSector1Time;                  // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mPersonalFastestSector2Time;                  // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mPersonalFastestSector3Time;                  // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mWorldFastestSector1Time;                     // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mWorldFastestSector2Time;                     // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    public Single mWorldFastestSector3Time;                     // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]

    // Flags
    public UInt32 mHighestFlagColour;                           // [ enum (Type#5) Flag Colour ]
    public UInt32 mHighestFlagReason;                           // [ enum (Type#6) Flag Reason ]

    // Pit Info
    public UInt32 mPitMode;                                     // [ enum (Type#7) Pit Mode ]
    public UInt32 mPitSchedule;                                 // [ enum (Type#8) Pit Stop Schedule ]

    // Car State
    public UInt32 mCarFlags;                                    // [ enum (Type#9) Car Flags ]
    public Single mOilTempCelsius;                              // [ UNITS = Celsius ]   [ UNSET = 0.0f ]
    public Single mOilPressureKPa;                              // [ UNITS = Kilopascal ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
    public Single mWaterTempCelsius;                            // [ UNITS = Celsius ]   [ UNSET = 0.0f ]
    public Single mWaterPressureKPa;                            // [ UNITS = Kilopascal ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
    public Single mFuelPressureKPa;                             // [ UNITS = Kilopascal ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
    public Single mFuelLevel;                                   // [ RANGE = 0.0f->1.0f ]
    public Single mFuelCapacity;                                // [ UNITS = Liters ]   [ RANGE = 0.0f->1.0f ]   [ UNSET = 0.0f ]
    public Single mSpeed;                                       // [ UNITS = Metres per-second ]   [ RANGE = 0.0f->... ]
    public Single mRpm;                                         // [ UNITS = Revolutions per minute ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
    public Single mMaxRPM;                                      // [ UNITS = Revolutions per minute ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
    public Single mBrake;                                       // [ RANGE = 0.0f->1.0f ]
    public Single mThrottle;                                    // [ RANGE = 0.0f->1.0f ]
    public Single mClutch;                                      // [ RANGE = 0.0f->1.0f ]
    public Single mSteering;                                    // [ RANGE = -1.0f->1.0f ]
    public Int32 mGear;                                         // [ RANGE = -1 (Reverse)  0 (Neutral)  1 (Gear 1)  2 (Gear 2)  etc... ]   [ UNSET = 0 (Neutral) ]
    public Int32 mNumGears;                                     // [ RANGE = 0->... ]   [ UNSET = -1 ]
    public Single mOdometerKM;                                  // [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    [MarshalAs(UnmanagedType.I1)] public bool mAntiLockActive;  // [ UNITS = boolean ]   [ RANGE = false->true ]   [ UNSET = false ]
    public Int32 mLastOpponentCollisionIndex;                   // [ RANGE = 0->STORED_PARTICIPANTS_MAX ]   [ UNSET = -1 ]
    public Single mLastOpponentCollisionMagnitude;              // [ RANGE = 0.0f->... ]
    [MarshalAs(UnmanagedType.I1)] public bool mBoostActive;     // [ UNITS = boolean ]   [ RANGE = false->true ]   [ UNSET = false ]
    public Single mBoostAmount;                                 // [ RANGE = 0.0f->100.0f ]

    // Motion & Device Related
    public Vector3 mOrientation;                    // [ UNITS = Euler Angles ]
    public Vector3 mLocalVelocity;                  // [ UNITS = Metres per-second ]
    public Vector3 mWorldVelocity;                  // [ UNITS = Metres per-second ]
    public Vector3 mAngularVelocity;                // [ UNITS = Radians per-second ]
    public Vector3 mLocalAcceleration;              // [ UNITS = Metres per-second ]
    public Vector3 mWorldAcceleration;              // [ UNITS = Metres per-second ]
    public Vector3 mExtentsCentre;                  // [ UNITS = Local Space  X  Y  Z ]

    // Wheels / Tyres
    public Tyres<UInt32> mTyreFlags;                // [ enum (Type#10) Tyre Flags ]
    public Tyres<UInt32> mTerrain;                  // [ enum (Type#11) Terrain Materials ]
    public Tyres<Single> mTyreY;                    // [ UNITS = Local Space  Y ]
    public Tyres<Single> mTyreRPS;                  // [ UNITS = Revolutions per second ]
    public Tyres<Single> mTyreSlipSpeed;            // OBSOLETE, kept for backward compatibility only
    public Tyres<Single> mTyreTemp;                 // [ UNITS = Celsius ]   [ UNSET = 0.0f ]
    public Tyres<Single> mTyreGrip;                 // OBSOLETE, kept for backward compatibility only
    public Tyres<Single> mTyreHeightAboveGround;    // [ UNITS = Local Space  Y ]
    public Tyres<Single> mTyreLateralStiffness;     // OBSOLETE, kept for backward compatibility only
    public Tyres<Single> mTyreWear;                 // [ RANGE = 0.0f->1.0f ]
    public Tyres<Single> mBrakeDamage;              // [ RANGE = 0.0f->1.0f ]
    public Tyres<Single> mSuspensionDamage;         // [ RANGE = 0.0f->1.0f ]
    public Tyres<Single> mBrakeTempCelsius;         // [ UNITS = Celsius ]
    public Tyres<Single> mTyreTreadTemp;            // [ UNITS = Kelvin ]
    public Tyres<Single> mTyreLayerTemp;            // [ UNITS = Kelvin ]
    public Tyres<Single> mTyreCarcassTemp;          // [ UNITS = Kelvin ]
    public Tyres<Single> mTyreRimTemp;              // [ UNITS = Kelvin ]
    public Tyres<Single> mTyreInternalAirTemp;      // [ UNITS = Kelvin ]

    // Car Damage
    public UInt32 mCrashState;                      // [ enum (Type#12) Crash Damage State ]
    public Single mAeroDamage;                      // [ RANGE = 0.0f->1.0f ]
    public Single mEngineDamage;                    // [ RANGE = 0.0f->1.0f ]

    // Weather
    public Single mAmbientTemperature;              // [ UNITS = Celsius ]   [ UNSET = 25.0f ]
    public Single mTrackTemperature;                // [ UNITS = Celsius ]   [ UNSET = 30.0f ]
    public Single mRainDensity;                     // [ UNITS = How much rain will fall ]   [ RANGE = 0.0f->1.0f ]
    public Single mWindSpeed;                       // [ RANGE = 0.0f->100.0f ]   [ UNSET = 2.0f ]
    public Single mWindDirectionX;                  // [ UNITS = Normalised Vector X ]
    public Single mWindDirectionY;                  // [ UNITS = Normalised Vector Y ]
    public Single mCloudBrightness;                 // [ RANGE = 0.0f->... ]

    //PCars2 additions start, version 8
    // Sequence Number to help slightly with data integrity reads
    public volatile UInt32 mSequenceNumber;         // 0 at the start, incremented at start and end of writing, so odd when Shared Memory is being filled, even when the memory is not being touched

    //Additional car variables
    public Tyres<Single> mWheelLocalPositionY;                                      // [ UNITS = Local Space  Y ]
    public Tyres<Single> mSuspensionTravel;                                         // [ UNITS = meters ] [ RANGE 0.f =>... ]  [ UNSET =  0.0f ]
    public Tyres<Single> mSuspensionVelocity;                                       // [ UNITS = Rate of change of pushrod deflection ] [ RANGE 0.f =>... ]  [ UNSET =  0.0f ]
    public Tyres<Single> mAirPressure;                                              // [ UNITS = PSI ]  [ RANGE 0.f =>... ]  [ UNSET =  0.0f ]
    public Single mEngineSpeed;                                                     // [ UNITS = Rad/s ] [UNSET = 0.f ]
    public Single mEngineTorque;                                                    // [ UNITS = Newton Meters] [UNSET = 0.f ] [ RANGE = 0.0f->... ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public Single[] mWings;    // [ RANGE = 0.0f->1.0f ] [UNSET = 0.f ]
    public Single mHandBrake;                                                       // [ RANGE = 0.0f->1.0f ] [UNSET = 0.f ]

    // additional race variables Constants.Restrictions.STORED_PARTICIPANTS_MAX
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public Single[] mCurrentSector1Times;    // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public Single[] mCurrentSector2Times;    // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public Single[] mCurrentSector3Times;    // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public Single[] mFastestSector1Times;    // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public Single[] mFastestSector2Times;    // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public Single[] mFastestSector3Times;    // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public Single[] mFastestLapTimes;        // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public Single[] mLastLapTimes;           // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public byte[] mLapsInvalidated;          // [ UNITS = boolean for all participants ]   [ RANGE = false->true ]   [ UNSET = false ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public UInt32[] mRaceStates;             // [ enum (Type#3) Race State ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public UInt32[] mPitModes;               // [ enum (Type#7)  Pit Mode ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public Vector3[] mOrientations;          // [ UNITS = Euler Angles ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public Single[] mSpeeds;                 // [ UNITS = Metres per-second ]   [ RANGE = 0.0f->... ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public StringData[] mCarNames;           // [ string ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public StringData[] mCarClassNames;      // [ string ]

    // additional race variables
    public Int32 mEnforcedPitStopLap;               // [ UNITS = in which lap there will be a mandatory pitstop] [ RANGE = 0.0f->... ] [ UNSET = -1 ]
    public StringData mTranslatedTrackLocation;     // [ string ]
    public StringData mTranslatedTrackVariation;    // [ string ]
    public Single mBrakeBias;                       // [ RANGE = 0.0f->1.0f... ]   [ UNSET = -1.0f ]
    public Single mTurboBoostPressure;              // RANGE = 0.0f->1.0f... ]   [ UNSET = -1.0f ]
    public Tyres<CompoundName> mTyreCompound;       // [ strings  ]

    // Constants.Restrictions.STORED_PARTICIPANTS_MAX
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public UInt32[] mPitSchedules;        // [ enum (Type#7)  Pit Mode ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public UInt32[] mHighestFlagColours;  // [ enum (Type#5) Flag Colour ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public UInt32[] mHighestFlagReasons;  // [ enum (Type#6) Flag Reason ]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Constants.Restrictions.STORED_PARTICIPANTS_MAX)] public UInt32[] mNationalities;       // [ nationality table , SP AND UNSET = 0 ]

    public Single mSnowDensity; // [ UNITS = How much snow will fall ]   [ RANGE = 0.0f->1.0f ], this is non zero only in Winter and Snow seasons

    // AMS2 Additions (v10...)

    // Session info
    public Single mSessionDuration;         // [ UNITS = minutes ]   [ UNSET = 0.0f ]  The scheduled session Length (unset means laps race. See mLapsInEvent)
    public Int32 mSessionAdditionalLaps;    // The number of additional complete laps lead lap drivers must complete to finish a timed race after the session duration has elapsed.

    // Tyres
    public Tyres<Single> mTyreTempLeft;     // [ UNITS = Celsius ]   [ UNSET = 0.0f ]
    public Tyres<Single> mTyreTempCenter;   // [ UNITS = Celsius ]   [ UNSET = 0.0f ]
    public Tyres<Single> mTyreTempRight;    // [ UNITS = Celsius ]   [ UNSET = 0.0f ]

    // DRS
    public UInt32 mDrsState; // [ enum (Type#14) DrsState ]

    // Suspension
    public Tyres<Single> mRideHeight; // [ UNITS = cm ]

    // Input
    public UInt32 mJoyPad0; // button mask
    public UInt32 mDPad;    // button mask

    public Int32 mAntiLockSetting;          // [ UNSET = -1 ] Current ABS garage setting. Valid under player control only.
    public Int32 mTractionControlSetting;   // [ UNSET = -1 ] Current ABS garage setting. Valid under player control only.

    // ERS
    public Int32 mErsDeploymentMode;                                // [ enum (Type#15)  ErsDeploymentMode ]
    [MarshalAs(UnmanagedType.I1)] public bool mErsAutoModeEnabled;  // true if the deployment mode was selected by auto system. Valid only when mErsDeploymentMode > ERS_DEPLOYMENT_MODE_NONE

    // Clutch State & Damage
    public Single mClutchTemp;                                      // [ UNITS = Kelvin ] [ UNSET = -273.16 ]
    public Single mClutchWear;                                      // [ RANGE = 0.0f->1.0f... ]
    [MarshalAs(UnmanagedType.I1)] public bool mClutchOverheated;    // true if clutch performance is degraded due to overheating
    [MarshalAs(UnmanagedType.I1)] public bool mClutchSlipping;      // true if clutch is slipping (can be induced by overheating or wear)
    public Int32 mYellowFlagState;                                  // [ enum (Type#16) YellowFlagState ]
    [MarshalAs(UnmanagedType.I1)] public bool mSessionIsPrivate;    // true if this is a public session where users cannot see or interact with other drivers (and so would not need positional awareness of them etc)
    public Int32 mLaunchStage;                                      // [ enum (Type#17) LaunchStage

    public static readonly Int32 Size = Marshal.SizeOf(typeof(Shared));
    public static readonly byte[] Buffer = new byte[Size];
}
