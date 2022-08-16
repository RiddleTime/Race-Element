using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Database.Telemetry
{
    public static class DbLapTelemetryExtensions
    {
        public static Dictionary<long, TelemetryPoint> DeserializeLapData(this DbLapTelemetry dbLapTelemetry)
        {
            Dictionary<long, TelemetryPoint> Data = new Dictionary<long, TelemetryPoint>();

            using (var ms = new MemoryStream(dbLapTelemetry.LapData))
            {
                var formatter = new BinaryFormatter();
                Data = (Dictionary<long, TelemetryPoint>)formatter.Deserialize(ms);
                ms.Close();
            }

            return Data;
        }

        public static byte[] SerializeLapData(this Dictionary<long, TelemetryPoint> dbLapTelemetry)
        {
            byte[] Data;

            using (var ms = new MemoryStream())
            {
                var formatter =
                   new BinaryFormatter();

                formatter.Serialize(ms, dbLapTelemetry);
                Data = ms.ToArray();
                ms.Close();
            }

            return Data;
        }
    }
}
