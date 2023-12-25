using RaceElement.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RaceElement.Data.ACC.Database.Telemetry;

public static class DbLapTelemetryExtensions
{
    public static Dictionary<long, TelemetryPoint> DeserializeLapData(this DbLapTelemetry dbLapTelemetry)
    {
        Dictionary<long, TelemetryPoint> Data = new();

        try
        {
            using (var ms = new MemoryStream(dbLapTelemetry.LapData))
            {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                var formatter = new BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
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
        byte[] Data = [];

        try
        {
            using (var ms = new MemoryStream())
            {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                var formatter = new BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete

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
