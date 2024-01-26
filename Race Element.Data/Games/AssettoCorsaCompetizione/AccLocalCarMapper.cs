using RaceElement.Data.Common;
using Riok.Mapperly.Abstractions;
using static RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory.ACCSharedMemory;

namespace RaceElement.Data.AssettoCorsaCompetizione;

[Mapper]
public static partial class AccLocalCarMapper
{
    // -- Engine data
    [MapProperty(nameof(PageFilePhysics.Rpms), $"{nameof(CommonLocalCarData.Engine)}.{nameof(CommonEngineData.RPM)}")]
    // -- Inputs Data
    [MapProperty(nameof(PageFilePhysics.Gas), $"{nameof(CommonLocalCarData.Inputs)}.{nameof(CommonInputsData.Throttle)}")]
    [MapProperty(nameof(PageFilePhysics.Brake), $"{nameof(CommonLocalCarData.Inputs)}.{nameof(CommonInputsData.Brake)}")]
    [MapProperty(nameof(PageFilePhysics.Clutch), $"{nameof(CommonLocalCarData.Inputs)}.{nameof(CommonInputsData.Clutch)}")]
    [MapProperty(nameof(PageFilePhysics.Gear), $"{nameof(CommonLocalCarData.Inputs)}.{nameof(CommonInputsData.Gear)}")]
    [MapProperty(nameof(PageFilePhysics.SteerAngle), $"{nameof(CommonLocalCarData.Inputs)}.{nameof(CommonInputsData.Steering)}")]
    // -- Physics data
    [MapProperty(nameof(PageFilePhysics.SpeedKmh), $"{nameof(CommonLocalCarData.Physics)}.{nameof(CommonPhysicsData.Velocity)}")]
    // -- Tyre Data
    [MapProperty(nameof(PageFilePhysics.TyreTemp), $"{nameof(CommonLocalCarData.Tyres)}.{nameof(CommonWheelData.CoreTemperature)}")]
    [MapProperty(nameof(PageFilePhysics.WheelPressure), $"{nameof(CommonLocalCarData.Tyres)}.{nameof(CommonWheelData.Pressure)}")]
    [MapProperty(nameof(PageFilePhysics.Velocity), $"{nameof(CommonLocalCarData.Tyres)}.{nameof(CommonWheelData.Velocity)}")]
    [MapProperty(nameof(PageFilePhysics.SlipAngle), $"{nameof(CommonLocalCarData.Tyres)}.{nameof(CommonWheelData.SlipAngle)}")]
    // -- Brakes Data
    [MapProperty(nameof(PageFilePhysics.BrakeTemperature), $"{nameof(CommonLocalCarData.BrakesData)}.{nameof(CommonBrakesData.DiscTemperature)}")]
    [MapProperty(nameof(PageFilePhysics.BrakePressure), $"{nameof(CommonLocalCarData.BrakesData)}.{nameof(CommonBrakesData.Pressure)}")]
    // -- Electronics activation
    [MapProperty(nameof(PageFilePhysics.TC), $"{nameof(CommonLocalCarData.Electronics)}.{nameof(CommonElectronicsData.TractionControlActivation)}")]
    [MapProperty(nameof(PageFilePhysics.Abs), $"{nameof(CommonLocalCarData.Electronics)}.{nameof(CommonElectronicsData.AbsActivation)}")]
    private static partial void WithAccPhysics(PageFilePhysics physicsData, CommonLocalCarData commonData);

    public static CommonLocalCarData AddSharedPhysicsPage(PageFilePhysics physicsData, CommonLocalCarData commonData)
    {
        WithAccPhysics(physicsData, commonData);

        commonData.Physics.Acceleration = new(physicsData.AccG[0], physicsData.AccG[1], physicsData.AccG[2]);

        return commonData;
    }

    // Electronics Data
    [MapProperty(nameof(PageFileGraphic.TC), $"{nameof(CommonLocalCarData.Electronics)}.{nameof(CommonElectronicsData.TractionControlLevel)}")]
    [MapProperty(nameof(PageFileGraphic.TCCut), $"{nameof(CommonLocalCarData.Electronics)}.{nameof(CommonElectronicsData.TractionControlCutLevel)}")]
    [MapProperty(nameof(PageFileGraphic.ABS), $"{nameof(CommonLocalCarData.Electronics)}.{nameof(CommonElectronicsData.AbsLevel)}")]
    public static partial void WithSharedGraphicsPage(PageFileGraphic graphicsData, CommonLocalCarData commonData);

    // Engine Data
    [MapProperty(nameof(PageFileStatic.MaxRpm), $"{nameof(CommonLocalCarData.Engine)}.{nameof(CommonEngineData.MaxRPM)}")]
    // Model Data
    [MapProperty(nameof(PageFileStatic.CarModel), $"{nameof(CommonLocalCarData.Model)}.{nameof(CommonModelData.GameName)}")]
    public static partial void WithSharedStaticPage(PageFileStatic staticData, CommonLocalCarData commonData);
}
