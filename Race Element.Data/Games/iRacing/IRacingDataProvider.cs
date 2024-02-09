using iRacingSDK;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.iRacing.DataMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// https://github.com/vipoo/iRacingSDK.Net
namespace RaceElement.Data.Games.iRacing
{
    internal static class IRacingDataProvider
    {
        internal static void Update(ref LocalCarData localCar, ref Common.SimulatorData.SessionData sessionData, ref GameData gameData)
        {
            try
            {
                var iRacing = new iRacingConnection();
                var data = iRacing.GetDataFeed().First();

                IRacingLocalCarMapper.WithTelemetry(data.Telemetry, localCar);
                IRacingLocalCarMapper.WithSessionData(data.SessionData, localCar);
            }
            catch (Exception)
            {
                // atm not leaning on iRacing sdk exceptions
            }
        }
    }
}
