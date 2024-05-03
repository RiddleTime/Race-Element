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

    private readonly List<PointF> _trackedPositions;
    private CreationState _mapTrackingState;

    public EventHandler<List<PointF>> OnMapCreation;
    private float _carNormPosition;

    private int _completedLaps;
    private int _prevPacketId;

    public TrackMapCreationJob()
    {
        _mapTrackingState = CreationState.Start;
        _trackedPositions = new();

        _carNormPosition = ACCSharedMemory.Instance.PageFileGraphic.NormalizedCarPosition;
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
            } break;

            case CreationState.LoadFromFile:
            {
                _mapTrackingState = LoadMapFromFile();
            } break;

            case CreationState.TraceTrack:
            {
                _mapTrackingState = PositionTracking();
            } break;

            case CreationState.NotifySubscriber:
            {
                OnMapCreation?.Invoke(this, _trackedPositions);
                _mapTrackingState = CreationState.End;
            } break;

            default:
            {
                Thread.Sleep(10);
            } break;
        }
    }

    private CreationState InitialState()
    {
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

        var laps = ACCSharedMemory.Instance.PageFileGraphic.CompletedLaps;
        var position = ACCSharedMemory.Instance.PageFileGraphic.NormalizedCarPosition;

        if (laps != _completedLaps && position < 0.01)
        {
            _completedLaps = laps;
            _carNormPosition = position;
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

        var pos = pageGraphics.CarCoordinates[0];
        _trackedPositions.Add(new PointF(pos.X, pos.Z));

        if (_carNormPosition > pageGraphics.NormalizedCarPosition)
        {
            WriteMapToFile();
            return CreationState.NotifySubscriber;
        }

        _carNormPosition = pageGraphics.NormalizedCarPosition;
        return CreationState.TraceTrack;
    }

    private CreationState LoadMapFromFile()
    {
        var trackName = ACCSharedMemory.Instance.PageFileStatic.Track;
        string path = FileUtil.RaceElementTracks + trackName + ".bin";

        var fs = new FileStream(path, FileMode.Open);
        var br = new BinaryReader(fs);

        while (fs.Position < fs.Length)
        {
            PointF pos = new();

            pos.X = br.ReadSingle();
            pos.Y = br.ReadSingle();

            _trackedPositions.Add(pos);
        }

        br.Close();
        fs.Close();

        return CreationState.NotifySubscriber;
    }

    private void WriteMapToFile()
    {
        DirectoryInfo directory = new(FileUtil.RaceElementTracks);
        if (!directory.Exists) directory.Create();

        var trackName = ACCSharedMemory.Instance.PageFileStatic.Track;
        string path = FileUtil.RaceElementTracks + trackName + ".bin";

        var fs = new FileStream(path, FileMode.Create);
        var bw = new BinaryWriter(fs);

        foreach (var it in _trackedPositions)
        {
            bw.Write(it.X);
            bw.Write(it.Y);
        }

        bw.Close();
        fs.Close();
    }
}
