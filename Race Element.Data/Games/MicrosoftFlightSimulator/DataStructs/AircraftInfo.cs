using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RaceElement.Data.Games.MicrosoftFlightSimulator.DataStructs;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AircraftInfo
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public String Title;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public String Category;
    //[SimVar(NameId = FsSimVar.PlaneLatitude, UnitId = FsUnit.Radian)]
    //public double Latitude;
    //[SimVar(NameId = FsSimVar.PlaneLongitude, UnitId = FsUnit.Radian)]
    //public double Longitude;
    //[SimVar(NameId = FsSimVar.PlaneAltitudeAboveGround, UnitId = FsUnit.Feet)]
    //public double AltitudeAboveGround;
    //[SimVar(NameId = FsSimVar.PlaneAltitude, UnitId = FsUnit.Feet)]
    //public double Altitude;
    //[SimVar(NameId = FsSimVar.PlaneHeadingDegreesTrue, UnitId = FsUnit.Degree)]
    //public double Heading;
    //[SimVar(NameId = FsSimVar.AirspeedTrue, UnitId = FsUnit.Knot)]
    //public double Speed;
};
