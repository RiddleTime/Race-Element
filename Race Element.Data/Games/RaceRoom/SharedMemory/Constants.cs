namespace RaceElement.Data.Games.RaceRoom.SharedMemory;

// https://github.com/sector3studios/r3e-api
internal sealed class Constants
{
    public const string SharedMemoryName = "$R3E";

    enum VersionMajor
    {
        // Major version number to test against
        R3E_VERSION_MAJOR = 2
    };

    enum VersionMinor
    {
        // Minor version number to test against
        R3E_VERSION_MINOR = 15
    };

    enum Session
    {
        Unavailable = -1,
        Practice = 0,
        Qualify = 1,
        Race = 2,
        Warmup = 3,
    };

    enum SessionPhase
    {
        Unavailable = -1,

        // Currently in garage
        Garage = 1,

        // Gridwalk or track walkthrough
        Gridwalk = 2,

        // Formation lap, rolling start etc.
        Formation = 3,

        // Countdown to race is ongoing
        Countdown = 4,

        // Race is ongoing
        Green = 5,

        // End of session
        Checkered = 6,
    };

    enum Control
    {
        Unavailable = -1,

        // Controlled by the actual player
        Player = 0,

        // Controlled by AI
        AI = 1,

        // Controlled by a network entity of some sort
        Remote = 2,

        // Controlled by a replay or ghost
        Replay = 3,
    };

    enum PitWindow
    {
        Unavailable = -1,

        // Pit stops are not enabled for this session
        Disabled = 0,

        // Pit stops are enabled, but you're not allowed to perform one right now
        Closed = 1,

        // Allowed to perform a pit stop now
        Open = 2,

        // Currently performing the pit stop changes (changing driver, etc.)
        Stopped = 3,

        // After the current mandatory pitstop have been completed
        Completed = 4,
    };

    enum PitStopStatus
    {
        // No mandatory pitstops
        Unavailable = -1,

        // Mandatory pitstop for two tyres not served yet
        UnservedTwoTyres = 0,

        // Mandatory pitstop for four tyres not served yet
        UnservedFourTyres = 1,

        // Mandatory pitstop served
        Served = 2,
    };

    enum FinishStatus
    {
        // N/A
        Unavailable = -1,

        // Still on track, not finished
        None = 0,

        // Finished session normally
        Finished = 1,

        // Did not finish
        DNF = 2,

        // Did not qualify
        DNQ = 3,

        // Did not start
        DNS = 4,

        // Disqualified
        DQ = 5,
    };

    enum SessionLengthFormat
    {
        // N/A
        Unavailable = -1,

        TimeBased = 0,

        LapBased = 1,

        // Time and lap based session means there will be an extra lap after the time has run out
        TimeAndLapBased = 2
    };

    enum PitMenuSelection
    {
        // Pit menu unavailable
        Unavailable = -1,

        // Pit menu preset
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

        // Pit menu buttons
        ButtonTop = 9,
        ButtonBottom = 10,

        // Pit menu nothing selected
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


