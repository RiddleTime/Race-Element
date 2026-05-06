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

    public Vector3 Orientation { get; internal set; }

    public double YawAngle { get; internal set; }

    public double PitchAngle { get; internal set; }

    public double RollAngle { get; internal set; }



    /// <summary>
    /// Feet per Minute
    /// </summary>
    public double VerticalSpeed { get; internal set; }

    public double AltitudeFeet { get; internal set; }

    public double IndicatedAirSpeed { get; internal set; }
    public double GroundSpeed { get; internal set; }
}
