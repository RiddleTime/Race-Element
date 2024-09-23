using RaceElement.Data.Common.SimulatorData.LocalCar;
using Riok.Mapperly.Abstractions;
using System.Numerics;
using static RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory.AccSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaCompetizione.DataMapper;

[Mapper]
internal static partial class LocalCarMapper
{
    // -- Engine data
    [MapProperty(nameof(PageFilePhysics.Rpms), nameof(@LocalCarData.Engine.Rpm))]
    [MapProperty(nameof(PageFilePhysics.IsEngineRunning), nameof(@LocalCarData.Engine.IsRunning))]
    [MapProperty(nameof(PageFilePhysics.IgnitionOn), nameof(@LocalCarData.Engine.IsIgnitionOn))]
    [MapProperty(nameof(PageFilePhysics.PitLimiterOn), nameof(@LocalCarData.Engine.IsPitLimiterOn))]
    // -- Inputs Data
    [MapProperty(nameof(PageFilePhysics.Gas), nameof(@LocalCarData.Inputs.Throttle))]
    [MapProperty(nameof(PageFilePhysics.Brake), nameof(@LocalCarData.Inputs.Brake))]
    [MapProperty(nameof(PageFilePhysics.Clutch), nameof(@LocalCarData.Inputs.Clutch))]
    [MapProperty(nameof(PageFilePhysics.Gear), nameof(@LocalCarData.Inputs.Gear))]
    [MapProperty(nameof(PageFilePhysics.SteerAngle), nameof(@LocalCarData.Inputs.Steering))]
    // -- Physics data
    [MapProperty(nameof(PageFilePhysics.SpeedKmh), nameof(@LocalCarData.Physics.Velocity))]
    // -- Tyre Data
    [MapProperty(nameof(PageFilePhysics.TyreTemp), nameof(@LocalCarData.Tyres.CoreTemperature))]
    [MapProperty(nameof(PageFilePhysics.WheelPressure), nameof(@LocalCarData.Tyres.Pressure))]
    [MapProperty(nameof(PageFilePhysics.Velocity), nameof(@LocalCarData.Tyres.Velocity))]
    [MapProperty(nameof(PageFilePhysics.SlipAngle), nameof(@LocalCarData.Tyres.SlipAngle))]
    // -- Brakes Data
    [MapProperty(nameof(PageFilePhysics.BrakeTemperature), nameof(@LocalCarData.Brakes.DiscTemperature))]
    [MapProperty(nameof(PageFilePhysics.BrakePressure), nameof(@LocalCarData.Brakes.Pressure))]
    // -- Electronics activation
    [MapProperty(nameof(PageFilePhysics.TC), nameof(@LocalCarData.Electronics.TractionControlActivation))]
    [MapProperty(nameof(PageFilePhysics.Abs), nameof(@LocalCarData.Electronics.AbsActivation))]
    private static partial void AddAccPhysics(PageFilePhysics physicsData, LocalCarData commonData);

    internal static void WithPhysicsPage(PageFilePhysics physicsData, LocalCarData commonData)
    {
        AddAccPhysics(physicsData, commonData);

        commonData.Physics.Acceleration = new(physicsData.AccG[0], physicsData.AccG[2], physicsData.AccG[1]);
        commonData.Physics.Rotation = Quaternion.CreateFromYawPitchRoll(physicsData.Heading, physicsData.Pitch, physicsData.Roll);
    }

    // Electronics Data
    [MapProperty(nameof(PageFileGraphics.TC), nameof(@LocalCarData.Electronics.TractionControlLevel))]
    [MapProperty(nameof(PageFileGraphics.TCCut), nameof(@LocalCarData.Electronics.TractionControlCutLevel))]
    [MapProperty(nameof(PageFileGraphics.ABS), nameof(@LocalCarData.Electronics.AbsLevel))]
    [MapProperty(nameof(PageFileGraphics.Position), nameof(@LocalCarData.Race.GlobalPosition))]
    private static partial void WithGraphicsPage(PageFileGraphics pageGraphics, LocalCarData commonData);

    internal static void AddAccGraphics(PageFileGraphics pageGraphics, LocalCarData commonData)
    {

        WithGraphicsPage(pageGraphics, commonData);
        int playerArrayIndex = Array.IndexOf(pageGraphics.CarIds, pageGraphics.PlayerCarID);
        if (playerArrayIndex != -1)
            commonData.Physics.Location = pageGraphics.CarCoordinates[playerArrayIndex];
    }
    // Engine Data
    [MapProperty(nameof(PageFileStatic.MaxRpm), nameof(@LocalCarData.Engine.MaxRpm))]
    // Model Data
    [MapProperty(nameof(PageFileStatic.CarModel), nameof(@LocalCarData.CarModel.GameName))]
    internal static partial void WithStaticPage(PageFileStatic pageStatic, LocalCarData commonData);
}
