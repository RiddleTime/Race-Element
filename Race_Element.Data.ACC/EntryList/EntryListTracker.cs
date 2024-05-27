using Newtonsoft.Json;
using RaceElement.Broadcast;
using RaceElement.Broadcast.Structs;
using RaceElement.Data.ACC.EntryList.TrackPositionGraph;
using RaceElement.Data.ACC.Tracker;
using System;
using System.Collections.Concurrent;
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

    internal readonly ConcurrentDictionary<int, CarData> _entryListCars = [];
    public List<KeyValuePair<int, CarData>> Cars
    {
        get
        {
            return [.. _entryListCars];
        }
    }

    public CarData GetCarData(int identifier)
    {

        if (_entryListCars.TryGetValue(identifier, out CarData carData))
        {
            return carData;
        }

        return null;
    }

    private void Clear()
    {

        _entryListCars.Clear();
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
        List<KeyValuePair<int, CarData>> cars = [.. _entryListCars];

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

            _entryListCars.Remove(entry.Key, out CarData _);

            PositionGraph.Instance.RemoveCar(entry.Key);
        }
    }

    private void UpdateEntryList(int carIndex, object obj, bool fromBroadCast = false)
    {
        // Remove drives that haven't received updates in a while
        CleanupEntryList();


        if (!_entryListCars.TryGetValue(carIndex, out var carData))
        {
            carData = new CarData();
            _entryListCars.TryAdd(carIndex, carData);
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

    private void Broadcast_EventHandler(object sender, BroadcastingEvent evt)
    {
        switch (evt.Type)
        {
            case BroadcastingCarEventType.LapCompleted:
                {
                    UpdateEntryList(evt.CarData.CarIndex, evt.CarData, true);
                }
                break;

            default:
                {
                    Debug.WriteLine(JsonConvert.SerializeObject(evt));
                }
                break;
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
