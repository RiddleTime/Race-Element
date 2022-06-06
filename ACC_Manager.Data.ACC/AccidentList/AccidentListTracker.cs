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
        private Dictionary<AccidentTimeData, List<BroadcastingEvent>> _accidentDataList = new Dictionary<AccidentTimeData, List<BroadcastingEvent>>();

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
                AccidentTimeData accidentTimeData = accidentDataKV.Key;
                // return accident data older than 1.5 sec.
                if ((currentTime - accidentTimeData.Timestamp).TotalSeconds > 1.5)
                {
                    list.Add(_accidentDataList[accidentTimeData]);
                    _accidentDataList.Remove(accidentTimeData);
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
                        foreach (var accidentTimeData in _accidentDataList.Keys)
                        {
                            //Debug.WriteLine($"- time diff {broadcastingEvent.TimeMs} - {accidentTimeData.accidentTimeMS} = {(broadcastingEvent.TimeMs - accidentTimeData.accidentTimeMS)}");
                            // Group all accidents within 1 sec. together
                            // If an accident occurs at the same time on different track positions, we have only one accident.
                            if ((broadcastingEvent.TimeMs - accidentTimeData.accidentTimeMS) < 1000) 
                            {
                                //Debug.WriteLine($"- add to existing group {accidentTimeData.accidentTimeMS} size: {_accidentDataList[accidentTimeData].Count}");
                                added = true;
                                _accidentDataList[accidentTimeData].Add(broadcastingEvent);
                            }
                        }

                        if (!added)
                        {
                            //Debug.WriteLine($"+ create new group");
                            List<BroadcastingEvent> broadcastEventList = new List<BroadcastingEvent> { broadcastingEvent };

                            AccidentTimeData accidentTimeData = new AccidentTimeData();
                            accidentTimeData.accidentTimeMS = broadcastingEvent.TimeMs;
                            accidentTimeData.Timestamp = DateTime.Now;

                            _accidentDataList[accidentTimeData] = broadcastEventList;
                        }

                        break;
                    }
            }

        }

        private class AccidentTimeData
        {
            public DateTime Timestamp;
            public int accidentTimeMS;
        }
    }


}
