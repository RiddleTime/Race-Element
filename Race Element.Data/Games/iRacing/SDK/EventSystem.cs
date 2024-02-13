using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games.iRacing.SDK
{
    public class EventSystem
    {
        private const int BufferSize = 8 * 1024;

        private readonly string directory;

        private int subSessionID = -1;
        private StreamWriter? streamWriter = null;
        private double sessionTime = 0;
        private readonly EventTracks eventTracks = new();
        private readonly StringBuilder stringBuilder = new(BufferSize);

        public EventSystem(string directory)
        {
            this.directory = directory;
        }

        public void Update(IRacingSdkData data)
        {
            if (subSessionID != data.SessionInfo.WeekendInfo.SubSessionID)
            {
                Reset(data);

                subSessionID = data.SessionInfo.WeekendInfo.SubSessionID;

                Directory.CreateDirectory(directory);

                var filePath = Path.Combine(directory, $"subses{subSessionID}.yaml");

                if (data.SessionInfo.WeekendInfo.SimMode == "replay")
                {
                    LoadEvents(filePath);
                }
                else
                {
                    streamWriter = new StreamWriter(filePath, true, Encoding.UTF8, BufferSize);
                }
            }
        }

        public void Reset(IRacingSdkData? data = null)
        {
            subSessionID = -1;

            streamWriter?.Close();
            streamWriter = null;

            sessionTime = 0;

            if (data != null)
            {
                eventTracks.Reset(data);
            }
        }

        public void Record(IRacingSdkData data)
        {
            if (streamWriter != null)
            {
                var sessionNum = data.GetInt("SessionNum");
                var sessionTime = data.GetDouble("SessionTime");
                var sessionTick = data.GetInt("SessionTick");

                eventTracks.Update(sessionNum, sessionTime, sessionTick, stringBuilder, data);

                if (stringBuilder.Length > 0)
                {
                    streamWriter.WriteLine("---");
                    streamWriter.WriteLine($" SessionNum: {sessionNum}");
                    streamWriter.WriteLine($" SessionTime: {sessionTime:0.0000}");
                    streamWriter.Write(stringBuilder);
                    streamWriter.WriteLine("...");

                    stringBuilder.Clear();
                }

                if ((sessionTime < this.sessionTime) || ((this.sessionTime + 5) <= sessionTime))
                {
                    this.sessionTime = sessionTime;

                    streamWriter.Flush();
                }
            }
        }

        private void LoadEvents(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"Warning - Event system file '{filePath}' does not exist.");

                return;
            }

            // TODO
        }

        public class EventTracks
        {
            public EventTrack<uint> SessionFlags { get; } = new("SessionFlags", 0, "SessionFlags");
            public EventTrack<int> SessionLapsTotal { get; } = new("SessionLapsTotal", 0, "SessionLapsTotal");
            public EventTrack<int> PaceMode { get; } = new("PaceMode", (int)IRacingSdkEnum.PaceMode.SingleFileStart, "PaceMode");
            public EventTrack<int> CarLeftRight { get; } = new("CarLeftRight", (int)IRacingSdkEnum.CarLeftRight.Off, "CarLeftRight");
            public EventTrack<float> FuelLevel { get; } = new("FuelLevel", 0, "FuelLevel", 120);

            public EventTrack<uint>[] CarIdxSessionFlags { get; } = new EventTrack<uint>[IRacingSdkConst.MaxNumCars];
            public EventTrack<int>[] CarIdxPosition { get; } = new EventTrack<int>[IRacingSdkConst.MaxNumCars];
            public EventTrack<int>[] CarIdxClassPosition { get; } = new EventTrack<int>[IRacingSdkConst.MaxNumCars];
            public EventTrack<int>[] CarIdxPaceLine { get; } = new EventTrack<int>[IRacingSdkConst.MaxNumCars];
            public EventTrack<int>[] CarIdxPaceRow { get; } = new EventTrack<int>[IRacingSdkConst.MaxNumCars];
            public EventTrack<uint>[] CarIdxPaceFlags { get; } = new EventTrack<uint>[IRacingSdkConst.MaxNumCars];

            public EventTrack<int>[] CurDriverIncidentCount { get; } = new EventTrack<int>[IRacingSdkConst.MaxNumCars];
            public EventTrack<int>[] TeamIncidentCount { get; } = new EventTrack<int>[IRacingSdkConst.MaxNumCars];

            public EventTrack<int>[] QualifyPosition { get; } = new EventTrack<int>[IRacingSdkConst.MaxNumCars];
            public EventTrack<int>[] QualifyClassPosition { get; } = new EventTrack<int>[IRacingSdkConst.MaxNumCars];
            public EventTrack<int>[] QualifyFastestLap { get; } = new EventTrack<int>[IRacingSdkConst.MaxNumCars];
            public EventTrack<float>[] QualifyFastestTime { get; } = new EventTrack<float>[IRacingSdkConst.MaxNumCars];

            public EventTracks()
            {
                for (var i = 0; i < IRacingSdkConst.MaxNumCars; i++)
                {
                    CarIdxSessionFlags[i] = new($"CarIdxSessionFlags.{i}", 0, "CarIdxSessionFlags");
                    CarIdxPosition[i] = new($"CarIdxPosition.{i}", 0, "CarIdxPosition");
                    CarIdxClassPosition[i] = new($"CarIdxClassPosition.{i}", 0, "CarIdxClassPosition");
                    CarIdxPaceLine[i] = new($"CarIdxPaceLine.{i}", -1, "CarIdxPaceLine");
                    CarIdxPaceRow[i] = new($"CarIdxPaceRow.{i}", -1, "CarIdxPaceRow");
                    CarIdxPaceFlags[i] = new($"CarIdxPaceFlags.{i}", 0, "CarIdxPaceFlags");

                    CurDriverIncidentCount[i] = new($"CurDriverIncidentCount.{i}", -1);
                    TeamIncidentCount[i] = new($"TeamIncidentCount.{i}", -1);

                    QualifyPosition[i] = new($"QualifyPosition.{i}", -1);
                    QualifyClassPosition[i] = new($"QualifyClassPosition.{i}", -1);
                    QualifyFastestLap[i] = new($"QualifyFastestLap.{i}", -1);
                    QualifyFastestTime[i] = new($"QualifyFastestTime.{i}", 0.0f);
                }
            }

            public void Reset(IRacingSdkData data)
            {
                SessionFlags.Reset(data);
                SessionLapsTotal.Reset(data);
                PaceMode.Reset(data);
                CarLeftRight.Reset(data);
                FuelLevel.Reset(data);

                for (var i = 0; i < IRacingSdkConst.MaxNumCars; i++)
                {
                    CarIdxSessionFlags[i].Reset(data);
                    CarIdxPosition[i].Reset(data);
                    CarIdxClassPosition[i].Reset(data);
                    CarIdxPaceLine[i].Reset(data);
                    CarIdxPaceRow[i].Reset(data);
                    CarIdxPaceFlags[i].Reset(data);

                    CurDriverIncidentCount[i].Reset(data);
                    TeamIncidentCount[i].Reset(data);

                    QualifyPosition[i].Reset(data);
                    QualifyClassPosition[i].Reset(data);
                    QualifyFastestLap[i].Reset(data);
                    QualifyFastestTime[i].Reset(data);
                }
            }

            public void Update(int sessionNum, double sessionTime, int sessionTick, StringBuilder stringBuilder, IRacingSdkData data)
            {
#pragma warning disable CS8604
                var sessionFlags = data.GetBitField(SessionFlags.Datum);
                var sessionLapsTotal = data.GetInt(SessionLapsTotal.Datum);
                var paceMode = data.GetInt(PaceMode.Datum);
                var carLeftRight = data.GetInt(CarLeftRight.Datum);
                var fuelLevel = data.GetFloat(FuelLevel.Datum);
#pragma warning restore CS8604

                SessionFlags.Update(sessionNum, sessionTime, sessionTick, stringBuilder, sessionFlags);
                SessionLapsTotal.Update(sessionNum, sessionTime, sessionTick, stringBuilder, sessionLapsTotal);
                PaceMode.Update(sessionNum, sessionTime, sessionTick, stringBuilder, paceMode);
                CarLeftRight.Update(sessionNum, sessionTime, sessionTick, stringBuilder, carLeftRight);
                FuelLevel.Update(sessionNum, sessionTime, sessionTick, stringBuilder, fuelLevel);

                uint[] carIdxSessionFlags = new uint[IRacingSdkConst.MaxNumCars];
                int[] carIdxPaceLine = new int[IRacingSdkConst.MaxNumCars];
                int[] carIdxPaceRow = new int[IRacingSdkConst.MaxNumCars];
                uint[] carIdxPaceFlags = new uint[IRacingSdkConst.MaxNumCars];

#pragma warning disable CS8604
                data.GetBitFieldArray(CarIdxSessionFlags[0].Datum, carIdxSessionFlags, 0, IRacingSdkConst.MaxNumCars);
                data.GetIntArray(CarIdxPaceLine[0].Datum, carIdxPaceLine, 0, IRacingSdkConst.MaxNumCars);
                data.GetIntArray(CarIdxPaceRow[0].Datum, carIdxPaceRow, 0, IRacingSdkConst.MaxNumCars);
                data.GetBitFieldArray(CarIdxPaceFlags[0].Datum, carIdxPaceFlags, 0, IRacingSdkConst.MaxNumCars);
#pragma warning restore CS8604

                for (var i = 0; i < IRacingSdkConst.MaxNumCars; i++)
                {
                    CarIdxSessionFlags[i].Update(sessionNum, sessionTime, sessionTick, stringBuilder, carIdxSessionFlags[i]);
                    // TODO Update CarIdxPosition
                    // TODO Update CarIdxClassPosition
                    CarIdxPaceLine[i].Update(sessionNum, sessionTime, sessionTick, stringBuilder, carIdxPaceLine[i]);
                    CarIdxPaceRow[i].Update(sessionNum, sessionTime, sessionTick, stringBuilder, carIdxPaceRow[i]);
                    CarIdxPaceFlags[i].Update(sessionNum, sessionTime, sessionTick, stringBuilder, carIdxPaceFlags[i]);
                }

                var sessionInfo = data.SessionInfo;

                foreach (var driver in sessionInfo.DriverInfo.Drivers)
                {
                    var carIdx = driver.CarIdx;

                    if (carIdx != -1)
                    {
                        CurDriverIncidentCount[carIdx].Update(sessionNum, sessionTime, sessionTick, stringBuilder, driver.CurDriverIncidentCount);
                        TeamIncidentCount[carIdx].Update(sessionNum, sessionTime, sessionTick, stringBuilder, driver.TeamIncidentCount);
                    }
                }

                var qualifyPositions = sessionInfo.SessionInfo.Sessions[sessionNum].QualifyPositions;

                if (qualifyPositions != null)
                {
                    foreach (var qualifyPosition in qualifyPositions)
                    {
                        var carIdx = qualifyPosition.CarIdx;

                        QualifyPosition[carIdx].Update(sessionNum, sessionTime, sessionTick, stringBuilder, qualifyPosition.Position);
                        QualifyClassPosition[carIdx].Update(sessionNum, sessionTime, sessionTick, stringBuilder, qualifyPosition.ClassPosition);
                        QualifyFastestLap[carIdx].Update(sessionNum, sessionTime, sessionTick, stringBuilder, qualifyPosition.FastestLap);
                        QualifyFastestTime[carIdx].Update(sessionNum, sessionTime, sessionTick, stringBuilder, qualifyPosition.FastestTime);
                    }
                }
            }

            public class EventTrack<T> where T : IEquatable<T>
            {
                private string TrackName;

                private T DefaultValue;
                private T RetainedValue;
                private T CurrentValue;

                private int SessionTickMask;

                private string? DatumName;
                public IRacingSdkDatum? Datum { get; private set; }

                private List<Event> events = new();

                public EventTrack(string trackName, T defaultValue, string? datumName = null, int sessionTickMask = 1)
                {
                    this.TrackName = trackName;
                    this.DefaultValue = defaultValue;
                    this.SessionTickMask = sessionTickMask;

                    this.DatumName = datumName;
                    this.Datum = null;

                    RetainedValue = defaultValue;
                    CurrentValue = defaultValue;
                }

                public void Reset(IRacingSdkData data)
                {
                    RetainedValue = DefaultValue;
                    CurrentValue = DefaultValue;

                    Datum = null;

                    if (DatumName != null)
                    {
                        Datum = data.TelemetryDataProperties[DatumName];
                    }

                    events.Clear();
                }

                public void Update(int sessionNum, double sessionTime, int sessionTick, StringBuilder stringBuilder, T value)
                {
                    if ((sessionTick % SessionTickMask) == 0)
                    {
                        if (!RetainedValue.Equals(value))
                        {
                            stringBuilder.AppendLine($" {TrackName}: {value}");

                            events.Add(new Event(sessionNum, sessionTime, value));

                            RetainedValue = value;
                        }
                    }
                }

                public class Event
                {
                    public readonly int sessionNum;
                    public readonly double sessionTime;
                    public readonly T value;

                    public Event(int sessionNum, double sessionTime, T value)
                    {
                        this.sessionNum = sessionNum;
                        this.sessionTime = sessionTime;
                        this.value = value;
                    }
                }
            }
        }
    }
}
