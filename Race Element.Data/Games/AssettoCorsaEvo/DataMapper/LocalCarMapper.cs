using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Common.SimulatorData;
using static RaceElement.Data.Games.AssettoCorsaEvo.SharedMemory.AcEvoSharedMemory;
using static RaceElement.Data.Common.SimulatorData.LocalCar.ElectronicsData;

namespace RaceElement.Data.Games.AssettoCorsaEvo.DataMapper;

internal static class LocalCarMapper
{
    internal static void AddPhysics(ref SPageFilePhysicsEvo pagePhysics, ref LocalCarData commonData, ref SessionData sessionData)
    {
        commonData.Physics.Acceleration = new(pagePhysics.AccG[0], pagePhysics.AccG[1], pagePhysics.AccG[2]);
        commonData.Physics.Velocity = pagePhysics.SpeedKmh;

        commonData.Engine.IsPitLimiterOn = pagePhysics.PitLimiterOn == 1;
        commonData.Engine.MaxRpm = pagePhysics.CurrentMaxRpm;
        commonData.Engine.Rpm = pagePhysics.Rpms;

        commonData.Engine.IsRunning = pagePhysics.IgnitionOn == 1;

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
        commonData.Tyres.Wear = [pagePhysics.TyreWear[0] * 100.000f, pagePhysics.TyreWear[1] * 100.000f, pagePhysics.TyreWear[2] * 100.000f, pagePhysics.TyreWear[3] * 100.000f];

        commonData.Brakes.DiscTemperature = pagePhysics.BrakeTemp;

        commonData.Electronics.TractionControlLevel = (int)pagePhysics.Tc;
        commonData.Electronics.TractionControlActivation = pagePhysics.TcinAction;
        commonData.Electronics.AbsLevel = (int)pagePhysics.Abs;
        commonData.Electronics.AbsActivation = pagePhysics.AbsInAction;

        commonData.Engine.FuelLiters = pagePhysics.Fuel;

        sessionData.Weather.AirTemperature = pagePhysics.AirTemp;
        sessionData.Track.Temperature = pagePhysics.RoadTemp;
    }

    internal static void AddGraphics(ref SPageFileGraphicEvo pageGraphics, ref LocalCarData commonData, ref SessionData sessionData, ref GameData gameData)
    {
        commonData.Brakes.Pressure =
        [
            pageGraphics.TyreLf.BrakePressure,
            pageGraphics.TyreRf.BrakePressure,
            pageGraphics.TyreLr.BrakePressure,
            pageGraphics.TyreRr.BrakePressure
        ];

        commonData.Electronics.Blinkers = (pageGraphics.Instrumentation.DirectionLightLeft ? BlinkerStatus.Left : BlinkerStatus.None) | (pageGraphics.Instrumentation.DirectionLightRight ? BlinkerStatus.Right : BlinkerStatus.None);
        if (pageGraphics.Instrumentation.WarningLights)
            commonData.Electronics.Blinkers = BlinkerStatus.Left | BlinkerStatus.Right;


        if (pageGraphics.Status == AcEvoStatus.AcPause)
            gameData.IsGamePaused = true;
        else gameData.IsGamePaused = false;
    }
}
