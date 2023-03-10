using RaceElement.Broadcast;
using RaceElement.Broadcast.Structs;
using RaceElement.Data.ACC.EntryList.TrackPositionGraph;
using RaceElement.Data.ACC.AccidentList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RaceElement.Data.ACC.Tracker;

namespace RaceElement.Data.ACC.EntryList
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
            //#if DEBUG

            _isRunning = true;
            BroadcastTracker.Instance.OnRealTimeCarUpdate += RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnEntryListUpdate += EntryListUpdate_EventHandler;
            BroadcastTracker.Instance.OnBroadcastEvent += Broadcast_EventHandler;

            _accidentListTracker.Start();

            StartEntryListCleanupTracker();
            Debug.WriteLine("Entrylist tracker started.");
            //#endif
        }

        internal void Stop()
        {
            //#if DEBUG
            Debug.WriteLine("Stopping EntryListTracker");
            _isRunning = false;
            BroadcastTracker.Instance.OnRealTimeCarUpdate -= RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnEntryListUpdate -= EntryListUpdate_EventHandler;
            BroadcastTracker.Instance.OnBroadcastEvent -= Broadcast_EventHandler;

            _accidentListTracker.Stop();

            _entryListCars?.Clear();
            //#endif
        }

        private void StartEntryListCleanupTracker()
        {
            new Thread(() =>
            {
                try
                {
                    List<(int, int)> previousTimes = new List<(int, int)>();
                    while (_isRunning)
                    {
                        const int waitTime = 5000;
                        Thread.Sleep(waitTime);

                        try
                        {
                            List<KeyValuePair<int, CarData>> newDatas = _entryListCars.ToList();
                            foreach (var entry in newDatas)
                            {
                                if (entry.Value != null)
                                {
                                    if (entry.Value.CarInfo == null)
                                    {
                                        //Debug.WriteLine($"Removed entry {entry.Key} - CarInfo null");  

                                        PositionGraph.Instance.RemoveCar(entry.Key);
                                        lock (_entryListCars)
                                        {
                                            _entryListCars.Remove(entry.Key);
                                        }
                                        continue;
                                    }

                                    if (entry.Value.RealtimeCarUpdate.CurrentLap == null)
                                    {
                                        //Debug.WriteLine($"Removed entry {entry.Key} - CurrentLap null");

                                        PositionGraph.Instance.RemoveCar(entry.Key);
                                        lock (_entryListCars)
                                        {
                                            _entryListCars.Remove(entry.Key);
                                        }
                                        continue;
                                    }

                                    int previousIndex = previousTimes.FindIndex(x => x.Item1 == entry.Key);
                                    if (previousIndex == -1)
                                        previousTimes.Add((entry.Key, entry.Value.RealtimeCarUpdate.CurrentLap.LaptimeMS.Value));
                                    else
                                    {
                                        int currentLapTime = entry.Value.RealtimeCarUpdate.CurrentLap.LaptimeMS.Value;
                                        if (currentLapTime > 0 && currentLapTime == previousTimes[previousIndex].Item2)
                                        {
                                            //Debug.WriteLine($"Possible leaver?: {entry.Key}, {previousIndex} - #{entry.Value.CarInfo.RaceNumber} - {entry.Value.CarInfo.GetCurrentDriverName()} - time: {entry.Value.RealtimeCarUpdate.CurrentLap.LaptimeMS}");

                                            Debug.WriteLine($"Removed entry {entry.Key} #{entry.Value.CarInfo.RaceNumber} - Laptime is frozen (above 0)");
                                            PositionGraph.Instance.RemoveCar(entry.Key);
                                            lock (_entryListCars)
                                            {
                                                _entryListCars.Remove(entry.Key);
                                                previousTimes.RemoveAt(previousIndex);
                                            }
                                            continue;
                                        }

                                        previousTimes[previousIndex] = (previousIndex, entry.Value.RealtimeCarUpdate.CurrentLap.LaptimeMS.Value);
                                    }

                                }
                            }


                            //int[] activeCarIds = ACCSharedMemory.Instance.ReadGraphicsPageFile().CarIds;

                            //List<KeyValuePair<int, CarData>> datas = _entryListCars.ToList();
                            //foreach (var entryListCar in datas)
                            //{
                            //    bool isInServer = activeCarIds.Contains(entryListCar.Key);
                            //    if (!isInServer)
                            //        lock (_entryListCars)
                            //            _entryListCars.Remove(entryListCar.Key);
                            //}

                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
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
                    case BroadcastingCarEventType.BestSessionLap:
                        {
                            break;
                        }
                    case BroadcastingCarEventType.LapCompleted:
                        {
                            if (broadcastingEvent.CarData == null)
                                break;
                            lock (_entryListCars)
                            {
                                CarData carData;
                                if (_entryListCars.TryGetValue(broadcastingEvent.CarData.CarIndex, out carData))
                                {
                                    carData.CarInfo = broadcastingEvent.CarData;
                                }
                                else
                                {
                                    //Debug.WriteLine($"BroadcastingCarEventType.LapCompleted car index: {broadcastingEvent.CarData.CarIndex} not found in entry list");
                                    carData = new CarData();
                                    carData.CarInfo = broadcastingEvent.CarData;
                                    _entryListCars.Add(broadcastingEvent.CarData.CarIndex, carData);
                                }
                            }
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

            //var accidentList = _accidentListTracker.GetNewAccidents();
            //foreach (var broadcastEventList in accidentList)
            //{
            //    if (broadcastEventList.Count() == 0) continue;

            //    string carNumbers = string.Join(" ", broadcastEventList.Select(x => x.CarData.RaceNumber).ToArray());
            //    string accidentMessage = $"accident between [{carNumbers}]";
            //    Debug.WriteLine($"{accidentMessage}");

            //}
        }
    }
}
