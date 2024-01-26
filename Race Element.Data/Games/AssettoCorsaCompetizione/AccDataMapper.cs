using RaceElement.Data.Common;
using Riok.Mapperly.Abstractions;
using static RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory.ACCSharedMemory;

namespace RaceElement.Data.AssettoCorsaCompetizione
{
    [Mapper]
    public static partial class AccCommonCarDataMapper
    {
        // Engine data
        [MapProperty(nameof(SPageFilePhysics.Rpms), $"{nameof(CommonLocalCarData.Engine)}.{nameof(CommonEngineData.RPM)}")]
        // Inputs Data
        [MapProperty(nameof(SPageFilePhysics.Gas), $"{nameof(CommonLocalCarData.Inputs)}.{nameof(CommonInputsData.Throttle)}")]
        [MapProperty(nameof(SPageFilePhysics.Brake), $"{nameof(CommonLocalCarData.Inputs)}.{nameof(CommonInputsData.Brake)}")]
        [MapProperty(nameof(SPageFilePhysics.Clutch), $"{nameof(CommonLocalCarData.Inputs)}.{nameof(CommonInputsData.Clutch)}")]
        [MapProperty(nameof(SPageFilePhysics.Gear), $"{nameof(CommonLocalCarData.Inputs)}.{nameof(CommonInputsData.Gear)}")]
        [MapProperty(nameof(SPageFilePhysics.SteerAngle), $"{nameof(CommonLocalCarData.Inputs)}.{nameof(CommonInputsData.Steering)}")]
        // Physics data
        [MapProperty(nameof(SPageFilePhysics.SpeedKmh), $"{nameof(CommonLocalCarData.Physics)}.{nameof(CommonPhysicsData.Velocity)}")]

        // Tyre Data
        [MapProperty(nameof(SPageFilePhysics.TyreTemp), $"{nameof(CommonLocalCarData.Tyres)}.{nameof(CommonWheelData.CoreTemperature)}")]
        [MapProperty(nameof(SPageFilePhysics.WheelPressure), $"{nameof(CommonLocalCarData.Tyres)}.{nameof(CommonWheelData.Pressure)}")]
        [MapProperty(nameof(SPageFilePhysics.Velocity), $"{nameof(CommonLocalCarData.Tyres)}.{nameof(CommonWheelData.Velocity)}")]
        [MapProperty(nameof(SPageFilePhysics.SlipAngle), $"{nameof(CommonLocalCarData.Tyres)}.{nameof(CommonWheelData.SlipAngle)}")]

        // Brakes Data
        [MapProperty(nameof(SPageFilePhysics.BrakeTemperature), $"{nameof(CommonLocalCarData.BrakesData)}.{nameof(CommonBrakesData.DiscTemperature)}")]
        [MapProperty(nameof(SPageFilePhysics.BrakePressure), $"{nameof(CommonLocalCarData.BrakesData)}.{nameof(CommonBrakesData.Pressure)}")]

        // Electronics activation
        [MapProperty(nameof(SPageFilePhysics.TC), $"{nameof(CommonLocalCarData.Electronics)}.{nameof(CommonElectronicsData.TractionControlActivation)}")]
        [MapProperty(nameof(SPageFilePhysics.Abs), $"{nameof(CommonLocalCarData.Electronics)}.{nameof(CommonElectronicsData.AbsActivation)}")]
        private static partial void WithAccPhysics(SPageFilePhysics physicsData, CommonLocalCarData commonData);

        public static CommonLocalCarData AddSharedPhysicsPage(SPageFilePhysics physicsData, CommonLocalCarData commonData)
        {
            WithAccPhysics(physicsData, commonData);

            commonData.Physics.Acceleration = new(physicsData.AccG[0], physicsData.AccG[1], physicsData.AccG[2]);

            return commonData;
        }

        // Electronics Data
        [MapProperty(nameof(SPageFileGraphic.TC), $"{nameof(CommonLocalCarData.Electronics)}.{nameof(CommonElectronicsData.TractionControlLevel)}")]
        [MapProperty(nameof(SPageFileGraphic.TCCut), $"{nameof(CommonLocalCarData.Electronics)}.{nameof(CommonElectronicsData.TractionControlCutLevel)}")]
        [MapProperty(nameof(SPageFileGraphic.ABS), $"{nameof(CommonLocalCarData.Electronics)}.{nameof(CommonElectronicsData.AbsLevel)}")]
        public static partial void WithSharedGraphicsPage(SPageFileGraphic graphicsData, CommonLocalCarData commonData);

        // Engine Data
        [MapProperty(nameof(SPageFileStatic.MaxRpm), $"{nameof(CommonLocalCarData.Engine)}.{nameof(CommonEngineData.MaxRPM)}")]
        // Model Data
        [MapProperty(nameof(SPageFileStatic.CarModel), $"{nameof(CommonLocalCarData.Model)}.{nameof(CommonModelData.GameName)}")]
        public static partial void WithSharedStaticPage(SPageFileStatic staticData, CommonLocalCarData commonData);
    }
}
