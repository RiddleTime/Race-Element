using ACCManager.Data.ACC.Database.SessionData;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Database.Telemetry
{
    public class DbLapTelemetry
    {
        public Guid Id { get; set; }
        public Guid LapId { get; set; }
        public int Herz { get; set; }
        public byte[] LapData { get; set; }
    }

    [Serializable]
    public class TelemetryPoint
    {
        public float SplinePosition { get; set; }
        public InputsData InputsData { get; set; }
        public TyreData TyreData { get; set; }
        public BrakeData BrakeData { get; set; }
    }

    [Serializable]
    public class InputsData
    {
        public float Gas { get; set; }
        public float Brake { get; set; }
        public float SteerAngle { get; set; }
        public int Gear { get; set; }
    }

    [Serializable]
    public class TyreData
    {
        public float[] TyreCoreTemperature { get; set; }
        public float[] TyrePressure { get; set; }
    }

    [Serializable]
    public class BrakeData
    {
        public float[] BrakeTemperature { get; set; }
    }

    public class LapTelemetryCollection
    {
        private static ILiteCollection<DbLapTelemetry> Collection
        {
            get
            {
                ILiteCollection<DbLapTelemetry> _collection = RaceWeekendDatabase.Database.GetCollection<DbLapTelemetry>();

                return _collection;
            }
        }

        private static ILiteCollection<DbLapTelemetry> GetCollection(ILiteDatabase db)
        {
            return db.GetCollection<DbLapTelemetry>();
        }

        public static DbLapTelemetry GetForLap(ILiteCollection<DbLapTelemetry> collection, Guid lapId)
        {
            return collection.FindOne(x => x.LapId == lapId);
        }
    }
}
