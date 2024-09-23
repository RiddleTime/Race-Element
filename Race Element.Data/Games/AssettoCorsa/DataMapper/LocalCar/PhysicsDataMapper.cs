using RaceElement.Data.Common.SimulatorData.LocalCar;
using Riok.Mapperly.Abstractions;
using System.Numerics;
using static RaceElement.Data.Games.AssettoCorsa.SharedMemory.AcSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsa.DataMapper.LocalCar;

[Mapper]
internal static partial class PhysicsDataMapper
{
    /// <summary>
    /// Populates LocalCarData.Physics
    /// </summary>
    /// <param name="physics"></param>
    /// <param name="localCar"></param>
    internal static void InsertPhysicsPage(ref PageFilePhysics pagePhysics, PhysicsData commonData)
    {
        AddPhysicsPage(pagePhysics, commonData);
        commonData.Acceleration = new(pagePhysics.AccG[0], pagePhysics.AccG[2], pagePhysics.AccG[1]);
        commonData.Rotation = Quaternion.CreateFromYawPitchRoll(pagePhysics.Heading, pagePhysics.Pitch, pagePhysics.Roll);
    }

    [MapProperty(nameof(PageFilePhysics.SpeedKmh), nameof(PhysicsData.Velocity))]
    private static partial void AddPhysicsPage(PageFilePhysics physics, PhysicsData localCar);

}
