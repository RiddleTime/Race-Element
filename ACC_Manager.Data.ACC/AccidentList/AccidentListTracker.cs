using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using ACCManager.Data.ACC.Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.Data.ACC.AccidentList
{
    public sealed class AccidentListTracker
    {
        private static AccidentListTracker _instance;
        private RealtimeUpdate _realtimeUpdate;
        private float _trackDistance;
        private DateTime _accidentTime = DateTime.MinValue;

        // History of realtime car updates
        // Maps session time to a map of carID and realTimeInfo
        private Dictionary<double, Dictionary<int, RealtimeCarUpdate>> _realTimeCarHistory = new Dictionary<double, Dictionary<int, RealtimeCarUpdate>>();

        private List<CarInfoWithRealTime> _unprocessedAccidents = new List<CarInfoWithRealTime>();
        //private Dictionary<double, CarInfoWithRealTime> _unprocessedAccidents = new Dictionary<double, CarInfoWithRealTime>();

        public static AccidentListTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new AccidentListTracker();
                return _instance;
            }
            private set { _instance = value; }
        }

        internal void Start()
        {
            BroadcastTracker.Instance.OnRealTimeCarUpdate += RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnRealTimeUpdate += RealTimeUpdate_EventHandler;
            BroadcastTracker.Instance.OnBroadcastEvent += BroadcastEvent_EventHandler;
            BroadcastTracker.Instance.OnTrackDataUpdate += TrackDataUpdate_EventHandler;
        }

        internal void Stop()
        {
            BroadcastTracker.Instance.OnRealTimeCarUpdate -= RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnRealTimeUpdate -= RealTimeUpdate_EventHandler;
            BroadcastTracker.Instance.OnBroadcastEvent -= BroadcastEvent_EventHandler;
            BroadcastTracker.Instance.OnTrackDataUpdate -= TrackDataUpdate_EventHandler;
        }

        private void BroadcastEvent_EventHandler(object sender, BroadcastingEvent broadcastingEvent)
        {
            switch (broadcastingEvent.Type)
            {
                case BroadcastingCarEventType.Accident:
                    {
                        HandleAccidentEvent(broadcastingEvent);
                        break;
                    }
            }
        }

        private void HandleAccidentEvent(BroadcastingEvent broadcastingEvent)
        {

            if (broadcastingEvent.CarData == null) return;

            Debug.WriteLine($"#{broadcastingEvent.CarData.RaceNumber}|{broadcastingEvent.TimeMs}| {broadcastingEvent.CarData.GetCurrentDriverName()} had an accident. {broadcastingEvent.Msg}");

            double key = GetSessionTimeKey();
            // accident events seems to be 5000 ms too late
            double accidentTime = key - 5000;

            //Debug.WriteLine($"session time {key} accident time {accidentTime}");
            if (!_realTimeCarHistory.ContainsKey(key))
            {
                Debug.WriteLine($"! no history data for key=({key})");
                return;
            }

            CarInfoWithRealTime carInfoWithRealTime = new CarInfoWithRealTime(broadcastingEvent.CarData);
            carInfoWithRealTime.RealtimeCarUpdate = _realTimeCarHistory[key][broadcastingEvent.CarId];
            lock(_unprocessedAccidents)
            {
                _unprocessedAccidents.Add(carInfoWithRealTime);
                Debug.WriteLine($"unprocessed accidents list size: {_unprocessedAccidents.Count}");
            }
            

            if (_accidentTime == DateTime.MinValue)
            {
                _accidentTime = DateTime.Now;
                Debug.WriteLine($"set new accident timestamp: {_accidentTime}");
            }
                

        }

        private void ProcessAccidentData()
        {
            DateTime processTime = DateTime.Now;
            // Debug.WriteLine($"process old accident data list size {processTime.Millisecond - _accidentTime.Millisecond} ...");
            // process accidents older than 1 sec.
            TimeSpan timediff = processTime - _accidentTime;
           //Debug.WriteLine($"process old accident data list size {(int)timediff.TotalMilliseconds} ...");
            if (_accidentTime != DateTime.MinValue && timediff.TotalMilliseconds > 1000)
            {
                Debug.WriteLine($"process old accident data list size {_unprocessedAccidents.Count} ...");
                
                if (_unprocessedAccidents.Count < 2)
                {
                    Debug.WriteLine($"accident: car {_unprocessedAccidents[0].RaceNumber}|{_unprocessedAccidents[0].GetCurrentDriverName()}");
                }

                for (int i=0; i<_unprocessedAccidents.Count-1; i++)
                {
                    float distance = (_unprocessedAccidents[i].RealtimeCarUpdate.SplinePosition - _unprocessedAccidents[i + 1].RealtimeCarUpdate.SplinePosition) * _trackDistance;
                    Debug.WriteLine($"accident: car {_unprocessedAccidents[i].RaceNumber} vs {_unprocessedAccidents[i+1].RaceNumber} distance {distance}");
                }

                lock(_unprocessedAccidents)
                {
                    Debug.WriteLine($"clear unprocessed accident list and reset accident timestamp");
                    _accidentTime = DateTime.MinValue;
                    _unprocessedAccidents.Clear();
                }
            }

           // double key = GetSessionTimeKey();

        }

        private void RealTimeCarUpdate_EventHandler(object sender, RealtimeCarUpdate realtimeCarUpdate)
        {
            AddCarUpdateToHistory(realtimeCarUpdate);
            RemoveOldHistoryData();
            ProcessAccidentData();
        }

        private void RemoveOldHistoryData()
        {
            double key = GetSessionTimeKey();
            foreach (var kv in _realTimeCarHistory.ToList())
            {
                if (key - kv.Key > 20000)
                {
                    _realTimeCarHistory.Remove(kv.Key);
                }
            }
        }

        private void AddCarUpdateToHistory(RealtimeCarUpdate realtimeCarUpdate)
        {
            double key = GetSessionTimeKey();
            if (key == 0) return; // session not started

            if (_realTimeCarHistory.ContainsKey(key)) 
            {
                //Debug.WriteLine($"+ add to key {key} car index {realtimeCarUpdate.CarIndex}");
                var carUpdate = _realTimeCarHistory[key];
                if (carUpdate.ContainsKey(realtimeCarUpdate.CarIndex))
                {
                    carUpdate[realtimeCarUpdate.CarIndex] = realtimeCarUpdate;
                }
                else
                {
                    _realTimeCarHistory[key].Add(realtimeCarUpdate.CarIndex, realtimeCarUpdate);
                }
                
            } 
            else 
            {
                Dictionary<int, RealtimeCarUpdate> carUpdate = new Dictionary<int, RealtimeCarUpdate>();
                carUpdate[realtimeCarUpdate.CarIndex] = realtimeCarUpdate;
                //Debug.WriteLine($"# create key {key} car index {realtimeCarUpdate.CarIndex}");
                _realTimeCarHistory[key] = carUpdate;
            }
        }

        private double GetSessionTimeKey()
        {
            return _realtimeUpdate.SessionTime.TotalMilliseconds;
        }

        private void RealTimeUpdate_EventHandler(object sender, RealtimeUpdate realtimeUpdate)
        {
            _realtimeUpdate = realtimeUpdate;
        }

        private void TrackDataUpdate_EventHandler(object sender, TrackData trackData)
        {
            Debug.WriteLine($"receive track data update, track id=({trackData.TrackId}) track distance=({trackData.TrackMeters})");
            _trackDistance = trackData.TrackMeters;
        }

        private class CarInfoWithRealTime : CarInfo
        {
            public RealtimeCarUpdate RealtimeCarUpdate { get; set; }

            public CarInfoWithRealTime(CarInfo carInfoIn) : base(carInfoIn.CarIndex)
            {
                CarModelType = carInfoIn.CarModelType;
                TeamName = carInfoIn.TeamName;
                RaceNumber = carInfoIn.RaceNumber;
                CupCategory = carInfoIn.CupCategory;
                CurrentDriverIndex = carInfoIn.CurrentDriverIndex;
                Nationality = carInfoIn.Nationality;
            }

            public CarInfoWithRealTime(ushort carIndex) : base(carIndex)
            {
            }
        }

        private class ContactData
        {
            public double SessionTime { get; set; }
        }
    }
}
