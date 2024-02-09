using iRacingSDK;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.iRacing.DataMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games.iRacing
{
    internal class IRacingDataProvider
    {
        internal static void Update(ref LocalCarData localCar, ref Common.SimulatorData.SessionData sessionData, ref GameData gameData)
        {
            var iRacing = new iRacingConnection();

            var data = iRacing.GetDataFeed().First();

            IRacingLocalCarMapper.WithTelemetry(data.Telemetry, localCar);
            IRacingLocalCarMapper.WithSessionData(data.SessionData, localCar);
        }
    }
}
