using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCSetupApp
{
    internal unsafe class SharedMemory
    {
        private string physicsMap = "Local\\acpmf_physics";
        private string graphicsMap = "Local\\acpmf_graphics";
        private string staticMap = "Local\\acpmf_static";


        enum AC_STATUS : int
        {
            AC_OFF,
            AC_REPLAY,
            AC_LIVE,
            AC_PAUSE,
        }

        enum AC_SESSION_TYPE : int
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

        enum AC_FLAG_TYPE : int
        {
            AC_NO_FLAG,
            AC_BLUE_FLAG,
            AC_YELLOW_FLAG,
            AC_BLACK_FLAG,
            AC_WHITE_FLAG,
            AC_CHECKERED_FLAG,
            AC_PENALTY_FLAG
        }

        enum PenaltyShortcut : int
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

        [StructLayout(LayoutKind.Sequential)]
        internal struct SPageFileGraphic
        {
            public int packetId;
            public int acStatus;
            public int acSessionType;
            public fixed char currentTime[15];
            public fixed char lastTime[15];
            public fixed char bestTime[15];
            public fixed char split[15];
            public int completedLaps;
            public int position;
            public int iCurrentTime;
            public int iLastTime;
            public int iBestTime;
            public float sessionTimeLeft;
            public float distanceTraveled;
            public int isInPit;
            public int currentSectorIndex;
            public int lastSectorTime;
            public int numberOfLaps;
            public fixed char tyreCompound[33];
            public float replayTimeMultiplier;
            public float normalizedCarPosition;

            public int activeCars;
            //public float[][] carCoordinates; // [60][3]
            public fixed int carID[60];
            public int playerCarID;
            public float penaltyTime;
            public int flagType;
            public int penaltyShortcut;
            public int idealLineOn;
            public int isInPitLane;

            public float surfaceGrip;
            public int mandatoryPitDone;

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
        };


        [StructLayout(LayoutKind.Sequential)]
        internal struct SPageFilePhysics
        {
            public int packetId;
            public float gas;
            public float brake;
            public float fuel;
            public int gear;
            public int rpms;
            public float steerAngle;
            public float speedKmh;
            public fixed float velocity[3];
            public fixed float accG[3];
            public fixed float wheelSlip[4];
            public fixed float wheelLoad[4];
            public fixed float wheelsPressure[4];
            public fixed float wheelAngularSpeed[4];
            public fixed float tyreWear[4];
            public fixed float tyreDirtyLevel[4];
            public fixed float tyreCoreTemperature[4];
            public fixed float camberRAD[4];
            public fixed float suspensionTravel[4];
            public float drs;
            public float tc;
            public float heading;
            public float pitch;
            public float roll;
            public float cgHeight;
            public fixed float carDamage[5];
            public int numberOfTyresOut;
            public int pitLimiterOn;
            public float abs;
            public float kersCharge;
            public float kersInput;
            public int autoShifterOn;
            public fixed float rideHeight[2];
            public float turboBoost;
            public float ballast;
            public float airDensity;
            public float airTemp;
            public float roadTemp;
            public fixed float localAngularVel[3];
            public float finalFF;
            public float performanceMeter;

            public int engineBrake;
            public int ersRecoveryLevel;
            public int ersPowerLevel;
            public int ersHeatCharging;
            public int ersIsCharging;
            public float kersCurrentKJ;

            public int drsAvailable;
            public int drsEnabled;

            public fixed float brakeTemp[4];
            public float clutch;

            public fixed float tyreTempI[4];
            public fixed float tyreTempM[4];
            public fixed float tyreTempO[4];

            public int isAIControlled;

            //public float[][] tyreContactPoint;  
            //public float[][] tyreContactNormal;
            //public float[][] tyreContactHeading;

            public float brakeBias;

            public fixed float localVelocity[3];

            public int P2PActivations;
            public int P2PStatus;

            public int currentMaxRpm;

            public fixed float mz[4];
            public fixed float fx[4];
            public fixed float fy[4];
            public fixed float slipRatio[4];
            public fixed float slipAngle[4];


            public int tcinAction;
            public int absInAction;
            public fixed float suspensionDamage[4];
            public fixed float tyreTemp[4];
        };

        [StructLayout(LayoutKind.Sequential)]
        internal struct SPageFileStatic
        {
            public fixed char smVersion[15];
            public fixed char acVersion[15];

            // session static info
            public int numberOfSessions;
            public int numCars;
            public fixed char carModel[33];
            public fixed char track[33];
            public fixed char playerName[33];
            public fixed char playerSurname[33];
            public fixed char playerNick[33];
            public int sectorCount;

            //// car static info
            public float maxTorque;
            public float maxPower;
            public int maxRpm;
            public float maxFuel;
            public fixed float suspensionMaxTravel[4];
            public fixed float tyreRadius[4];
            public float maxTurboBoost;

            public float deprecated_1;
            public float deprecated_2;

            public int penaltiesEnabled;

            public float aidFuelRate;
            public float aidTireRate;
            public float aidMechanicalDamage;
            public int aidAllowTyreBlankets;
            public float aidStability;
            public int aidAutoClutch;
            public int aidAutoBlip;

            public int hasDRS;
            public int hasERS;
            public int hasKERS;
            public float kersMaxJ;
            public int engineBrakeSettingsCount;
            public int ersPowerControllerCount;
            public float trackSPlineLength;
            public fixed char trackConfiguration[33];
            public float ersMaxJ;

            public int isTimedRace;
            public int hasExtraLap;

            public fixed char carSkin[33];
            public int reversedGridPositions;
            public int PitWindowStart;
            public int PitWindowEnd;
            public int isOnline;
        };

        public unsafe SharedMemory()
        {
            //SPageFileStatic pageStatic = ReadStaticPageFile();
            //SPageFileGraphic pageGraphic = ReadGraphicsPageFile();
            //SPageFilePhysics pagePhysics = ReadPhysicsPageFile();

            //string info = "Page File Static:";
            //info += $"\nSM Version: {new string(pageStatic.smVersion)}";
            //info += $"\nAC Version: {new string(pageStatic.acVersion)}";
            //info += $"\nNumber of sessions: {pageStatic.numberOfSessions}";
            //info += $"\nNumber of cars: {pageStatic.numCars}";
            //info += $"\nCar model: {new string(pageStatic.carModel)}";
            //info += $"\nTrack: {new string(pageStatic.track)}";
            //info += $"\nTrack.Config {new string(pageStatic.trackConfiguration)}";
            //info += $"\nPlayer.Name: {new string(pageStatic.playerName)}";
            //info += $"\nPlayer.Surname: {new string(pageStatic.playerSurname)}";
            //info += $"\nPlayer.Nickname: {new string(pageStatic.playerNick)}";
            //info += $"\nSector count: {pageStatic.sectorCount}";
            //info += $"\n\n";
            //info += $"Car.Skin: {new string(pageStatic.carSkin)}";
            //info += $"\nCar.MaxTorque: {pageStatic.maxTorque}";
            //info += $"\nCar.MaxPower: {pageStatic.maxPower}";
            //info += $"\nCar.MaxRPM: {pageStatic.maxRpm}";
            //info += $"\nCar.MaxFuel {pageStatic.maxFuel}";
            //info += $"\nCar.maxSuspensionTravel: {pageStatic.suspensionMaxTravel[0]}, {pageStatic.suspensionMaxTravel[1]}, {pageStatic.suspensionMaxTravel[2]}, {pageStatic.suspensionMaxTravel[3]}";
            //info += $"\nCar.TyreRadius: {pageStatic.tyreRadius[0]}, {pageStatic.tyreRadius[1]}, {pageStatic.tyreRadius[2]}, {pageStatic.tyreRadius[3]}";
            //info += $"\nCar.MaxTurboBoost: {pageStatic.maxTurboBoost}";


            //info += $"\n\n\n Tyre.Pressures: {pagePhysics.wheelsPressure[0]},{pagePhysics.wheelsPressure[1]},{pagePhysics.wheelsPressure[2]},{pagePhysics.wheelsPressure[3]},";
            //Debug.WriteLine(info);

        }


        public unsafe SPageFileGraphic ReadGraphicsPageFile()
        {
            var mappedFile = MemoryMappedFile.CreateOrOpen(graphicsMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite);
            MemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
            accessor.Read(0, out SPageFileGraphic pageFile);
            return pageFile;
        }

        public unsafe SPageFileStatic ReadStaticPageFile()
        {
            var mappedFile = MemoryMappedFile.CreateOrOpen(staticMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite);
            MemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
            accessor.Read(0, out SPageFileStatic pageFile);
            return pageFile;
        }

        public unsafe SPageFilePhysics ReadPhysicsPageFile()
        {
            var mappedFile = MemoryMappedFile.CreateOrOpen(physicsMap, sizeof(byte), MemoryMappedFileAccess.ReadWrite);
            MemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
            accessor.Read(0, out SPageFilePhysics pageFile);
            return pageFile;
        }
    }
}
