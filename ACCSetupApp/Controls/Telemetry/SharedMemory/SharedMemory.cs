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
    internal unsafe class SharedMemory
    {
        // TODO https://github.com/gro-ove/actools/blob/master/AcManager.Tools/SharedMemory/AcSharedMemory.cs

        private string physicsMap = "Local\\acpmf_physics";
        private string graphicsMap = "Local\\acpmf_graphics";
        private string staticMap = "Local\\acpmf_static";


        public enum AC_STATUS : int
        {
            AC_OFF,
            AC_REPLAY,
            AC_LIVE,
            AC_PAUSE,
        }

        public enum AC_SESSION_TYPE : int
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

        public enum AC_FLAG_TYPE : int
        {
            AC_NO_FLAG,
            AC_BLUE_FLAG,
            AC_YELLOW_FLAG,
            AC_BLACK_FLAG,
            AC_WHITE_FLAG,
            AC_CHECKERED_FLAG,
            AC_PENALTY_FLAG
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

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode), Serializable]
        internal class SPageFileGraphic
        {
            public int PacketId;
            public AC_STATUS Status;
            public AC_SESSION_TYPE Session;

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

            public float ReplayTimeMultiplier;
            public float NormalizedCarPosition;

            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public Vector3 CarCoordinates;

            public float PenaltyTime;
            public AC_FLAG_TYPE Flag;

            [MarshalAs(UnmanagedType.Bool)]
            public bool IdealLineOn;

            // added in 1.5
            [MarshalAs(UnmanagedType.Bool)]
            public bool IsInPitLane;
            public float SurfaceGrip;

            // added in 1.13
            [MarshalAs(UnmanagedType.Bool)]
            public bool MandatoryPitDone;

            public float windSpeed;
            public float windDirection;


            public int isSetupMenuVisible;

            public int mainDisplayIndex;
            public int secondaryDisplayIndex;
            public int TC;
            public int TCCut;
            public int EngineMap;
            public int ABS;
            public int fuelXLap;
            public int rainLights;
            public int flashingLights;
            public int lightsStage;
            public float exhaustTemperature;
            public int wiperLV;
            public int DriverStintTotalTimeLeft;
            public int DriverStintTimeLeft;
            public int rainTyres;


            public static readonly int Size = Marshal.SizeOf(typeof(SPageFileGraphic));
            public static readonly byte[] Buffer = new byte[Size];
        };


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

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] WheelLoad;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] WheelPressure;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] WheelAngularSpeed;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreWear;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreDirtyLevel;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreCoreTemperature;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] CamberRad;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SuspensionTravel;

            public float Drs;
            public float TC;
            public float Heading;
            public float Pitch;
            public float Roll;
            public float CgHeight;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public float[] CarDamage;

            public int NumberOfTyresOut;

            [MarshalAs(UnmanagedType.Bool)]
            public bool PitLimiterOn;
            public float Abs;

            public float KersCharge;
            public float KersInput;

            [MarshalAs(UnmanagedType.Bool)]
            public bool AutoShifterOn;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public float[] RideHeight;

            public float TurboBoost;
            public float Ballast;
            public float AirDensity;

            public float AirTemp;
            public float RoadTemp;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] LocalAngularVelocity;

            public float finalFF;
            public float performanceMeter;


            public float PerformanceMeter;
            public int EngineBrake;
            public int ErsRecoveryLevel;
            public int ErsPowerLevel;
            public int ErsHeatCharging;
            public int ErsIsCharging;
            public float KersCurrentKJ;

            [MarshalAs(UnmanagedType.Bool)]
            public bool DrsAvailable;

            [MarshalAs(UnmanagedType.Bool)]
            public bool DrsEnabled;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] BrakeTemperature;

            public float Clutch;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreTempI;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreTempM;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreTempO;

            [MarshalAs(UnmanagedType.Bool)]
            public bool IsAiControlled;


            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public Vector3[] TyreContactPoint;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public Vector3[] TyreContactNormal;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public Vector3[] TyreContactHeading;

            public float BrakeBias;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] LocalVelocity;

            public int P2PActivations;
            public int P2PStatus;

            public int CurrentMaxRpm;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] mz;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] fx;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] fy;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SlipRatio;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SlipAngle;


            public int TcinAction;
            public int AbsInAction;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SuspensionDamage;

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

            public int IgnitionOn;
            public int StarterEngineOn;
            public int IsEngineRunning;
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

            // session static info
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
            public float MaxTorque;
            public float MaxPower;
            public int MaxRpm;
            public float MaxFuel;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] SuspensionMaxTravel;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] TyreRadius;

            // added in 1.5
            public float MaxTurboBoost;

            [Obsolete]
            public float AirTemperature; // AirTemp since 1.6 in physic

            [Obsolete]
            public float RoadTemperature; // RoadTemp since 1.6 in physic

            public int PenaltiesEnabled;
            public float AidFuelRate;
            public float AidTireRate;
            public float AidMechanicalDamage;
            public int AidAllowTyreBlankets;
            public float AidStability;
            public int AidAutoClutch;
            public int AidAutoBlip;

            // added in 1.7.1
            [MarshalAs(UnmanagedType.Bool)]
            public bool HasDRS;

            [MarshalAs(UnmanagedType.Bool)]
            public bool HasERS;

            [MarshalAs(UnmanagedType.Bool)]
            public bool HasKERS;

            public float KersMaxJoules;
            public int EngineBrakeSettingsCount;
            public int ErsPowerControllerCount;

            // added in 1.7.2
            public float TrackSplineLength;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string TrackConfiguration;

            // added in 1.10.2
            public float ErsMaxJ;

            // added in 1.13
            [MarshalAs(UnmanagedType.Bool)]
            public bool IsTimedRace;

            [MarshalAs(UnmanagedType.Bool)]
            public bool HasExtraLap;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string CarSkin;

            public int ReversedGridPositions;
            public int PitWindowStart;
            public int PitWindowEnd;

            public int isOnline;

            public static readonly int Size = Marshal.SizeOf(typeof(SPageFileStatic));
            public static readonly byte[] Buffer = new byte[Size];
        };

        public SPageFileGraphic ReadGraphicsPageFile()
        {
            var mappedFile = MemoryMappedFile.CreateOrOpen(graphicsMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite);
            var data = StructExtension.ToStruct<SPageFileGraphic>(mappedFile, SPageFileGraphic.Buffer);
            return data;
        }

        public SPageFileStatic ReadStaticPageFile()
        {
            var mappedFile = MemoryMappedFile.CreateOrOpen(staticMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite);
            var data = StructExtension.ToStruct<SPageFileStatic>(mappedFile, SPageFileStatic.Buffer);
            return data;
        }

        public SPageFilePhysics ReadPhysicsPageFile()
        {
            var mappedFile = MemoryMappedFile.CreateOrOpen(physicsMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite);
            var data = StructExtension.ToStruct<SPageFilePhysics>(mappedFile, SPageFilePhysics.Buffer);
            return data;
        }
    }
}
