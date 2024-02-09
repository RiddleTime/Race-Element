using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games.iRacing.SDK
{
    public class IRacingSdkEnum
    {
        public enum StatusField
        {
            StConnected = 1
        }

        public enum VarType
        {
            Char = 0,
            Bool,
            Int,
            BitField,
            Float,
            Double
        };

        [Flags]
        public enum EngineWarnings : uint
        {
            WaterTempWarning = 0x01,
            FuelPressureWarning = 0x02,
            OilPressureWarning = 0x04,
            EngineStalled = 0x08,
            PitSpeedLimiter = 0x10,
            RevLimiterActive = 0x20,
            OilTempWarning = 0x40
        };

        [Flags]
        public enum Flags : uint
        {
            Checkered = 0x00000001,
            White = 0x00000002,
            Green = 0x00000004,
            Yellow = 0x00000008,
            Red = 0x00000010,
            Blue = 0x00000020,
            Debris = 0x00000040,
            Crossed = 0x00000080,
            YellowWaving = 0x00000100,
            OneLapToGreen = 0x00000200,
            GreenHeld = 0x00000400,
            TenToGo = 0x00000800,
            FiveToGo = 0x00001000,
            RandomWaving = 0x00002000,
            Caution = 0x00004000,
            CautionWaving = 0x00008000,
            Black = 0x00010000,
            Disqualify = 0x00020000,
            Servicible = 0x00040000,
            Furled = 0x00080000,
            Repair = 0x00100000,
            StartHidden = 0x10000000,
            StartReady = 0x20000000,
            StartSet = 0x40000000,
            StartGo = 0x80000000
        };

        public enum TrkLoc
        {
            NotInWorld = -1,
            OffTrack,
            InPitStall,
            AproachingPits,
            OnTrack
        };

        public enum TrkSurf
        {
            SurfaceNotInWorld = -1,
            UndefinedMaterial = 0,
            Asphalt1Material,
            Asphalt2Material,
            Asphalt3Material,
            Asphalt4Material,
            Concrete1Material,
            Concrete2Material,
            RacingDirt1Material,
            RacingDirt2Material,
            Paint1Material,
            Paint2Material,
            Rumble1Material,
            Rumble2Material,
            Rumble3Material,
            Rumble4Material,
            Grass1Material,
            Grass2Material,
            Grass3Material,
            Grass4Material,
            Dirt1Material,
            Dirt2Material,
            Dirt3Material,
            Dirt4Material,
            SandMaterial,
            Gravel1Material,
            Gravel2Material,
            GrasscreteMaterial,
            AstroturfMaterial
        };

        public enum SessionState
        {
            Invalid,
            GetInCar,
            Warmup,
            ParadeLaps,
            Racing,
            Checkered,
            CoolDown
        };

        public enum CarLeftRight
        {
            Off,
            Clear,
            CarLeft,
            CarRight,
            CarLeftRight,
            TwoCarsLeft,
            TwoCarsRight
        };

        [Flags]
        public enum CameraState : uint
        {
            IsSessionScreen = 0x0001,
            IsScenicActive = 0x0002,
            CamToolActive = 0x0004,
            UIHidden = 0x0008,
            UseAutoShotSelection = 0x0010,
            UseTemporaryEdits = 0x0020,
            UseKeyAcceleration = 0x0040,
            UseKey10xAcceleration = 0x0080,
            UseMouseAimMode = 0x0100
        };

        [Flags]
        public enum PitSvFlags : uint
        {
            LFTireChange = 0x0001,
            RFTireChange = 0x0002,
            LRTireChange = 0x0004,
            RRTireChange = 0x0008,
            FuelFill = 0x0010,
            WindshieldTearoff = 0x0020,
            FastRepair = 0x0040
        }

        public enum PitSvStatus
        {
            None = 0,
            InProgress,
            Complete,
            TooFarLeft = 100,
            TooFarRight,
            TooFarForward,
            TooFarBack,
            BadAngle,
            CantFixThat
        };

        public enum PaceMode
        {
            SingleFileStart = 0,
            DoubleFileStart,
            SingleFileRestart,
            DoubleFileRestart,
            NotPacing
        };

        [Flags]
        public enum PaceFlags : uint
        {
            EndOfLine = 0x01,
            FreePass = 0x02,
            WavedAround = 0x04
        };

        public enum BroadcastMsg
        {
            CamSwitchPos = 0,
            CamSwitchNum,
            CamSetState,
            ReplaySetPlaySpeed,
            ReplaySetPlayPosition,
            ReplaySearch,
            ReplaySetState,
            ReloadTextures,
            ChatComand,
            PitCommand,
            TelemCommand,
            FFBCommand,
            ReplaySearchSessionTime,
            VideoCapture,
            Last
        };

        public enum ChatCommandMode
        {
            Macro = 0,
            BeginChat,
            Reply,
            Cancel
        };

        public enum PitCommandMode
        {
            Clear = 0,
            WS,
            Fuel,
            LF,
            RF,
            LR,
            RR,
            ClearTires,
            FR,
            ClearWS,
            ClearFR,
            ClearFuel
        };

        public enum TelemCommandMode
        {
            Stop = 0,
            Start,
            Restart
        };

        public enum RpyStateMode
        {
            EraseTape = 0,
            Last
        };

        public enum ReloadTexturesMode
        {
            All = 0,
            CarIdx
        };

        public enum RpySrchMode
        {
            ToStart = 0,
            ToEnd,
            PrevSession,
            NextSession,
            PrevLap,
            NextLap,
            PrevFrame,
            NextFrame,
            PrevIncident,
            NextIncident,
            Last
        };

        public enum RpyPosMode
        {
            Begin = 0,
            Current,
            End,
            Last
        };

        public enum FFBCommandMode
        {
            MaxForce = 0,
            Last
        };

        public enum CamSwitchMode
        {
            FocusAtIncident = -3,
            FocusAtLeader = -2,
            FocusAtExiting = -1,
            FocusAtDriver = 0
        };

        public enum VideoCaptureMode
        {
            TriggerScreenShot = 0,
            StartVideoCapture,
            EndVideoCapture,
            ToggleVideoCapture,
            ShowVideoTimer,
            HideVideoTimer
        };
    }
}
