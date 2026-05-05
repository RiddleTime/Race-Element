using RaceElement.Graph.Edge;
using RaceElement.Graph.Node;

namespace RaceElement.Data.Common.Graph;


/// <summary>
/// Child of <see cref="DriverNode"/> with <see cref="OwnsEdge"/>.
/// </summary>
public sealed record class LapDataNode : AbstractNode
{
    /// <summary>
    /// Lap index (starting from 0). -1 means invalid and not set.
    /// </summary>
    public required int LapIndex { get; init; } = -1;

    /// <summary>
    /// The duration of the lap in milliseconds.
    /// -1 if invalid.
    /// </summary>
    public int LapTimeMs { get; init; } = -1;

    /// <summary>
    /// Sector Split times in milliseconds. Empty array if none exist.
    /// </summary>
    public int[] SectorTimesMs { get; init; } = [];


    /// <summary>
    /// Whether the lap is valid, true by default.
    /// </summary>
    public bool IsValid { get; init; } = true;
}

/// <summary>
/// Child of <see cref="CarNode"/> with <see cref="OwnsEdge" />.
/// </summary>
public sealed record class DriverNode : AbstractNode
{
    /// <summary>
    /// Can be driver index or specific number if assigned.
    /// Should be used to identify the driver within the game.
    /// -1 is invalid.
    /// </summary>
    public required int DriverId { get; init; } = -1;
    public string Name { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

public sealed record class CarNode : AbstractNode
{
    /// <summary>
    /// Car number.
    /// </summary>
    public required int CarNumber { get; init; }

    /// <summary>
    /// The Car Model specified by the game.
    /// Empty string if not specified.
    /// </summary>
    public string CarModelGameID { get; init; } = string.Empty;

    /// <summary>
    /// Race Position
    /// </summary>
    public int Position { get; set; } = -1;

    public TrackStates TrackState = TrackStates.None;

    /// <summary>
    /// The amount of completed laps
    /// </summary>
    public int Laps { get; set; } = -1;
}

/// <summary>
/// Describes a <see cref="TrackStates"/> change for a <see cref="CarNode"/>
/// </summary>
public sealed record class TrackStateEdge : AbstractEdge
{
    /// <summary>
    /// Specifies the track state
    /// </summary>
    public TrackStates State { get; init; } = TrackStates.None;
}

[Flags]
public enum TrackStates : uint
{
    None = 1 << 0,
    PitLaneIn = 1 << 1,
    PitLaneOut = 1 << 2,
    Pitlane = 1 << 3,
    Track = 1 << 4,
    OffTrack = 1 << 5,
}
