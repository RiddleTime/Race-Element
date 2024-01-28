using RaceElement.Data.Common.SimulatorData;
using Riok.Mapperly.Abstractions;
using System.Numerics;
using static RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory.AccSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaCompetizione.DataMapper;

[Mapper]
internal static partial class LocalCarMapper
{
    // -- Engine data
    [MapProperty(nameof(PageFilePhysics.Rpms), $"{nameof(LocalCarData.Engine)}.{nameof(EngineData.RPM)}")]
    [MapProperty(nameof(PageFilePhysics.IsEngineRunning), $"{nameof(LocalCarData.Engine)}.{nameof(EngineData.IsRunning)}")]
    [MapProperty(nameof(PageFilePhysics.IgnitionOn), $"{nameof(LocalCarData.Engine)}.{nameof(EngineData.IsIgnitionOn)}")]
    // -- Inputs Data
    [MapProperty(nameof(PageFilePhysics.Gas), $"{nameof(LocalCarData.Inputs)}.{nameof(InputsData.Throttle)}")]
    [MapProperty(nameof(PageFilePhysics.Brake), $"{nameof(LocalCarData.Inputs)}.{nameof(InputsData.Brake)}")]
    [MapProperty(nameof(PageFilePhysics.Clutch), $"{nameof(LocalCarData.Inputs)}.{nameof(InputsData.Clutch)}")]
    [MapProperty(nameof(PageFilePhysics.Gear), $"{nameof(LocalCarData.Inputs)}.{nameof(InputsData.Gear)}")]
    [MapProperty(nameof(PageFilePhysics.SteerAngle), $"{nameof(LocalCarData.Inputs)}.{nameof(InputsData.Steering)}")]
    // -- Physics data
    [MapProperty(nameof(PageFilePhysics.SpeedKmh), $"{nameof(LocalCarData.Physics)}.{nameof(PhysicsData.Velocity)}")]
    // -- Tyre Data
    [MapProperty(nameof(PageFilePhysics.TyreTemp), $"{nameof(LocalCarData.Tyres)}.{nameof(TyresData.CoreTemperature)}")]
    [MapProperty(nameof(PageFilePhysics.WheelPressure), $"{nameof(LocalCarData.Tyres)}.{nameof(TyresData.Pressure)}")]
    [MapProperty(nameof(PageFilePhysics.Velocity), $"{nameof(LocalCarData.Tyres)}.{nameof(TyresData.Velocity)}")]
    [MapProperty(nameof(PageFilePhysics.SlipAngle), $"{nameof(LocalCarData.Tyres)}.{nameof(TyresData.SlipAngle)}")]
    // -- Brakes Data
    [MapProperty(nameof(PageFilePhysics.BrakeTemperature), $"{nameof(LocalCarData.Brakes)}.{nameof(BrakesData.DiscTemperature)}")]
    [MapProperty(nameof(PageFilePhysics.BrakePressure), $"{nameof(LocalCarData.Brakes)}.{nameof(BrakesData.Pressure)}")]
    // -- Electronics activation
    [MapProperty(nameof(PageFilePhysics.TC), $"{nameof(LocalCarData.Electronics)}.{nameof(ElectronicsData.TractionControlActivation)}")]
    [MapProperty(nameof(PageFilePhysics.Abs), $"{nameof(LocalCarData.Electronics)}.{nameof(ElectronicsData.AbsActivation)}")]
    private static partial void AddAccPhysics(PageFilePhysics physicsData, LocalCarData commonData);

    internal static void WithPhysicsPage(PageFilePhysics physicsData, LocalCarData commonData)
    {
        AddAccPhysics(physicsData, commonData);

        commonData.Physics.Acceleration = new(physicsData.AccG[0], physicsData.AccG[2], physicsData.AccG[1]);
        commonData.Rotations.Quaternion = Quaternion.CreateFromYawPitchRoll(physicsData.Heading, physicsData.Pitch, physicsData.Roll);
    }

    // Electronics Data
    [MapProperty(nameof(PageFileGraphics.TC), $"{nameof(LocalCarData.Electronics)}.{nameof(ElectronicsData.TractionControlLevel)}")]
    [MapProperty(nameof(PageFileGraphics.TCCut), $"{nameof(LocalCarData.Electronics)}.{nameof(ElectronicsData.TractionControlCutLevel)}")]
    [MapProperty(nameof(PageFileGraphics.ABS), $"{nameof(LocalCarData.Electronics)}.{nameof(ElectronicsData.AbsLevel)}")]
    internal static partial void WithGraphicsPage(PageFileGraphics pageGraphics, LocalCarData commonData);

    // Engine Data
    [MapProperty(nameof(PageFileStatic.MaxRpm), $"{nameof(LocalCarData.Engine)}.{nameof(EngineData.MaxRPM)}")]
    // Model Data
    [MapProperty(nameof(PageFileStatic.CarModel), $"{nameof(LocalCarData.CarModel)}.{nameof(CarModelData.GameName)}")]
    internal static partial void WithStaticPage(PageFileStatic pageStatic, LocalCarData commonData);
}
