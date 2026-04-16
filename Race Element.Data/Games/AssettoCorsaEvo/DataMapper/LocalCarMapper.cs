using RaceElement.Data.Common.SimulatorData.LocalCar;
using Riok.Mapperly.Abstractions;
using System.Numerics;
using RaceElement.Data.Common.SimulatorData;
using static RaceElement.Data.Games.AssettoCorsaEvo.SharedMemory.AcEvoSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaEvo.DataMapper;

[Mapper]
internal static partial class LocalCarMapper
{
    internal static void AddPhysics(ref SPageFilePhysicsEvo pagePhysics, ref LocalCarData commonData, ref SessionData sessionData)
    {
        commonData.Physics.Acceleration = new(pagePhysics.AccG[0], pagePhysics.AccG[1], pagePhysics.AccG[2]);
        commonData.Physics.Velocity = pagePhysics.SpeedKmh;

        commonData.Engine.IsPitLimiterOn = pagePhysics.PitLimiterOn == 1;
        commonData.Engine.MaxRpm = pagePhysics.CurrentMaxRpm;
        commonData.Engine.Rpm = pagePhysics.Rpms;

        commonData.Engine.IsRunning = pagePhysics.IsEngineRunning == 1;

        commonData.Inputs.Steering = pagePhysics.SteerAngle;
        commonData.Inputs.Clutch = 1 - pagePhysics.Clutch;
        commonData.Inputs.Throttle = pagePhysics.Gas;
        commonData.Inputs.Brake = pagePhysics.Brake;
        commonData.Inputs.Gear = pagePhysics.Gear;

        commonData.Tyres.CoreTemperature = pagePhysics.TyreCoreTemperature;
        commonData.Tyres.Pressure = pagePhysics.WheelsPressure;
        commonData.Tyres.SlipRatio = pagePhysics.WheelSlip;
        commonData.Tyres.SlipAngle = pagePhysics.SlipAngle;
        commonData.Tyres.Velocity = pagePhysics.Velocity;

        commonData.Brakes.DiscTemperature = pagePhysics.BrakeTemp;

        commonData.Electronics.TractionControlLevel = (int)pagePhysics.Tc;
        commonData.Electronics.AbsLevel = (int)pagePhysics.Abs;
        commonData.Engine.FuelLiters = pagePhysics.Fuel;

        ///
        sessionData.Weather.AirTemperature = pagePhysics.AirTemp;
    }

    internal static void AddGraphics(ref SPageFileGraphicEvo pageGraphics, ref LocalCarData commonData, ref SessionData sessionData)
    {
        commonData.Brakes.Pressure = new float[]
        {
            pageGraphics.TyreLf.BrakePressure,
            pageGraphics.TyreRf.BrakePressure,
            pageGraphics.TyreLr.BrakePressure,
            pageGraphics.TyreRr.BrakePressure
        };


        commonData.Engine.IsRunning = commonData.Engine.Rpm > 0 && !pageGraphics.IsInPitBox;
    }
}
