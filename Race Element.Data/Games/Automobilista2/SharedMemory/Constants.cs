namespace RaceElement.Data.Games.Automobilista2.SharedMemory;

internal sealed class Constants
{
    public const string SharedMemoryName = "$pcars2$";

    public enum Version
    {
        /// <summary>
        /// Shared memory current version.
        /// </summary>
        SHARED_MEMORY_VERSION = 14
    }

    public enum Restrictions
    {
        /// <summary>
        /// Maximum allowed length of string.
        /// </summary>
        STRING_LENGTH_MAX = 64,

        /// <summary>
        /// Maximum number of general participant information allowed to be stored in memory-mapped file.
        /// </summary>
        STORED_PARTICIPANTS_MAX = 64,

        /// <summary>
        /// Maximum length of a tyre compound name.
        /// </summary>
        TYRE_COMPOUND_NAME_LENGTH_MAX = 40
    }

    public enum Tyres
    {
        TYRE_FRONT_LEFT = 0,
        TYRE_FRONT_RIGHT,
        TYRE_REAR_LEFT,
        TYRE_REAR_RIGHT,
        TYRE_MAX
    }

    public enum Vector
    {
        VEC_X = 0,
        VEC_Y,
        VEC_Z,
        VEC_MAX
    }

    public enum GameState // (Type#1) GameState (to be used with 'mGameState')
    {
        GAME_EXITED = 0,
        GAME_FRONT_END,
        GAME_INGAME_PLAYING,
        GAME_INGAME_PAUSED,
        GAME_INGAME_INMENU_TIME_TICKING,
        GAME_INGAME_RESTARTING,
        GAME_INGAME_REPLAY,
        GAME_FRONT_END_REPLAY,
        GAME_MAX
    }

    public enum RaceSession // (Type#2) Session state (to be used with 'mSessionState')
    {
        SESSION_INVALID = 0,
        SESSION_PRACTICE,
        SESSION_TEST,
        SESSION_QUALIFY,
        SESSION_FORMATION_LAP,
        SESSION_RACE,
        SESSION_TIME_ATTACK,
        SESSION_MAX
    }

    public enum RaceState // (Type#3) RaceState (to be used with 'mRaceState' and 'mRaceStates')
    {
        RACESTATE_INVALID,
        RACESTATE_NOT_STARTED,
        RACESTATE_RACING,
        RACESTATE_FINISHED,
        RACESTATE_DISQUALIFIED,
        RACESTATE_RETIRED,
        RACESTATE_DNF,
        RACESTATE_MAX
    }

    public enum RaceFlags // (Type#5) Flag Colours (to be used with 'mHighestFlagColour')
    {
        FLAG_COLOUR_NONE = 0,               // Not used for actual flags, only for some query functions
        FLAG_COLOUR_GREEN,                  // End of danger zone, or race started
        FLAG_COLOUR_BLUE,                   // Faster car wants to overtake the participant
        FLAG_COLOUR_WHITE_SLOW_CAR,         // Slow car in area
        FLAG_COLOUR_WHITE_FINAL_LAP,        // Final Lap
        FLAG_COLOUR_RED,                    // Huge collisions where one or more cars become wrecked and block the track
        FLAG_COLOUR_YELLOW,                 // Danger on the racing surface itself
        FLAG_COLOUR_DOUBLE_YELLOW,          // Danger that wholly or partly blocks the racing surface
        FLAG_COLOUR_BLACK_AND_WHITE,        // Unsportsmanlike conduct
        FLAG_COLOUR_BLACK_ORANGE_CIRCLE,    // Mechanical Failure
        FLAG_COLOUR_BLACK,                  // Participant disqualified
        FLAG_COLOUR_CHEQUERED,              // Chequered flag
        FLAG_COLOUR_MAX
    }

    public enum RaceFlagReason // (Type#6) Flag Reason (to be used with 'mHighestFlagReason')
    {
        FLAG_REASON_NONE = 0,
        FLAG_REASON_SOLO_CRASH,
        FLAG_REASON_VEHICLE_CRASH,
        FLAG_REASON_VEHICLE_OBSTRUCTION,
        FLAG_REASON_MAX
    }

    public enum PitMode // (Type#7) Pit Mode (to be used with 'mPitMode')
    {
        PIT_MODE_NONE = 0,
        PIT_MODE_DRIVING_INTO_PITS,
        PIT_MODE_IN_PIT,
        PIT_MODE_DRIVING_OUT_OF_PITS,
        PIT_MODE_IN_GARAGE,
        PIT_MODE_DRIVING_OUT_OF_GARAGE,
        PIT_MODE_MAX
    }

    public enum PitStopSchedule // (Type#8) Pit Stop Schedule (to be used with 'mPitSchedule')
    {
        PIT_SCHEDULE_NONE = 0,              // Nothing scheduled
        PIT_SCHEDULE_PLAYER_REQUESTED,      // Used for standard pit sequence - requested by player
        PIT_SCHEDULE_ENGINEER_REQUESTED,    // Used for standard pit sequence - requested by engineer
        PIT_SCHEDULE_DAMAGE_REQUESTED,      // Used for standard pit sequence - requested by engineer for damage
        PIT_SCHEDULE_MANDATORY,             // Used for standard pit sequence - requested by engineer from career enforced lap number
        PIT_SCHEDULE_DRIVE_THROUGH,         // Used for drive-through penalty
        PIT_SCHEDULE_STOP_GO,               // Used for stop-go penalty
        PIT_SCHEDULE_PITSPOT_OCCUPIED,      // Used for drive-through when pitspot is occupied
        PIT_SCHEDULE_MAX
    }

    public enum CarFlags // (Type#9) Car Flags (to be used with 'mCarFlags')
    {
        CAR_HEADLIGHT = 1 << 0,
        CAR_ENGINE_ACTIVE = 1 << 1,
        CAR_ENGINE_WARNING = 1 << 2,
        CAR_SPEED_LIMITER = 1 << 3,
        CAR_ABS = 1 << 4,
        CAR_HANDBRAKE = 1 << 5,
        CAR_TCS = 1 << 6,
        CAR_SCS = 1 << 7
    }

    public enum TyreFlag // (Type#10) Tyre Flags (to be used with 'mTyreFlags')
    {
        TYRE_ATTACHED = 1 << 0,
        TYRE_INFLATED = 1 << 1,
        TYRE_IS_ON_GROUND = 1 << 2
    }

    public enum TerrainMaterials // (Type#11) Terrain Materials (to be used with 'mTerrain')
    {
        TERRAIN_ROAD = 0,
        TERRAIN_LOW_GRIP_ROAD,
        TERRAIN_BUMPY_ROAD1,
        TERRAIN_BUMPY_ROAD2,
        TERRAIN_BUMPY_ROAD3,
        TERRAIN_MARBLES,
        TERRAIN_GRASSY_BERMS,
        TERRAIN_GRASS,
        TERRAIN_GRAVEL,
        TERRAIN_BUMPY_GRAVEL,
        TERRAIN_RUMBLE_STRIPS,
        TERRAIN_DRAINS,
        TERRAIN_TYREWALLS,
        TERRAIN_CEMENTWALLS,
        TERRAIN_GUARDRAILS,
        TERRAIN_SAND,
        TERRAIN_BUMPY_SAND,
        TERRAIN_DIRT,
        TERRAIN_BUMPY_DIRT,
        TERRAIN_DIRT_ROAD,
        TERRAIN_BUMPY_DIRT_ROAD,
        TERRAIN_PAVEMENT,
        TERRAIN_DIRT_BANK,
        TERRAIN_WOOD,
        TERRAIN_DRY_VERGE,
        TERRAIN_EXIT_RUMBLE_STRIPS,
        TERRAIN_GRASSCRETE,
        TERRAIN_LONG_GRASS,
        TERRAIN_SLOPE_GRASS,
        TERRAIN_COBBLES,
        TERRAIN_SAND_ROAD,
        TERRAIN_BAKED_CLAY,
        TERRAIN_ASTROTURF,
        TERRAIN_SNOWHALF,
        TERRAIN_SNOWFULL,
        TERRAIN_DAMAGED_ROAD1,
        TERRAIN_TRAIN_TRACK_ROAD,
        TERRAIN_BUMPYCOBBLES,
        TERRAIN_ARIES_ONLY,
        TERRAIN_ORION_ONLY,
        TERRAIN_B1RUMBLES,
        TERRAIN_B2RUMBLES,
        TERRAIN_ROUGH_SAND_MEDIUM,
        TERRAIN_ROUGH_SAND_HEAVY,
        TERRAIN_SNOWWALLS,
        TERRAIN_ICE_ROAD,
        TERRAIN_RUNOFF_ROAD,
        TERRAIN_ILLEGAL_STRIP,
        TERRAIN_PAINT_CONCRETE,
        TERRAIN_PAINT_CONCRETE_ILLEGAL,
        TERRAIN_RALLY_TARMAC,
        TERRAIN_MAX
    }

    public enum CrashDamageState // (Type#12) Crash Damage State  (to be used with 'mCrashState')
    {
        CRASH_DAMAGE_NONE = 0,
        CRASH_DAMAGE_OFFTRACK,
        CRASH_DAMAGE_LARGE_PROP,
        CRASH_DAMAGE_SPINNING,
        CRASH_DAMAGE_ROLLING,
        CRASH_MAX
    }

    public enum DrsState // (Type#14) DrsState Flags (to be used with 'mDrsState')
    {
        DRS_INSTALLED = 1 << 0,         // Vehicle has DRS capability
        DRS_ZONE_RULES = 1 << 1,        // 1 if DRS uses F1 style rules
        DRS_AVAILABLE_NEXT = 1 << 2,    // detection zone was triggered (only applies to f1 style rules)
        DRS_AVAILABLE_NOW = 1 << 3,     // detection zone was triggered and we are now in the zone (only applies to f1 style rules)
        DRS_ACTIVE = 1 << 4             // Wing is in activated state
    }

    public enum ErsDeploymentMode // (Type#15) ErsDeploymentMode (to be used with 'mErsDeploymentMode')
    {
        ERS_DEPLOYMENT_MODE_NONE = 0,   // The vehicle does not support deployment modes
        ERS_DEPLOYMENT_MODE_OFF,        // Regen only, no deployment
        ERS_DEPLOYMENT_MODE_BUILD,      // Heavy emphasis towards regen
        ERS_DEPLOYMENT_MODE_BALANCED,   // Deployment map automatically adjusted to try and maintain target SoC
        ERS_DEPLOYMENT_MODE_ATTACK,     // More aggressive deployment, no target SoC
        ERS_DEPLOYMENT_MODE_QUAL        // Maximum deployment, no target Soc
    }

    public enum YellowFlagState // (Type#16) YellowFlagState represents current FCY state (to be used with 'mYellowFlagState')
    {
        YFS_INVALID = -1,
        YFS_NONE,           // No yellow flag pending on track
        YFS_PENDING,        // Flag has been thrown, but not yet taken by leader
        YFS_PITS_CLOSED,    // Flag taken by leader, pits not yet open
        YFS_PIT_LEAD_LAP,   // Those on the lead lap may pit
        YFS_PITS_OPEN,      // Everyone may pit
        YFS_PITS_OPEN2,     // Everyone may pit
        YFS_LAST_LAP,       // On the last caution lap
        YFS_RESUME,         // About to restart (pace car will duck out)
        YFS_RACE_HALT,      // Safety car will lead field into pits
        YFS_MAXIMUM
    }

    public enum LaunchStage
    {
        LAUNCH_INVALID = -1,    // Launch control unavailable
        LAUNCH_OFF = 0,         // Launch control inactive
        LAUNCH_REV,             // Launch control revving to optimum engine speed
        LAUNCH_ON               // Launch control actively accelerating vehicle
    }
}
