using System.Numerics;

namespace RaceElement.Data.Common.SimulatorData.LocalPlane;

public sealed record LocalPlaneData
{
    public GeneralPlaneData General { get; internal set; } = new();

    public PhysicsData Physics { get; internal set; } = new();

    public List<EngineData> Engines { get; internal set; } = [];

    public HelicopterData Helicopter { get; internal set; } = new();


    /// <summary>
    /// Provides Air traffic control data.
    /// </summary>
    public AtcData ATC { get; internal set; } = new();

    public FlightModelData FlightModel { get; internal set; } = new();
}

/// <summary>
/// Contains data about the flight model of the aviation device.
/// </summary>
public sealed record FlightModelData()
{
    public GeneralModelData General { get; internal set; } = new();
    public WeightModelData Weight { get; internal set; } = new();
    public CenterOfGravityModelData CenterOfGravity { get; internal set; } = new();

    public sealed record GeneralModelData
    {
        public double CurrentGForce { get; internal set; }
        public double MinGForceAttained { get; internal set; }
        public double MaxGForceAttained { get; internal set; }
    }

    public sealed record CenterOfGravityModelData
    {

    }

    public sealed record WeightModelData
    {

    }
}

public sealed record GeneralPlaneData
{
    /// <summary>
    /// The amount of engines.
    /// </summary>
    public uint EngineCount { get; internal set; }
}

/// <summary>
/// Air traffic control data (ATC)
/// </summary>
public sealed record AtcData
{
    /// <summary>
    /// ID used by ATC
    /// </summary>
    public string Identifier { get; internal set; }

    /// <summary>
    /// Model used by ATC
    /// </summary>
    public string Model { get; internal set; }

    /// <summary>
    /// ATC Type
    /// </summary>
    public string Type { get; internal set; }

    /// <summary>
    /// Suggested minimum runway length for takeoff.
    /// </summary>
    public double SuggestedMinimumRunwayTakeoffLength { get; internal set; }

    /// <summary>
    /// Suggested minimum runway length for landing.
    /// </summary>
    public double SuggestedMinimumRunwayLandingLength { get; internal set; }

    public string AirportName { get; internal set; }
}

public sealed record HelicopterData
{
    /// <summary>
    /// 0-100
    /// </summary>
    public double CollectivePosition { get; internal set; }
}

public sealed record EngineData
{
    /// <summary>
    /// 0 based.
    /// </summary>
    public uint EngineIndex { get; internal set; }

    /// <summary>
    /// Revolutions per Minute
    /// </summary>
    public double Rpm { get; internal set; }

    public double MaxRatedEngineRpm { get; internal set; }

    public double MaxReachedEngineRpm { get; internal set; }

    /// <summary>
    /// 0 - 100
    /// </summary>
    public double ThrottlePosition { get; internal set; }


    /// <summary>
    /// Whether the engine is running.
    /// </summary>
    public bool IsRunning { get; internal set; }


    public string EngineType { get; internal set; }
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
    /// Altitude in Feet, above sea level.
    /// </summary>
    public double AltitudeSea { get; internal set; }

    public double AltitudeGround { get; internal set; }

    /// <summary>
    /// Knots.
    /// </summary>
    public double IndicatedAirSpeed { get; internal set; }


    public double GroundSpeed { get; internal set; }
}


