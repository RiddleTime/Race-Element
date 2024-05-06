using System.Drawing;
using System.Collections.Generic;

using System;
using System.IO;
using System.Threading;

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

    private readonly List<PointF> _trackedPositions;
    private CreationState _mapTrackingState;

    private int _completedLaps;
    private int _prevPacketId;

    public TrackMapCreationJob()
    {
        _mapTrackingState = CreationState.Start;
        _trackedPositions = new();

        _completedLaps = ACCSharedMemory.Instance.PageFileGraphic.CompletedLaps;
        _prevPacketId = ACCSharedMemory.Instance.PageFileGraphic.PacketId;
    }

    public override void RunAction()
    {
        switch (_mapTrackingState)
        {
            case CreationState.Start:
                {
                    _mapTrackingState = InitialState();
                }
                break;

            case CreationState.LoadFromFile:
                {
                    _mapTrackingState = LoadMapFromFile();
                }
                break;

            case CreationState.TraceTrack:
                {
                    _mapTrackingState = PositionTracking();
                }
                break;

            case CreationState.NotifySubscriber:
                {
                    OnMapPositionsCallback?.Invoke(this, _trackedPositions);
                    _mapTrackingState = CreationState.End;
                }
                break;

            default:
                {
                    Thread.Sleep(10);
                }
                break;
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
        if (ACCSharedMemory.Instance.PageFileGraphic.PacketId == _prevPacketId)
        {
            return CreationState.TraceTrack;
        }

        var pageGraphics = ACCSharedMemory.Instance.PageFileGraphic;
        _prevPacketId = pageGraphics.PacketId;

        if (!pageGraphics.IsValidLap)
        {
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
        var trackName = ACCSharedMemory.Instance.PageFileStatic.Track;
        string path = FileUtil.RaceElementTracks + trackName + ".bin";

        using FileStream fileStream = new(path, FileMode.Open);
        using BinaryReader binaryReader = new(fileStream);

        while (fileStream.Position < fileStream.Length)
        {
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
        DirectoryInfo directory = new(FileUtil.RaceElementTracks);
        if (!directory.Exists) directory.Create();

        var trackName = ACCSharedMemory.Instance.PageFileStatic.Track;
        string path = FileUtil.RaceElementTracks + trackName + ".bin";

        using FileStream fileStream = new(path, FileMode.Create);
        using BinaryWriter binaryWriter = new(fileStream);

        foreach (var it in _trackedPositions)
        {
            binaryWriter.Write(it.X);
            binaryWriter.Write(it.Y);
        }

        binaryWriter.Close();
        fileStream.Close();
    }
}
