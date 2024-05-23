using Newtonsoft.Json;
using RaceElement.Broadcast;
using RaceElement.Broadcast.Structs;
using RaceElement.Data.ACC.EntryList.TrackPositionGraph;
using RaceElement.Data.ACC.Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RaceElement.Data.ACC.EntryList;

public class EntryListTracker
{
    public class CarData
    {
        public DateTime LastUpdate;
        public CarInfo CarInfo { get; set; }
        public RealtimeCarUpdate RealtimeCarUpdate { get; set; }
    }

    private static EntryListTracker _instance;
    public static EntryListTracker Instance
    {
        get
        {
            _instance ??= new EntryListTracker();
            return _instance;
        }
    }

    internal Dictionary<int, CarData> _entryListCars = [];
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

    public CarData GetCarData(int identifier)
    {
        lock (_entryListCars)
        {
            if (_entryListCars.TryGetValue(identifier, out CarData carData))
            {
                return carData;
            }
        }

        return null;
    }

    private void Clear()
    {
        lock (_entryListCars)
        {
            _entryListCars.Clear();
        }
    }

    internal void Start()
    {
        BroadcastTracker.Instance.OnRealTimeCarUpdate += RealTimeCarUpdate_EventHandler;
        BroadcastTracker.Instance.OnEntryListUpdate += EntryListUpdate_EventHandler;
        BroadcastTracker.Instance.OnBroadcastEvent += Broadcast_EventHandler;

        Clear();
        Debug.WriteLine("EntryListTracker started");
    }

    internal void Stop()
    {
        Debug.WriteLine("Stopping EntryListTracker");
        Clear();

        BroadcastTracker.Instance.OnRealTimeCarUpdate -= RealTimeCarUpdate_EventHandler;
        BroadcastTracker.Instance.OnEntryListUpdate -= EntryListUpdate_EventHandler;
        BroadcastTracker.Instance.OnBroadcastEvent -= Broadcast_EventHandler;
    }

    private void CleanupEntryList()
    {
        var cleanupWaitTime = TimeSpan.FromSeconds(5);
        List<KeyValuePair<int, CarData>> cars;

        lock (_entryListCars)
        {
            cars = _entryListCars.ToList();
        }

        foreach (var entry in cars)
        {
            if (entry.Value == null)
            {
                continue;
            }

            if ((DateTime.UtcNow - entry.Value.LastUpdate) < cleanupWaitTime)
            {
                continue;
            }

            lock (_entryListCars)
            {
                _entryListCars.Remove(entry.Key);
            }

            PositionGraph.Instance.RemoveCar(entry.Key);
        }
    }

    private void UpdateEntryList(int carIndex, object obj, bool fromBroadCast = false)
    {
        // Remove drives that haven't received updates in a while
        CleanupEntryList();

        lock (_entryListCars)
        {
            if (!_entryListCars.TryGetValue(carIndex, out var carData))
            {
                carData = new CarData();
                _entryListCars.Add(carIndex, carData);
            }

            if (obj is CarInfo carInfo)
            {
                carData.CarInfo = carInfo;
                carData.LastUpdate = DateTime.UtcNow;
            }
            else if (obj is RealtimeCarUpdate carUpdate)
            {
                carData.RealtimeCarUpdate = carUpdate;
                carData.LastUpdate = DateTime.UtcNow;
            }

            if (fromBroadCast) return;
            var carGraph = PositionGraph.Instance.GetCar(carIndex);

            if (carGraph == null)
            {
                PositionGraph.Instance.AddCar(carIndex);
            }
            else
            {
                carGraph.UpdateLocation(carData.RealtimeCarUpdate.SplinePosition, carData.RealtimeCarUpdate.CarLocation);
            }
        }
    }

    private void Broadcast_EventHandler(object sender, BroadcastingEvent broadcastingEvent)
    {
        // Remove drives that haven't received updates in a while
        CleanupEntryList();

        switch (broadcastingEvent.Type)
        {
            case BroadcastingCarEventType.BestSessionLap:
            {
                Debug.WriteLine(JsonConvert.SerializeObject(broadcastingEvent));
            } break;

            case BroadcastingCarEventType.LapCompleted:
            {
                UpdateEntryList(broadcastingEvent.CarData.CarIndex, broadcastingEvent.CarData, true);
            } break;

            case BroadcastingCarEventType.Accident:
            {
                Debug.WriteLine(JsonConvert.SerializeObject(broadcastingEvent));
            } break;

            default:
            {
                Debug.WriteLine(JsonConvert.SerializeObject(broadcastingEvent));
            } break;
        }
    }

    private void EntryListUpdate_EventHandler(object sender, CarInfo carInfo)
    {
        UpdateEntryList(carInfo.CarIndex, carInfo);
    }

    private void RealTimeCarUpdate_EventHandler(object sender, RealtimeCarUpdate carUpdate)
    {
        UpdateEntryList(carUpdate.CarIndex, carUpdate);
    }
}
