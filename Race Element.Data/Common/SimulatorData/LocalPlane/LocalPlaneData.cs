using System.Numerics;

namespace RaceElement.Data.Common.SimulatorData.LocalPlane;

public sealed record LocalPlaneData
{
    public PhysicsData Physics { get; internal set; } = new();
}

public sealed record PhysicsData
{
    public double Latitude { get; internal set; }
    public double Longitude { get; internal set; }

    /// <summary>
    /// Z-forward, Y-up, X-right - Standard right-handed system.
    /// </summary>
    public Vector3 Orientation { get; internal set; }

    /// <summary>
    /// Feet per Minute.
    /// </summary>
    public double VerticalSpeed { get; internal set; }

    /// <summary>
    /// Altitude above sea level.
    /// </summary>
    public double AltitudeFeet { get; internal set; }

    /// <summary>
    /// Knots.
    /// </summary>
    public double IndicatedAirSpeed { get; internal set; }


    public double GroundSpeed { get; internal set; }
}
