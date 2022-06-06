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

        public List<List<BroadcastingEvent>> GetNewAccidents()
        {

            List<List<BroadcastingEvent>> list = new List<List<BroadcastingEvent>>();
            DateTime currentTime = DateTime.Now;

            foreach (var accidentDataKV in _accidentDataList.ToList())
            {
                int key = accidentDataKV.Key;
                // return accident data older than 1.5 sec.
                if ((currentTime - _accidentDataList[key].Timestamp).TotalSeconds > 1.5)
                {
                    list.Add(_accidentDataList[key].EventList);
                    _accidentDataList.Remove(key);
                }
            }
                
            return list;
        }

        internal void Start()
        {
            BroadcastTracker.Instance.OnBroadcastEvent += Broadcast_EventHandler;
        }

        internal void Stop()
        {
            BroadcastTracker.Instance.OnBroadcastEvent -= Broadcast_EventHandler;
            _accidentDataList?.Clear();
        }

        private void Broadcast_EventHandler(object sender, BroadcastingEvent broadcastingEvent)
        {
            switch (broadcastingEvent.Type)
            {
                case BroadcastingCarEventType.Accident:
                    {

                        Debug.WriteLine($"#{broadcastingEvent.CarData.RaceNumber}|{broadcastingEvent.TimeMs}| {broadcastingEvent.CarData.GetCurrentDriverName()} had an accident. {broadcastingEvent.Msg}");

                        if (broadcastingEvent.CarData == null)
                            break;

                        var added = false;

                        foreach (var key in _accidentDataList.Keys)
                        {
                            //Debug.WriteLine($"- time diff {broadcastingEvent.TimeMs} - {accidentTimeData.accidentTimeMS} = {(broadcastingEvent.TimeMs - accidentTimeData.accidentTimeMS)}");
                            // Group all accidents within 1 sec. together
                            // If an accident occurs at the same time on different track positions, we have only one accident.
                            if ((broadcastingEvent.TimeMs - key) < 1000) 
                            {
                                //Debug.WriteLine($"- add to existing group {accidentTimeData.accidentTimeMS} size: {_accidentDataList[accidentTimeData].Count}");
                                added = true;
                                _accidentDataList[key].EventList.Add(broadcastingEvent);
                            }
                        }

                        if (!added)
                        {
                            //Debug.WriteLine($"+ create new group");
                            //List<BroadcastingEvent> broadcastEventList = new List<BroadcastingEvent> { broadcastingEvent };

                            AccidentData accidentData = new AccidentData();
                            accidentData.AccidentTimeMS = broadcastingEvent.TimeMs;
                            accidentData.Timestamp = DateTime.Now;
                            accidentData.EventList.Add(broadcastingEvent);

                            _accidentDataList[broadcastingEvent.TimeMs] = accidentData;
                        }

                        break;
                    }
            }

        }

        private class AccidentData
        {
            public DateTime Timestamp;
            public int AccidentTimeMS;
            public List<BroadcastingEvent> EventList = new List<BroadcastingEvent>();
        }

    }


}
