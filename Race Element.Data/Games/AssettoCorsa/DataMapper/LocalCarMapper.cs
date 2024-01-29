using RaceElement.Data.Common.SimulatorData;
using Riok.Mapperly.Abstractions;
using System.Numerics;
using static RaceElement.Data.Games.AssettoCorsa.SharedMemory.AcSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsa.DataMapper;

[Mapper]
internal static partial class LocalCarMapper
{

    // -- Engine data
    [MapProperty(nameof(PageFilePhysics.Rpms), nameof(@LocalCarData.Engine.RPM))]
    // -- Inputs Data
    [MapProperty(nameof(PageFilePhysics.Gas), nameof(@LocalCarData.Inputs.Throttle))]
    [MapProperty(nameof(PageFilePhysics.Brake), nameof(@LocalCarData.Inputs.Brake))]
    [MapProperty(nameof(PageFilePhysics.Clutch), nameof(@LocalCarData.Inputs.Clutch))]
    [MapProperty(nameof(PageFilePhysics.Gear), nameof(@LocalCarData.Inputs.Gear))]
    [MapProperty(nameof(PageFilePhysics.SteerAngle), nameof(@LocalCarData.Inputs.Steering))]
    // -- Physics data
    [MapProperty(nameof(PageFilePhysics.SpeedKmh), nameof(@LocalCarData.Physics.Velocity))]
    // -- Tyre Data
    [MapProperty(nameof(PageFilePhysics.TyreCoreTemperature), nameof(@LocalCarData.Tyres.CoreTemperature))]
    [MapProperty(nameof(PageFilePhysics.WheelPressure), nameof(@LocalCarData.Tyres.Pressure))]
    [MapProperty(nameof(PageFilePhysics.Velocity), nameof(@LocalCarData.Tyres.Velocity))]
    // -- Brakes Data
    [MapProperty(nameof(PageFilePhysics.BrakeTemperature), nameof(@LocalCarData.Brakes.DiscTemperature))]
    // -- Electronics activation
    [MapProperty(nameof(PageFilePhysics.TC), nameof(@LocalCarData.Electronics.TractionControlActivation))]
    [MapProperty(nameof(PageFilePhysics.Abs), nameof(@LocalCarData.Electronics.AbsActivation))]
    private static partial void WithPhysicsPage(PageFilePhysics physicsData, LocalCarData commonData);

    internal static void AddPhysics(PageFilePhysics physicsData, LocalCarData commonData)
    {

        commonData.Physics.Acceleration = new(physicsData.AccG[0], physicsData.AccG[2], physicsData.AccG[1]);
        commonData.Physics.Rotation = Quaternion.CreateFromYawPitchRoll(physicsData.Heading, physicsData.Pitch, physicsData.Roll);

        WithPhysicsPage(physicsData, commonData);
    }

    // Electronics Data
    private static partial void WithGraphicsPage(PageFileGraphics pageGraphics, LocalCarData commonData);

    internal static void AddGraphics(PageFileGraphics pageGraphics, LocalCarData commonData)
    {
        var coords = pageGraphics.CarCoordinates[pageGraphics.PlayerCarID];
        commonData.Physics.Location = new Vector3(coords.X * 10f, coords.Y, coords.Z);

        WithGraphicsPage(pageGraphics, commonData);
    }

    // Engine Data
    [MapProperty(nameof(PageFileStatic.MaxRpm), nameof(@LocalCarData.Engine.MaxRPM))]
    // Model Data
    [MapProperty(nameof(PageFileStatic.CarModel), nameof(@LocalCarData.CarModel.GameName))]
    internal static partial void WithStaticPage(PageFileStatic pageStatic, LocalCarData commonData);
}
