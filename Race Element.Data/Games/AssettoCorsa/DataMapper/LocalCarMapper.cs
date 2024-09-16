using RaceElement.Data.Common.SimulatorData;
using Riok.Mapperly.Abstractions;
using System.Numerics;
using static RaceElement.Data.Games.AssettoCorsa.SharedMemory.AcSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsa.DataMapper;

[Mapper]
internal static partial class LocalCarMapper
{

    // -- Engine data
    [MapProperty(nameof(PageFilePhysics.Rpms), nameof(@LocalCarData.Engine.Rpm))]
    // -- Inputs Data
    [MapProperty(nameof(PageFilePhysics.Gas), nameof(@LocalCarData.Inputs.Throttle))]
    [MapProperty(nameof(PageFilePhysics.Brake), nameof(@LocalCarData.Inputs.Brake))]
    [MapProperty(nameof(PageFilePhysics.Clutch), nameof(@LocalCarData.Inputs.Clutch))]
    [MapProperty(nameof(PageFilePhysics.Gear), nameof(@LocalCarData.Inputs.Gear))]
    [MapProperty(nameof(PageFilePhysics.SteerAngle), nameof(@LocalCarData.Inputs.Steering))]
    // -- Tyre Data
    [MapProperty(nameof(PageFilePhysics.TyreCoreTemperature), nameof(@LocalCarData.Tyres.CoreTemperature))]
    [MapProperty(nameof(PageFilePhysics.WheelPressure), nameof(@LocalCarData.Tyres.Pressure))]
    [MapProperty(nameof(PageFilePhysics.Velocity), nameof(@LocalCarData.Tyres.Velocity))]
    [MapProperty(nameof(PageFilePhysics.WheelSlip), nameof(@LocalCarData.Tyres.SlipRatio))]
    // -- Brakes Data
    [MapProperty(nameof(PageFilePhysics.BrakeTemperature), nameof(@LocalCarData.Brakes.DiscTemperature))]
    // -- Electronics activation
    [MapProperty(nameof(PageFilePhysics.TC), nameof(@LocalCarData.Electronics.TractionControlActivation))]
    [MapProperty(nameof(PageFilePhysics.Abs), nameof(@LocalCarData.Electronics.AbsActivation))]
    [MapProperty(nameof(PageFilePhysics.Fuel), nameof(@LocalCarData.Engine.FuelLiters))]
    private static partial void WithPhysicsPage(PageFilePhysics pagePhysics, LocalCarData commonData);

    internal static void AddPhysics(ref PageFilePhysics pagePhysics, ref LocalCarData commonData)
    {
        commonData.Physics.Acceleration = new(pagePhysics.AccG[0], pagePhysics.AccG[2], pagePhysics.AccG[1]);

        WithPhysicsPage(pagePhysics, commonData);
    }

    // Electronics Data
    [MapProperty(nameof(PageFileGraphics.Position), nameof(@LocalCarData.Race.GlobalPosition))]
    private static partial void WithGraphicsPage(PageFileGraphics pageGraphics, LocalCarData commonData);

    internal static void AddGraphics(PageFileGraphics pageGraphics, LocalCarData commonData)
    {
        var coords = pageGraphics.CarCoordinates[pageGraphics.PlayerCarID];
        commonData.Physics.Location = new Vector3(coords.X * 10f, coords.Y, coords.Z);
        WithGraphicsPage(pageGraphics, commonData);
    }

    // Engine Data
    [MapProperty(nameof(PageFileStatic.MaxRpm), nameof(@LocalCarData.Engine.MaxRpm))]
    [MapProperty(nameof(PageFileStatic.MaxFuel), nameof(@LocalCarData.Engine.MaxFuelLiters))]
    // Model Data
    [MapProperty(nameof(PageFileStatic.CarModel), nameof(@LocalCarData.CarModel.GameName))]
    internal static partial void WithStaticPage(PageFileStatic pageStatic, LocalCarData commonData);
}
