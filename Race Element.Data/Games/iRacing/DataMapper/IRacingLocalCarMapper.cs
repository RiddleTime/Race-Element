using RaceElement.Data.Common.SimulatorData;
using Riok.Mapperly.Abstractions;

namespace RaceElement.Data.Games.iRacing.DataMapper
{
    [Mapper]
    internal static partial class IRacingLocalCarMapper
    {
        //[MapProperty(nameof(Telemetry.PlayerCarPosition), nameof(@LocalCarData.RacePosition.GlobalPosition))]
        //[MapProperty(nameof(Telemetry.Speed), nameof(@LocalCarData.Physics.Velocity))]
        //public static partial void WithTelemetry(Telemetry telemetry, LocalCarData localCarData);

        //private static partial void AddSessionData(iRacingSDK.SessionData sessionData, LocalCarData localCarData);

        //public static void WithSessionData(iRacingSDK.SessionData sessionData, LocalCarData localCarData)
        //{
        //    localCarData.RacePosition.CarNumber = (int)sessionData.DriverInfo.Drivers[sessionData.DriverInfo.DriverCarIdx].CarNumberRaw;
        //}
    }
}
