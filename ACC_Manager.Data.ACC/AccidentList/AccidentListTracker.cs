using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using ACCManager.Data.ACC.Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ACCManager.Data.ACC.AccidentList
{
    public class AccidentListTracker
    {
        private static AccidentListTracker _instance;
        private float _trackDistance = 0f;

        // the key into the dictionary is the timeMS of the first accident event
        private Dictionary<int, AccidentData> _accidentDataList = new Dictionary<int, AccidentData>();

        public static AccidentListTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new AccidentListTracker();
                return _instance;
            }
            private set { _instance = value; }
        }
        /*
        public List<List<BroadcastingEvent>> GetNewAccidents()
        {

            List<List<BroadcastingEvent>> list = new List<List<BroadcastingEvent>>();
            DateTime currentTime = DateTime.Now;

            lock (_accidentDataList)
            {
                foreach (var accidentDataKV in _accidentDataList.ToList())
                {
                    int key = accidentDataKV.Key;
                    // return accident data older than 1.5 sec.
                    if ((currentTime - _accidentDataList[key].Timestamp).TotalSeconds > 1.5)
                    {
                        //list.Add(_accidentDataList[key].EventList); // TOOD
                        _accidentDataList.Remove(key);
                    }
                }
            }

            return list;
        }
        */
        internal void Start()
        {
            BroadcastTracker.Instance.OnBroadcastEvent += Broadcast_EventHandler;
            BroadcastTracker.Instance.OnRealTimeCarUpdate += RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnTrackDataUpdate += TrackDataUpdate_EventHandler;
        }

        internal void Stop()
        {
            BroadcastTracker.Instance.OnBroadcastEvent -= Broadcast_EventHandler;
            BroadcastTracker.Instance.OnRealTimeCarUpdate -= RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnTrackDataUpdate -= TrackDataUpdate_EventHandler;
            _accidentDataList?.Clear();
        }

        private void TrackDataUpdate_EventHandler(object sender, TrackData trackData)
        {
            //if (_trackDistance != 0) return;
            Debug.WriteLine($"receive track data update, track id=({trackData.TrackId}) track distance=({trackData.TrackMeters})");
            _trackDistance = trackData.TrackMeters;
        }

        private void RealTimeCarUpdate_EventHandler(object sender, RealtimeCarUpdate realtimeCarUpdate)
        {

            AddRealtimeCarUpdate(realtimeCarUpdate);
            CheckAccidentDistance();

        }

        private void CheckAccidentDistance()
        {
            // if we have no track distance, we can not calculate the distance between the accident cars
            if (_trackDistance == 0) return;

            DateTime currentTime = DateTime.Now;

            foreach (var accidentDataKV in _accidentDataList)
            {
                int key = accidentDataKV.Key;
                // check distance for accidents older than 1.5 sec.
                if ((currentTime - _accidentDataList[key].Timestamp).TotalSeconds > 1.5)
                {
                    

                    var accidentEventList = _accidentDataList[key].AccidentEventList;
                    // accident event with only one car can be ignored
                    if (accidentEventList.Count < 2)
                    {
                        continue;
                    }

                    for (int i= 0; i < accidentEventList.Count-1; i++)
                    {
                        float distance = (accidentEventList[i].ReadlTimeCarUpdate.SplinePosition - accidentEventList[i + 1].ReadlTimeCarUpdate.SplinePosition) * _trackDistance;
                        _accidentDataList[key].DistanceList.Add(distance);
                        //Debug.WriteLine($"car distance accident=({key}) car1=({accidentEventList[i].ReadlTimeCarUpdate.CarIndex}) car2=({accidentEventList[i + 1].ReadlTimeCarUpdate.CarIndex}) distance=({distance})");
                    }

                    if (_accidentDataList[key].DistanceList.Count == accidentEventList.Count - 1)
                    {
                        Debug.WriteLine($"distance list: {string.Join(" ", _accidentDataList[key].DistanceList)}");
                    }

                }
            }
        }

        private void AddRealtimeCarUpdate(RealtimeCarUpdate realtimeCarUpdate)
        {
            // check if there is an accident in our list where no realtime car update is set
            DateTime currentTime = DateTime.Now;

            foreach (var accidentDataKV in _accidentDataList)
            {
                int key = accidentDataKV.Key;
                // irgnore all accident data older than 1 sec.
                if ((currentTime - _accidentDataList[key].Timestamp).TotalSeconds < 1.0)
                {
                    foreach (var accidentEvent in _accidentDataList[key].AccidentEventList)
                    {
                        if (accidentEvent.ReadlTimeCarUpdate.CarIndex == 0 && accidentEvent.BroadcastingEvent.CarData.CarIndex == realtimeCarUpdate.CarIndex)
                        {
                            Debug.WriteLine($"add realtime car update for car {realtimeCarUpdate.CarIndex} to accident {key} events");
                            accidentEvent.ReadlTimeCarUpdate = realtimeCarUpdate;
                        }
                    }
                }
            }

        }

        private void Broadcast_EventHandler(object sender, BroadcastingEvent broadcastingEvent)
        {
            switch (broadcastingEvent.Type)
            {
                case BroadcastingCarEventType.Accident:
                    {

                        if (broadcastingEvent.CarData == null || broadcastingEvent.Msg == null)
                            break;

                        Debug.WriteLine($"#{broadcastingEvent.CarData.RaceNumber}|{broadcastingEvent.TimeMs}| {broadcastingEvent.CarData.GetCurrentDriverName()} had an accident. {broadcastingEvent.Msg}");

                        var added = false;

                        lock (_accidentDataList)
                        {
                            foreach (var key in _accidentDataList.Keys)
                            {
                                //Debug.WriteLine($"- time diff {broadcastingEvent.TimeMs} - {accidentTimeData.accidentTimeMS} = {(broadcastingEvent.TimeMs - accidentTimeData.accidentTimeMS)}");
                                // Group all accidents within 1 sec. together
                                // If an accident occurs at the same time on different track positions, we have only one accident.
                                if ((broadcastingEvent.TimeMs - key) < 1000)
                                {
                                    Debug.WriteLine($"add to existing group {key} size: {_accidentDataList[key].AccidentEventList.Count}");
                                    added = true;
                                    _accidentDataList[key].AccidentEventList.Add(new AccidentEvents() { BroadcastingEvent = broadcastingEvent, ReadlTimeCarUpdate = new RealtimeCarUpdate()});
                                }
                            }

                            if (!added)
                            {
                                Debug.WriteLine($"create new accident group group {broadcastingEvent.TimeMs}");
                                
                                AccidentData accidentData = new AccidentData()
                                {
                                    AccidentTimeMS = broadcastingEvent.TimeMs,
                                    Timestamp = DateTime.Now,
                                };
                                accidentData.AccidentEventList.Add(new AccidentEvents() { BroadcastingEvent = broadcastingEvent, ReadlTimeCarUpdate = new RealtimeCarUpdate() });
                                _accidentDataList[broadcastingEvent.TimeMs] = accidentData;
                            }
                        }
                        

                        break;
                    }
            }

        }

        public class AccidentData
        {
            public DateTime Timestamp { get; internal set; }
            public int AccidentTimeMS { get; internal set; }
            public List<float> DistanceList = new List<float>();
            public List<AccidentEvents> AccidentEventList = new List<AccidentEvents>();
        }

        public class AccidentEvents
        {
            public BroadcastingEvent BroadcastingEvent { set; get; }
            public RealtimeCarUpdate ReadlTimeCarUpdate { set; get; }
        }

    }


}
