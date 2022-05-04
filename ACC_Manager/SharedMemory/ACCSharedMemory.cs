using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ACCSetupApp.Controls.Telemetry.SharedMemory;

namespace ACCSetupApp
{
    /// <summary>
    /// Used certain shared memory from https://github.com/gro-ove/actools
    /// </summary>
    internal unsafe class ACCSharedMemory
    {
        private string physicsMap = "Local\\acpmf_physics";
        private string graphicsMap = "Local\\acpmf_graphics";
        private string staticMap = "Local\\acpmf_static";

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


        public static string SessionTypeToString(AcSessionType sessionType)
        {
            switch (sessionType)
            {
                case AcSessionType.AC_UNKNOWN: return "Unknown";
                case AcSessionType.AC_PRACTICE: return "Practice";
                case AcSessionType.AC_QUALIFY: return "Qualify";
                case AcSessionType.AC_RACE: return "Race";
                case AcSessionType.AC_HOTLAP: return "Hotlap";
                case AcSessionType.AC_TIME_ATTACK: return "Time attack";
                case AcSessionType.AC_DRIFT: return "Drift";
                case AcSessionType.AC_DRAG: return "Drag";
                case AcSessionType.AC_HOTSTINT: return "Hotstint";
                case AcSessionType.AC_HOTLAPSUPERPOLE: return "Hotlap superpole";

                default: return sessionType.ToString();
            }
        }

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

        public static string FlagTypeToString(AcFlagType flagType)
        {
            switch (flagType)
            {
                case AcFlagType.AC_NO_FLAG: return "Green";
                case AcFlagType.AC_BLUE_FLAG: return "Blue";
                case AcFlagType.AC_YELLOW_FLAG: return "Yellow";
                case AcFlagType.AC_BLACK_FLAG: return "Black";
                case AcFlagType.AC_WHITE_FLAG: return "White";
                case AcFlagType.AC_CHECKERED_FLAG: return "Checkered";
                case AcFlagType.AC_PENALTY_FLAG: return "Penalty";
                case AcFlagType.AC_GREEN_FLAG: return "Green";
                case AcFlagType.AC_BLACK_FLAG_WITH_ORANGE_CIRCLE: return "Black/Orange Circle";

                default: return flagType.ToString();
            }
        }

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
            Drizzle,
            Light_Rain,
            Medium_Rain,
            Heavy_Rain,
            Thunderstorm,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode), Serializable]
        internal class SPageFileGraphic
        {
            public int PacketId;
            public AcStatus Status;
            public AcSessionType SessionType;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string CurrentTime;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string LastTime;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string BestTime;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string Split;

            public int CompletedLaps;
            public int Position;
            public int CurrentTimeMs;
            public int LastTimeMs;
            public int BestTimeMs;
            public float SessionTimeLeft;
            public float DistanceTraveled;

            [MarshalAs(UnmanagedType.Bool)]
            public bool IsInPits;

            public int CurrentSectorIndex;
            public int LastSectorTime;
            public int NumberOfLaps;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string TyreCompound;

            [Obsolete]
            public float ReplayTimeMultiplier;
            public float NormalizedCarPosition;

            public int ActiveCars;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
            public StructVector3[] CarCoordinates;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
            public int[] CarIds;

            public int PlayerCarID;

            public float PenaltyTime;
            public AcFlagType Flag;

            public PenaltyShortcut PenaltyType;

            [MarshalAs(UnmanagedType.Bool)]
            public bool IdealLineOn;

            [MarshalAs(UnmanagedType.Bool)]
            public bool IsInPitLane;
            public float SurfaceGrip;

            [MarshalAs(UnmanagedType.Bool)]
            public bool MandatoryPitDone;

            public float WindSpeed;
            public float WindDirection;

            [MarshalAs(UnmanagedType.Bool)]
            public bool IsSetupMenuVisible;

            public int MainDisplayIndex;
            public int SecondaryDisplayIndex;
            public int TC;
            public int TCCut;
            public int EngineMap;
            public int ABS;
            public float FuelXLap;
            public int RainLights;
            public int FlashingLights;
            public int LightsStage;
            public float ExhaustTemperature;
            public int WiperLV;
            public int DriverStintTotalTimeLeft;
            public int DriverStintTimeLeft;
            public int RainTyres;

            public int SessionIndex;
            public float UsedFuelSinceRefuel;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string DeltaLapTime;

            public int DeltaLapTimeMillis;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string EstimatedLapTime;
            public int EstimatedLapTimeMillis;

            [MarshalAs(UnmanagedType.Bool)]
            public bool IsDeltaPositive;
            public int SplitTimeMillis;

            [MarshalAs(UnmanagedType.Bool)]
            public bool IsValidLap;
            public float FuelEstimatedLaps;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string TrackStatus;

            public int MandatoryPitStopsLeft;
            float ClockTimeDaySeconds;

            [MarshalAs(UnmanagedType.Bool)]
            public bool BlinkerLeftOn;

            [MarshalAs(UnmanagedType.Bool)]
            public bool BlinkerRightOn;

            [MarshalAs(UnmanagedType.Bool)]
            public bool GlobalYellow;

            [MarshalAs(UnmanagedType.Bool)]
            public bool GlobalYellowSector1;

            [MarshalAs(UnmanagedType.Bool)]
            public bool GlobalYellowSector2;

            [MarshalAs(UnmanagedType.Bool)]
            public bool GlobalYellowSector3;

            [MarshalAs(UnmanagedType.Bool)]
            public bool GlobalWhite;

            [MarshalAs(UnmanagedType.Bool)]
            public bool GreenFlag;

            [MarshalAs(UnmanagedType.Bool)]
            public bool GlobalChequered;

            [MarshalAs(UnmanagedType.Bool)]
            public bool GlobalRed;

            public int mfdTyreSet;
            public float mfdFuelToAdd;

            public float mfdTyrePressureLF;
            public float mfdTyrePressureRF;
            public float mfdTyrePressureLR;
            public float mfdTyrePressureRR;
            public AcTrackGripStatus trackGripStatus;
            public AcRainIntensity rainIntensity;
            public AcRainIntensity rainIntensityIn10min;
            public AcRainIntensity rainIntensityIn30min;
            public int currentTyreSet;
            public int strategyTyreSet;
            public int gapAheadMillis;
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

            public override string ToString()
            {
                return $"X: {X}, Y: {Y}, Z: {Z}";
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode), Serializable]
        public class SPageFilePhysics
        {
            public int PacketId;
            public float Gas;
            public float Brake;
            public float Fuel;
            public int Gear;
            public int Rpms;
            public float SteerAngle;
            public float SpeedKmh;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] Velocity;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] AccG;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] WheelSlip;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] WheelLoad;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] WheelPressure;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] WheelAngularSpeed;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreWear;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreDirtyLevel;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreCoreTemperature;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] CamberRad;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SuspensionTravel;

            [Obsolete]
            public float Drs;
            public float TC;
            public float Heading;
            public float Pitch;
            public float Roll;

            [Obsolete]
            public float CgHeight;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public float[] CarDamage;

            [Obsolete]
            public int NumberOfTyresOut;

            [MarshalAs(UnmanagedType.Bool)]
            public bool PitLimiterOn;
            public float Abs;

            [Obsolete]
            public float KersCharge;

            [Obsolete]
            public float KersInput;

            [MarshalAs(UnmanagedType.Bool)]
            public bool AutoShifterOn;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public float[] RideHeight;

            public float TurboBoost;

            [Obsolete]
            public float Ballast;
            [Obsolete]
            public float AirDensity;

            public float AirTemp;
            public float RoadTemp;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] LocalAngularVelocity;

            public float finalFF;

            [Obsolete]
            public float PerformanceMeter;
            [Obsolete]
            public int EngineBrake;
            [Obsolete]
            public int ErsRecoveryLevel;
            [Obsolete]
            public int ErsPowerLevel;
            [Obsolete]
            public int ErsHeatCharging;
            [Obsolete]
            public int ErsIsCharging;
            [Obsolete]
            public float KersCurrentKJ;

            [Obsolete]
            [MarshalAs(UnmanagedType.Bool)]
            public bool DrsAvailable;

            [Obsolete]
            [MarshalAs(UnmanagedType.Bool)]
            public bool DrsEnabled;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] BrakeTemperature;

            public float Clutch;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreTempI;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreTempM;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreTempO;

            [MarshalAs(UnmanagedType.Bool)]
            public bool IsAiControlled;


            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public StructVector3[] TyreContactPoint;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public StructVector3[] TyreContactNormal;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public StructVector3[] TyreContactHeading;

            public float BrakeBias;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] LocalVelocity;

            [Obsolete]
            public int P2PActivations;
            [Obsolete]
            public int P2PStatus;

            [Obsolete]
            public int CurrentMaxRpm;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] mz;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] fx;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] fy;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SlipRatio;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SlipAngle;

            [Obsolete]
            public int TcinAction;
            [Obsolete]
            public int AbsInAction;

            //[Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SuspensionDamage;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreTemp;

            public float WaterTemp;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] brakePressure;
            public int frontBrakeCompound;
            public int rearBrakeCompound;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] PadLife;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] DiscLife;

            [MarshalAs(UnmanagedType.Bool)]
            public bool IgnitionOn;

            [MarshalAs(UnmanagedType.Bool)]
            public bool StarterEngineOn;

            [MarshalAs(UnmanagedType.Bool)]
            public bool IsEngineRunning;

            public float KerbVibration;
            public float SlipVibrations;
            public float Gvibrations;
            public float AbsVibrations;

            public static readonly int Size = Marshal.SizeOf(typeof(SPageFilePhysics));
            public static readonly byte[] Buffer = new byte[Size];

        };

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode), Serializable]
        public class SPageFileStatic
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string SharedMemoryVersion;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string AssettoCorsaVersion;

            public int NumberOfSessions;
            public int NumberOfCars;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string CarModel;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string Track;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string PlayerName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string PlayerSurname;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string PlayerNickname;

            public int SectorCount;

            // car static info
            [Obsolete]
            public float MaxTorque;
            [Obsolete]
            public float MaxPower;
            public int MaxRpm;
            public float MaxFuel;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SuspensionMaxTravel;

            [Obsolete]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreRadius;

            public float MaxTurboBoost;

            [Obsolete]
            public float AirTemperature;

            [Obsolete]
            public float RoadTemperature;

            [MarshalAs(UnmanagedType.Bool)]
            public bool PenaltiesEnabled;
            public float AidFuelRate;
            public float AidTireRate;
            public float AidMechanicalDamage;
            [MarshalAs(UnmanagedType.Bool)]
            public bool AidAllowTyreBlankets;
            public float AidStability;
            [MarshalAs(UnmanagedType.Bool)]
            public bool AidAutoClutch;
            [MarshalAs(UnmanagedType.Bool)]
            public bool AidAutoBlip;

            [Obsolete]
            [MarshalAs(UnmanagedType.Bool)]
            public bool HasDRS;

            [Obsolete]
            [MarshalAs(UnmanagedType.Bool)]
            public bool HasERS;

            [Obsolete]
            [MarshalAs(UnmanagedType.Bool)]
            public bool HasKERS;
            [Obsolete]
            public float KersMaxJoules;
            [Obsolete]
            public int EngineBrakeSettingsCount;
            [Obsolete]
            public int ErsPowerControllerCount;
            [Obsolete]
            public float TrackSplineLength;
            [Obsolete]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string TrackConfiguration;
            [Obsolete]
            public float ErsMaxJ;
            [Obsolete]
            [MarshalAs(UnmanagedType.Bool)]
            public bool IsTimedRace;
            [Obsolete]
            [MarshalAs(UnmanagedType.Bool)]
            public bool HasExtraLap;
            [Obsolete]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string CarSkin;
            [Obsolete]
            public int ReversedGridPositions;

            public int PitWindowStart;
            public int PitWindowEnd;


            [MarshalAs(UnmanagedType.Bool)]
            public bool isOnline;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string DryTyresName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string WetTyresName;

            public static readonly int Size = Marshal.SizeOf(typeof(SPageFileStatic));
            public static readonly byte[] Buffer = new byte[Size];
        };

        public SPageFileGraphic ReadGraphicsPageFile()
        {
            var mappedFile = MemoryMappedFile.CreateOrOpen(graphicsMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite);
            SPageFileGraphic data = StructExtension.ToStruct<SPageFileGraphic>(mappedFile, SPageFileGraphic.Buffer);
            return data;
        }

        public SPageFileStatic ReadStaticPageFile()
        {
            var mappedFile = MemoryMappedFile.CreateOrOpen(staticMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite);
            SPageFileStatic data = StructExtension.ToStruct<SPageFileStatic>(mappedFile, SPageFileStatic.Buffer);
            return data;
        }

        public SPageFilePhysics ReadPhysicsPageFile()
        {
            var mappedFile = MemoryMappedFile.CreateOrOpen(physicsMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite);
            SPageFilePhysics data = StructExtension.ToStruct<SPageFilePhysics>(mappedFile, SPageFilePhysics.Buffer);
            return data;
        }
    }
}
