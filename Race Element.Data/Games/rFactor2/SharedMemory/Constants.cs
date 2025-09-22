using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games.rFactor2.SharedMemory
{
    class Constants
    {
        public const string MM_TELEMETRY_FILE_NAME = "$rFactor2SMMP_Telemetry$";
        public const string MM_SCORING_FILE_NAME = "$rFactor2SMMP_Scoring$";
        public const string MM_RULES_FILE_NAME = "$rFactor2SMMP_Rules$";
        public const string MM_FORCE_FEEDBACK_FILE_NAME = "$rFactor2SMMP_ForceFeedback$";
        public const string MM_GRAPHICS_FILE_NAME = "$rFactor2SMMP_Graphics$";
        public const string MM_PITINFO_FILE_NAME = "$rFactor2SMMP_PitInfo$";
        public const string MM_WEATHER_FILE_NAME = "$rFactor2SMMP_Weather$";
        public const string MM_EXTENDED_FILE_NAME = "$rFactor2SMMP_Extended$";

        public const string MM_HWCONTROL_FILE_NAME = "$rFactor2SMMP_HWControl$";
        public const int MM_HWCONTROL_LAYOUT_VERSION = 1;

        public const string MM_WEATHER_CONTROL_FILE_NAME = "$rFactor2SMMP_WeatherControl$";
        public const int MM_WEATHER_CONTROL_LAYOUT_VERSION = 1;

        public const string MM_RULES_CONTROL_FILE_NAME = "$rFactor2SMMP_RulesControl$";
        public const int MM_RULES_CONTROL_LAYOUT_VERSION = 1;

        public const string MM_PLUGIN_CONTROL_FILE_NAME = "$rFactor2SMMP_PluginControl$";
        public const int MM_PLUGIN_CONTROL_LAYOUT_VERSION = 1;

        public const int MAX_MAPPED_VEHICLES = 128;
        public const int MAX_MAPPED_IDS = 512;
        public const int MAX_STATUS_MSG_LEN = 128;
        public const int MAX_RULES_INSTRUCTION_MSG_LEN = 96;
        public const int MAX_HWCONTROL_NAME_LEN = 96;
        public const string RFACTOR2_PROCESS_NAME = "rFactor2";

        public const byte RowX = 0;
        public const byte RowY = 1;
        public const byte RowZ = 2;

        // 0 Before session has begun
        // 1 Reconnaissance laps (race only)
        // 2 Grid walk-through (race only)
        // 3 Formation lap (race only)
        // 4 Starting-light countdown has begun (race only)
        // 5 Green flag
        // 6 Full course yellow / safety car
        // 7 Session stopped
        // 8 Session over
        // 9 Paused (tag.2015.09.14 - this is new, and indicates that this is a heartbeat call to the plugin)
        public enum RF2GamePhase
        {
            Garage = 0,
            WarmUp = 1,
            GridWalk = 2,
            Formation = 3,
            Countdown = 4,
            GreenFlag = 5,
            FullCourseYellow = 6,
            SessionStopped = 7,
            SessionOver = 8,
            PausedOrHeartbeat = 9
        }

        // Yellow flag states (applies to full-course only)
        // -1 Invalid
        //  0 None
        //  1 Pending
        //  2 Pits closed
        //  3 Pit lead lap
        //  4 Pits open
        //  5 Last lap
        //  6 Resume
        //  7 Race halt (not currently used)
        public enum RF2YellowFlagState
        {
            Invalid = -1,
            NoFlag = 0,
            Pending = 1,
            PitClosed = 2,
            PitLeadLap = 3,
            PitOpen = 4,
            LastLap = 5,
            Resume = 6,
            RaceHalt = 7
        }

        // 0=dry, 1=wet, 2=grass, 3=dirt, 4=gravel, 5=rumblestrip, 6=special
        public enum RF2SurfaceType
        {
            Dry = 0,
            Wet = 1,
            Grass = 2,
            Dirt = 3,
            Gravel = 4,
            Kerb = 5,
            Special = 6
        }

        // 0=sector3, 1=sector1, 2=sector2 (don't ask why)
        public enum RF2Sector
        {
            Sector3 = 0,
            Sector1 = 1,
            Sector2 = 2
        }

        // 0=none, 1=finished, 2=dnf, 3=dq
        public enum RF2FinishStatus
        {
            None = 0,
            Finished = 1,
            Dnf = 2,
            Dq = 3
        }

        // who's in control: -1=nobody (shouldn't get this), 0=local player, 1=local AI, 2=remote, 3=replay (shouldn't get this)
        public enum RF2Control
        {
            Nobody = -1,
            Player = 0,
            AI = 1,
            Remote = 2,
            Replay = 3
        }

        // wheel info (front left, front right, rear left, rear right)
        public enum RF2WheelIndex
        {
            FrontLeft = 0,
            FrontRight = 1,
            RearLeft = 2,
            RearRight = 3
        }

        // 0=none, 1=request, 2=entering, 3=stopped, 4=exiting
        public enum RF2PitState
        {
            None = 0,
            Request = 1,
            Entering = 2,
            Stopped = 3,
            Exiting = 4
        }

        // primary flag being shown to vehicle (currently only 0=green or 6=blue)
        public enum RF2PrimaryFlag
        {
            Green = 0,
            Blue = 6
        }

        // 0 = do not count lap or time, 1 = count lap but not time, 2 = count lap and time
        public enum RF2CountLapFlag
        {
            DoNotCountLap = 0,
            CountLapButNotTime = 1,
            CountLapAndTime = 2,
        }

        // 0=disallowed, 1=criteria detected but not allowed quite yet, 2=allowed
        public enum RF2RearFlapLegalStatus
        {
            Disallowed = 0,
            DetectedButNotAllowedYet = 1,
            Alllowed = 2
        }

        // 0=off 1=ignition 2=ignition+starter
        public enum RF2IgnitionStarterStatus
        {
            Off = 0,
            Ignition = 1,
            IgnitionAndStarter = 2
        }

        // 0=no change, 1=go active, 2=head for pits
        public enum RF2SafetyCarInstruction
        {
            NoChange = 0,
            GoActive = 1,
            HeadForPits = 2
        }
    }

}
