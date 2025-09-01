using RaceElement.Data.Common.SimulatorData.LocalCar;
using Riok.Mapperly.Abstractions;
using System.Numerics;
using RaceElement.Data.Common.SimulatorData;
using static RaceElement.Data.Games.AssettoCorsaEvo.SharedMemory.AcEvoSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaEvo.DataMapper;

[Mapper]
internal static partial class LocalCarMapper
{
    internal static void AddPhysics(ref SPageFilePhysics pagePhysics, ref LocalCarData commonData, ref SessionData sessionData)
    {
        commonData.Physics.Acceleration = new(pagePhysics.AccG[0], pagePhysics.AccG[1], pagePhysics.AccG[2]);
        commonData.Engine.IsPitLimiterOn = pagePhysics.PitLimiterOn;
        commonData.Engine.MaxRpm = pagePhysics.CurrentMaxRpm;
        commonData.Engine.Rpm = pagePhysics.Rpms;

        commonData.Engine.IsRunning = commonData.Engine.Rpm > 0;

        commonData.Inputs.Steering = pagePhysics.SteerAngle;
        commonData.Inputs.Clutch = 1 - pagePhysics.Clutch;
        commonData.Inputs.Throttle = pagePhysics.Gas;
        commonData.Inputs.Brake = pagePhysics.Brake;
        commonData.Inputs.Gear = pagePhysics.Gear;

        commonData.Tyres.CoreTemperature = pagePhysics.TyreCoreTemperature;
        commonData.Tyres.Pressure = pagePhysics.WheelPressure;
        commonData.Tyres.SlipRatio = pagePhysics.WheelSlip;
        commonData.Tyres.Velocity = pagePhysics.Velocity;

        commonData.Brakes.DiscTemperature = pagePhysics.BrakeTemperature;
        commonData.Brakes.Pressure = pagePhysics.brakePressure;

        commonData.Electronics.TractionControlLevel = (int)pagePhysics.TC;
        commonData.Electronics.AbsLevel = (int)pagePhysics.Abs;
        commonData.Engine.FuelLiters = pagePhysics.Fuel;

        ///
        sessionData.Weather.AirTemperature = pagePhysics.AirTemp;
    }

    internal static void AddGraphics(ref SPageFileGraphic pageGraphics, ref LocalCarData commonData, ref SessionData sessionData)
    {
        if (pageGraphics.CarCoordinates.Length >= pageGraphics.PlayerCarID)
        {
            var coords = pageGraphics.CarCoordinates[pageGraphics.PlayerCarID];
            commonData.Physics.Location = new Vector3(coords.X * 10f, coords.Y, coords.Z);
        }

        switch (pageGraphics.SessionType)
        {
            case AcSessionType.AC_HOTLAPSUPERPOLE: sessionData.SessionType = RaceSessionType.HotlapSuperpole; break;
            case AcSessionType.AC_QUALIFY: sessionData.SessionType = RaceSessionType.Qualifying; break;
            case AcSessionType.AC_HOTSTINT: sessionData.SessionType = RaceSessionType.Hotstint; break;
            case AcSessionType.AC_PRACTICE: sessionData.SessionType = RaceSessionType.Practice; break;
            case AcSessionType.AC_HOTLAP: sessionData.SessionType = RaceSessionType.Hotlap; break;
            case AcSessionType.AC_RACE: sessionData.SessionType = RaceSessionType.Race; break;
            default: sessionData.SessionType = RaceSessionType.Unknown; break;
        }
    }
}
