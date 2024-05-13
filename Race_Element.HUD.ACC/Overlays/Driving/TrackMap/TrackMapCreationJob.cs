using System.Drawing;
using System.Collections.Generic;

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.ACC.Core;
using RaceElement.Util;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

public class TrackMapCreationJob : AbstractLoopJob
{
    private enum CreationState
    {
        Start,
        TraceTrack,
        LoadFromFile,
        NotifySubscriber,
        End
    }

    public EventHandler<List<PointF>> OnMapPositionsCallback;
    public EventHandler<string> OnMapProgressCallback;

    private readonly List<PointF> _trackedPositions;
    private CreationState _mapTrackingState;

    private int _completedLaps;
    private int _prevPacketId;

    private float _trackingPercentage;

    public TrackMapCreationJob()
    {
        _mapTrackingState = CreationState.Start;
        _trackedPositions = new();

        _completedLaps = ACCSharedMemory.Instance.PageFileGraphic.CompletedLaps;
        _prevPacketId = ACCSharedMemory.Instance.PageFileGraphic.PacketId;

        _trackingPercentage = 0;
    }

    public override void RunAction()
    {
        switch (_mapTrackingState)
        {
            case CreationState.Start:
            {
                _mapTrackingState = InitialState();
            } break;

            case CreationState.LoadFromFile:
            {
                try { _mapTrackingState = LoadMapFromFile(); }
                catch (Exception e) { Debug.WriteLine(e);  }
            } break;

            case CreationState.TraceTrack:
            {
                _mapTrackingState = PositionTracking();
            } break;

            case CreationState.NotifySubscriber:
            {
                {
                    const string msg = "Map tracked. Enjoy it!";
                    OnMapProgressCallback?.Invoke(null, msg);
                }

                OnMapPositionsCallback?.Invoke(this, _trackedPositions);
                _mapTrackingState = CreationState.End;
            } break;

            default:
            {
                OnMapProgressCallback?.Invoke(null, null);
                Thread.Sleep(10);
            } break;
        }
    }

    private CreationState InitialState()
    {
        var laps = ACCSharedMemory.Instance.PageFileGraphic.CompletedLaps;
        var trackName = ACCSharedMemory.Instance.PageFileStatic.Track;
        string path = FileUtil.RaceElementTracks + trackName + ".bin";

        if (trackName.Length > 0 && File.Exists(path))
        {
            return CreationState.LoadFromFile;
        }

        {
            const string msg = "Tracking state -> waiting for lap counter to change.";
            OnMapProgressCallback?.Invoke(null, msg);
        }

        if (!AccProcess.IsRunning)
        {
            return CreationState.Start;
        }

        if (laps != _completedLaps)
        {
            _completedLaps = laps;
            return CreationState.TraceTrack;
        }

        return CreationState.Start;
    }

    private CreationState PositionTracking()
    {
        {
            const string msg = "Tracking state -> tracking map ({0:0.0}%),\nif you invalidate the lap you will have to start again.";
            OnMapProgressCallback?.Invoke(null, String.Format(msg, _trackingPercentage * 100));
        }

        var pageGraphics = ACCSharedMemory.Instance.PageFileGraphic;

        if (pageGraphics.PacketId == _prevPacketId)
        {
            return CreationState.TraceTrack;
        }

        if (Math.Abs(pageGraphics.NormalizedCarPosition - _trackingPercentage) <= float.Epsilon)
        {
            return CreationState.TraceTrack;
        }

        _trackingPercentage = pageGraphics.NormalizedCarPosition;
        _prevPacketId = pageGraphics.PacketId;


        if (!pageGraphics.IsValidLap)
        {
            _trackingPercentage = 0;
            _trackedPositions.Clear();
            return CreationState.Start;
        }

        for (int i = 0; i < pageGraphics.ActiveCars; ++i)
        {
            if (pageGraphics.CarIds[i] == pageGraphics.PlayerCarID)
            {
                var pos = pageGraphics.CarCoordinates[i];
                _trackedPositions.Add(new PointF(pos.X, pos.Z));
                break;
            }
        }

        if (pageGraphics.CompletedLaps != _completedLaps)
        {
            WriteMapToFile();
            return CreationState.NotifySubscriber;
        }

        return CreationState.TraceTrack;
    }

    private CreationState LoadMapFromFile()
    {
        {
            const string msg = "Tracking state -> Map found on disk, loading it.";
            OnMapProgressCallback?.Invoke(null, msg);
        }

        var trackName = ACCSharedMemory.Instance.PageFileStatic.Track.ToLower();
        string path = FileUtil.RaceElementTracks + trackName + ".bin";

        using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using BinaryReader binaryReader = new(fileStream);

        while (fileStream.Position < fileStream.Length)
        {
            {
                const string msg = "Tracking state -> loading from disk ({0:0.0}%)";
                OnMapProgressCallback?.Invoke(null, String.Format(msg, ((float)fileStream.Position / fileStream.Length) * 100.0f));
            }

            PointF pos = new()
            {
                X = binaryReader.ReadSingle(),
                Y = binaryReader.ReadSingle()
            };

            _trackedPositions.Add(pos);
        }

        binaryReader.Close();
        fileStream.Close();

        return CreationState.NotifySubscriber;
    }

    private void WriteMapToFile()
    {
        {
            const string msg = "Tracking state -> write map to file.";
            OnMapProgressCallback?.Invoke(null, msg);
        }

        DirectoryInfo directory = new(FileUtil.RaceElementTracks);
        if (!directory.Exists) directory.Create();

        var trackName = ACCSharedMemory.Instance.PageFileStatic.Track.ToLower();
        string path = FileUtil.RaceElementTracks + trackName + ".bin";

        using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        using BinaryWriter binaryWriter = new(fileStream);

        for (var i = 0; i < _trackedPositions.Count; ++i)
        {
            {
                const string msg = "Tracking state -> writing from disk ({0:0.0}%)";
                OnMapProgressCallback?.Invoke(null, String.Format(msg, ((float)i / _trackedPositions.Count) * 100.0f));
            }

            binaryWriter.Write(_trackedPositions[i].X);
            binaryWriter.Write(_trackedPositions[i].Y);
        }

        binaryWriter.Close();
        fileStream.Close();
    }
}
