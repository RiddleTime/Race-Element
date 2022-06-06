﻿using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using ACCManager.Data.ACC.Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.EntryList
{
    public class EntryListTracker
    {
        public class CarData
        {
            public CarInfo CarInfo { get; set; }
            public RealtimeCarUpdate RealtimeCarUpdate { get; set; }
        }

        private static EntryListTracker _instance;
        public static EntryListTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new EntryListTracker();
                return _instance;
            }
            private set { _instance = value; }
        }

        private class AccidentData
        {
            public BroadcastingEvent Event;
            public DateTime Timestamp;
        }

        private Dictionary<int, List<AccidentData>> accidentDataList = new Dictionary<int, List<AccidentData>>();


        private Dictionary<int, CarData> _entryListCars = new Dictionary<int, CarData>();
        public List<KeyValuePair<int, CarData>> Cars
        {
            get
            {
                lock (_entryListCars)
                {
                    return _entryListCars.ToList();
                }
            }
        }
        private bool _isRunning = false;
        private readonly ACCSharedMemory _sharedMemory;

        private EntryListTracker()
        {
            _sharedMemory = new ACCSharedMemory();
        }

        internal void Start()
        {
            _isRunning = true;
            BroadcastTracker.Instance.OnRealTimeCarUpdate += RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnEntryListUpdate += EntryListUpdate_EventHandler;
            BroadcastTracker.Instance.OnBroadcastEvent += Broadcast_EventHandler;
            StartEntryListCleanupTracker();
        }

        internal void Stop()
        {
            _isRunning = false;
            BroadcastTracker.Instance.OnRealTimeCarUpdate -= RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnEntryListUpdate -= EntryListUpdate_EventHandler;
            BroadcastTracker.Instance.OnBroadcastEvent -= Broadcast_EventHandler;
            _entryListCars?.Clear();
        }

        private void StartEntryListCleanupTracker()
        {
            new Task(() =>
            {
                try
                {
                    while (_isRunning)
                    {
                        Thread.Sleep(100);

                        int[] activeCarIds = _sharedMemory.ReadGraphicsPageFile().CarIds;

                        List<KeyValuePair<int, CarData>> datas = _entryListCars.ToList();
                        foreach (var entryListCar in datas)
                        {
                            bool isInServer = activeCarIds.Contains(entryListCar.Key);
                            if (!isInServer)
                                lock (_entryListCars)
                                    _entryListCars.Remove(entryListCar.Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }).Start();
        }

        private void Broadcast_EventHandler(object sender, BroadcastingEvent broadcastingEvent)
        {
            try
            {
                switch (broadcastingEvent.Type)
                {
                    case BroadcastingCarEventType.LapCompleted:
                        {
                            if (broadcastingEvent.CarData == null)
                                break;
                            //lock (_entryListCars)
                            //{
                            //    CarData carData;
                            //    if (_entryListCars.TryGetValue(broadcastingEvent.CarData.CarIndex, out carData))
                            //    {
                            //        carData.CarInfo = broadcastingEvent.CarData;
                            //    }
                            //    else
                            //    {
                            //        //Debug.WriteLine($"BroadcastingCarEventType.LapCompleted car index: {broadcastingEvent.CarData.CarIndex} not found in entry list");
                            //        carData = new CarData();
                            //        carData.CarInfo = broadcastingEvent.CarData;
                            //        _entryListCars.Add(broadcastingEvent.CarData.CarIndex, carData);
                            //    }
                            //}
                            break;
                        }
                    case BroadcastingCarEventType.Accident:
                        {
                            if (broadcastingEvent.CarData == null)
                                break;

                            //Debug.WriteLine($"#{broadcastingEvent.CarData.RaceNumber}| {broadcastingEvent.CarData.GetCurrentDriverName()} had an accident. {broadcastingEvent.Msg}");

                            AccidentData accidentData = new AccidentData();
                            accidentData.Event = broadcastingEvent;
                            accidentData.Timestamp = DateTime.Now;

                            int accidentGroup = broadcastingEvent.TimeMs / 1000;
                            if (accidentDataList.ContainsKey(accidentGroup))
                            {
                                accidentDataList[accidentGroup].Add(accidentData);
                            } 
                            else
                            {
                                List<AccidentData> accidentList = new List<AccidentData>();
                                accidentList.Add(accidentData);
                                accidentDataList[accidentGroup] = accidentList;
                            }
                            break;
                        }

                    default:
                        {
                            if (broadcastingEvent.CarData == null)
                                break;

                            //Debug.WriteLine($"{broadcastingEvent.Type} - {broadcastingEvent.CarData.RaceNumber} - {broadcastingEvent.Msg}");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void EntryListUpdate_EventHandler(object sender, CarInfo carInfo)
        {
            try
            {
                CarData carData;
                lock (_entryListCars)
                {
                    if (_entryListCars.TryGetValue(carInfo.CarIndex, out carData))
                    {
                        carData.CarInfo = carInfo;
                    }
                    else
                    {
                        carData = new CarData();
                        carData.CarInfo = carInfo;

                        _entryListCars.Add(carInfo.CarIndex, carData);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void RealTimeCarUpdate_EventHandler(object sender, RealtimeCarUpdate carUpdate)
        {
            try
            {
                CarData carData;
                lock (_entryListCars)
                {
                    if (_entryListCars.TryGetValue(carUpdate.CarIndex, out carData))
                    {
                        carData.RealtimeCarUpdate = carUpdate;
                    }
                    else
                    {
                        //Debug.WriteLine($"RealTimeCarUpdate_EventHandler car index: {carUpdate.CarIndex} not found in entry list");
                        carData = new CarData();
                        carData.RealtimeCarUpdate = carUpdate;

                        _entryListCars.Add(carUpdate.CarIndex, carData);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            // publish accident events that are older than 1 second
            DateTime currentTime = DateTime.Now;
            foreach (var accidentDataKV in accidentDataList.ToList())
            {
                // first accident element is always available
                var firstAccidentData = accidentDataKV.Value[0];
                List<AccidentData> pushAccidentList = new List<AccidentData>();
                if (((currentTime - firstAccidentData.Timestamp).TotalSeconds) > 1) {
                    pushAccidentList = accidentDataKV.Value;
                    accidentDataList.Remove(accidentDataKV.Key);
                }

                if (pushAccidentList.Count > 0)
                {
                    string carNumbers = string.Join(" ", pushAccidentList.Select(x => x.Event.CarData.RaceNumber).ToArray());
                    string accidentMessage = $"{firstAccidentData.Event.Type} between [{carNumbers}]";
                    Debug.WriteLine($"{accidentMessage}");
                }
                
            }
        }
    }
}
