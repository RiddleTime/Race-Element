using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using ACCManager.Data.ACC.EntryList.TrackPositionGraph;
using ACCManager.Data.ACC.AccidentList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACCManager.Data.ACC.Tracker;

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

        internal Dictionary<int, CarData> _entryListCars = new Dictionary<int, CarData>();
        private static AccidentListTracker _accidentListTracker = AccidentListTracker.Instance;

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

        private EntryListTracker()
        {

        }

        internal void Start()
        {
#if DEBUG

            _isRunning = true;
            BroadcastTracker.Instance.OnRealTimeCarUpdate += RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnEntryListUpdate += EntryListUpdate_EventHandler;
            BroadcastTracker.Instance.OnBroadcastEvent += Broadcast_EventHandler;

            _accidentListTracker.Start();

            StartEntryListCleanupTracker();
#endif
        }

        internal void Stop()
        {
#if DEBUG
            Debug.WriteLine("Stopping EntryListTracker");
            _isRunning = false;
            BroadcastTracker.Instance.OnRealTimeCarUpdate -= RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnEntryListUpdate -= EntryListUpdate_EventHandler;
            BroadcastTracker.Instance.OnBroadcastEvent -= Broadcast_EventHandler;

            _accidentListTracker.Stop();

            _entryListCars?.Clear();
#endif
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

                        int[] activeCarIds = ACCSharedMemory.Instance.ReadGraphicsPageFile().CarIds;

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
                            //Debug.WriteLine($"#{broadcastingEvent.CarData.RaceNumber}| {broadcastingEvent.CarData.GetCurrentDriverName()} had an accident. {broadcastingEvent.Msg}");
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

                        // 
                        Car car = PositionGraph.Instance.GetCar(carInfo.CarIndex);
                        if (car != null)
                            car.UpdateLocation(carData.RealtimeCarUpdate.SplinePosition, carData.RealtimeCarUpdate.CarLocation);
                    }
                    else
                    {
                        carData = new CarData();
                        carData.CarInfo = carInfo;

                        _entryListCars.Add(carInfo.CarIndex, carData);
                        PositionGraph.Instance.AddCar(carInfo.CarIndex);
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

                        Car car = PositionGraph.Instance.GetCar(carUpdate.CarIndex);
                        if (car != null)
                            car.UpdateLocation(carUpdate.SplinePosition, carUpdate.CarLocation);
                    }
                    else
                    {
                        //Debug.WriteLine($"RealTimeCarUpdate_EventHandler car index: {carUpdate.CarIndex} not found in entry list");
                        carData = new CarData();
                        carData.RealtimeCarUpdate = carUpdate;

                        _entryListCars.Add(carUpdate.CarIndex, carData);

                        PositionGraph.Instance.AddCar(carUpdate.CarIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            /*var accidentList = _accidentListTracker.GetNewAccidents();
            foreach (var broadcastEventList in accidentList)
            {
                if (broadcastEventList.Count == 0) continue;

                string carNumbers = string.Join(" ", broadcastEventList.Select(x => x.CarData.RaceNumber).ToArray());
                string accidentMessage = $"accident between [{carNumbers}]";
                Debug.WriteLine($"{accidentMessage}");

            }*/

        }
    }
}
