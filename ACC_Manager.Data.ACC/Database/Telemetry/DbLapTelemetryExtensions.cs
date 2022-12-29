using RaceElement.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RaceElement.Data.ACC.Database.Telemetry
{
    public static class DbLapTelemetryExtensions
    {
        public static Dictionary<long, TelemetryPoint> DeserializeLapData(this DbLapTelemetry dbLapTelemetry)
        {
            Dictionary<long, TelemetryPoint> Data = new Dictionary<long, TelemetryPoint>();

            try
            {
                using (var ms = new MemoryStream(dbLapTelemetry.LapData))
                {
                    var formatter = new BinaryFormatter();
                    Data = (Dictionary<long, TelemetryPoint>)formatter.Deserialize(ms);
                    ms.Close();
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }

            return Data;
        }

        public static byte[] SerializeLapData(this Dictionary<long, TelemetryPoint> dbLapTelemetry)
        {
            byte[] Data = new byte[0];

            try
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();

                    formatter.Serialize(ms, dbLapTelemetry);
                    Data = ms.ToArray();
                    ms.Close();
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }

            return Data;
        }
    }
}
