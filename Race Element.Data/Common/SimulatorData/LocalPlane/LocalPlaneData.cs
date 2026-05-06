using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace RaceElement.Data.Common.SimulatorData.LocalPlane;

public sealed record LocalPlaneData
{
    public double Latitude { get; internal set; }
    public double Longitude { get; internal set; }


    public Vector3 Orientation { get; internal set; }

    public double YawAngle { get; internal set; }

    public double PitchAngle { get; internal set; }

    public double RollAngle { get; internal set; }


    public double AltitudeFeet { get; internal set; }

    public double AirSpeed { get; internal set; }
    public double GroundSpeed { get; internal set; }
}
