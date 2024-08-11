using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.iRacing.SDK;
using Riok.Mapperly.Abstractions;

namespace RaceElement.Data.Games.iRacing.DataMapper
{
    
    [Mapper]
    internal static partial class IRacingLocalCarMapper
    {
        private static IRacingSdkDatum carIdxLapDistPctDatum;
        static bool hasDatums = false;
        
        //[MapProperty(nameof(Telemetry.PlayerCarPosition), nameof(@LocalCarData.RacePosition.GlobalPosition))]
        //[MapProperty(nameof(Telemetry.Speed), nameof(@LocalCarData.Physics.Velocity))]
        //public static partial void WithTelemetry(Telemetry telemetry, LocalCarData localCarData);

        //private static partial void AddSessionData(iRacingSDK.SessionData sessionData, LocalCarData localCarData);

        // TODO: Use Mappers or direct update? This code is not used right now.
        public static void WithSessionData(IRSDKSharper iRacingSDK, IRacingSdkSessionInfo sessionData, LocalCarData localCarData)
        {
            // get datums to do faster access after initialization
            if (!hasDatums) {
                carIdxLapDistPctDatum = iRacingSDK.Data.TelemetryDataProperties[ "CarIdxLapDistPct" ];

                hasDatums = true;
            }

            // and then now you can repeatedly call this for the most blisteringly fastest speed possible
            var lapDistPct = iRacingSDK.Data.GetFloat( carIdxLapDistPctDatum, 5 );
            
            // localCarData. = 
            var x = (int)sessionData.DriverInfo.Drivers[sessionData.DriverInfo.DriverCarIdx].CarNumberRaw;
        }
    }
}
