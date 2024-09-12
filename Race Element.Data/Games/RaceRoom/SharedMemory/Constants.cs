namespace RaceElement.Data.Games.RaceRoom.SharedMemory;

// https://github.com/sector3studios/r3e-api
internal sealed class Constants
{
    public const string SharedMemoryName = "$R3E";

    enum VersionMajor
    {
        /// <summary>
        /// Major version number to test against
        /// </summary>
        R3E_VERSION_MAJOR = 2
    };

    enum VersionMinor
    {
        /// <summary>
        /// Minor version number to test against
        /// </summary>
        R3E_VERSION_MINOR = 16
    };

    /// <summary>
    /// Session Type
    /// </summary>
    enum Session
    {
        Unavailable = -1,
        Practice = 0,
        Qualify = 1,
        Race = 2,
        Warmup = 3,
    };


    /// <summary>
    /// Phase of the session.
    /// </summary>
    enum SessionPhase
    {
        Unavailable = -1,

        /// <summary>
        /// Currently in garage
        /// </summary>
        Garage = 1,

        /// <summary>
        /// Gridwalk or track walkthrough
        /// </summary>
        Gridwalk = 2,

        /// <summary>
        /// Formation lap, rolling start etc.
        /// </summary>
        Formation = 3,

        /// <summary>
        /// Countdown to race is ongoing
        /// </summary>
        Countdown = 4,

        /// <summary>
        /// Race is ongoing
        /// </summary>
        Green = 5,

        /// <summary>
        /// End of session
        /// </summary>
        Checkered = 6,
    };

    enum Control
    {
        Unavailable = -1,

        /// <summary>
        /// Controlled by the actual player
        /// </summary>
        Player = 0,

        /// <summary>
        /// Controlled by AI
        /// </summary>
        AI = 1,

        /// <summary>
        /// Controlled by a network entity of some sort
        /// </summary>
        Remote = 2,

        /// <summary>
        /// Controlled by a replay or ghost
        /// </summary>
        Replay = 3,
    };

    enum PitWindow
    {
        Unavailable = -1,

        /// <summary>
        /// Pit stops are not enabled for this session
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Pit stops are enabled, but you're not allowed to perform one right now
        /// </summary>
        Closed = 1,

        /// <summary>
        /// Allowed to perform a pit stop now
        /// </summary>
        Open = 2,

        /// <summary>
        /// Currently performing the pit stop changes (changing driver, etc.)
        /// </summary>
        Stopped = 3,

        /// <summary>
        /// After the current mandatory pitstop have been completed
        /// </summary>
        Completed = 4,
    };

    enum PitStopStatus
    {
        /// <summary>
        /// No mandatory pitstops
        /// </summary>
        Unavailable = -1,

        /// <summary>
        /// Mandatory pitstop for two tyres not served yet
        /// </summary>
        UnservedTwoTyres = 0,

        /// <summary>
        /// Mandatory pitstop for four tyres not served yet
        /// </summary>
        UnservedFourTyres = 1,

        /// <summary>
        /// Mandatory pitstop served
        /// </summary>
        Served = 2,
    };

    enum FinishStatus
    {
        /// <summary>
        /// N/A
        /// </summary>
        Unavailable = -1,

        /// <summary>
        /// Still on track, not finished
        /// </summary>
        None = 0,

        /// <summary>
        /// Finished session normally
        /// </summary>
        Finished = 1,

        /// <summary>
        /// Did not finish
        /// </summary>
        DNF = 2,

        /// <summary>
        /// Did not qualify
        /// </summary>
        DNQ = 3,

        /// <summary>
        /// Did not start
        /// </summary>
        DNS = 4,

        /// <summary>
        /// Disqualified
        /// </summary>
        DQ = 5,
    };

    enum SessionLengthFormat
    {
        /// <summary>
        /// N/A
        /// </summary>
        Unavailable = -1,

        TimeBased = 0,

        LapBased = 1,

        /// <summary>
        /// Time and lap based session means there will be an extra lap after the time has run out
        /// </summary>
        TimeAndLapBased = 2
    };

    enum PitMenuSelection
    {
        /// <summary>
        /// Pit menu unavailable
        /// </summary>
        Unavailable = -1,

        /// <summary>
        /// Pit menu preset
        /// </summary>
        Preset = 0,

        // Pit menu actions
        Penalty = 1,
        Driverchange = 2,
        Fuel = 3,
        Fronttires = 4,
        Reartires = 5,
        Frontwing = 6,
        Rearwing = 7,
        Suspension = 8,

        /// <summary>
        /// Pit menu buttons up
        /// </summary>
        ButtonTop = 9,
        /// <summary>
        /// Pit menu button down
        /// </summary>
        ButtonBottom = 10,

        /// <summary>
        /// Pit menu nothing selected
        /// </summary>
        Max = 11
    };

    enum TireType
    {
        Unavailable = -1,
        Option = 0,
        Prime = 1,
    };

    enum TireSubtype
    {
        Unavailable = -1,
        Primary = 0,
        Alternate = 1,
        Soft = 2,
        Medium = 3,
        Hard = 4
    };

    enum MtrlType
    {
        Unavailable = -1,
        None = 0,
        Tarmac = 1,
        Grass = 2,
        Dirt = 3,
        Gravel = 4,
        Rumble = 5
    };

    enum EngineType
    {
        COMBUSTION = 0,
        ELECTRIC = 1,
        HYBRID = 2,
    };
}


