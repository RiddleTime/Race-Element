using System;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
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
        RescaleTrack,
        BitmapTrack,
        LoadFromFile,
        End
    }

    private List<ACCSharedMemory.StructVector3> _mapPoints = new();
    private CreationState _mapTrackingState;

    public EventHandler<Bitmap> OnMapCreation;
    private Color _mapColor;

    private int _completedLaps;
    private int _prevPacketId;

    private float _carNormPosition;
    private float _thickness;
    private float _scale;

    public TrackMapCreationJob()
    {
        _mapTrackingState = CreationState.Start;
        _mapColor = Color.DarkGray;
        _thickness = 0.5f;
        _scale = 0.1f;

        _carNormPosition = ACCSharedMemory.Instance.PageFileGraphic.NormalizedCarPosition;
        _completedLaps = ACCSharedMemory.Instance.PageFileGraphic.CompletedLaps;
        _prevPacketId = ACCSharedMemory.Instance.PageFileGraphic.PacketId;

        Debug.WriteLine("[MAP TRACE] CreationState.Start");
    }

    public float Scale
    {
        set { _scale = value; }
    }

    public float Thickness
    {
        set { _thickness = value; }
    }

    public Color MapColor
    {
        set { _mapColor = value; }
    }

    public override void RunAction()
    {
        switch (_mapTrackingState)
        {
            case CreationState.Start:
            {
                var trackName = ACCSharedMemory.Instance.PageFileStatic.Track;
                string path = FileUtil.RaceElementTracks + trackName + ".bin";

                if (trackName.Length > 0 && File.Exists(path))
                {
                    Debug.WriteLine("[MAP TRACE] CreationState.Start -> CreationState.LoadFromFile");
                    _mapTrackingState = CreationState.LoadFromFile;
                    return;
                }

                if (!AccProcess.IsRunning)
                {
                    return;
                }

                int laps = ACCSharedMemory.Instance.PageFileGraphic.CompletedLaps;
                float position = ACCSharedMemory.Instance.PageFileGraphic.NormalizedCarPosition;

                if (laps != _completedLaps && position < 0.01)
                {
                    _completedLaps = laps;
                    _carNormPosition = position;
                    _mapTrackingState = CreationState.TraceTrack;
                    Debug.WriteLine("[MAP TRACE] CreationState.Start -> CreationState.TraceTrack");
                }
            } break;

            case CreationState.LoadFromFile:
            {
                _mapTrackingState = LoadMapFromFile();
            }break;

            case CreationState.TraceTrack:
            {
                _mapTrackingState = TrackMapPosition();
            }break;

            case CreationState.RescaleTrack:
            {
                _mapTrackingState = TrackMapRescale();
            }break;

            case CreationState.BitmapTrack:
            {
                _mapTrackingState = TrackMapCreateBitmap();
            }break;

            default:
            {
                Thread.Sleep(100);
            } break;
        }
    }

    private CreationState TrackMapPosition()
    {
        var pageGraphics = ACCSharedMemory.Instance.PageFileGraphic;

        if (pageGraphics.PacketId == _prevPacketId)
        {
            return CreationState.TraceTrack;
        }

        _prevPacketId = pageGraphics.PacketId;

        if (!pageGraphics.IsValidLap)
        {
            _mapPoints.Clear();
            return CreationState.Start;
        }

        _mapPoints.Add(pageGraphics.CarCoordinates[0]);

        if (_carNormPosition > pageGraphics.NormalizedCarPosition)
        {
            Debug.WriteLine("[MAP TRACE] CreationState.TraceTrack -> CreationState.RescaleTrack");
            WriteMapToFile();
            return CreationState.RescaleTrack;
        }

        _carNormPosition = pageGraphics.NormalizedCarPosition;
        return CreationState.TraceTrack;
    }

    private CreationState TrackMapRescale()
    {
        float maxX = -999999999999.0f;
        float minX = 999999999999.0f;

        float maxZ = -999999999999.0f;
        float minZ = 999999999999.0f;

        foreach (var pos in _mapPoints)
        {
            maxX = Math.Max(maxX, pos.X);
            minX = Math.Min(minX, pos.X);

            maxZ = Math.Max(maxZ, pos.Z);
            minZ = Math.Min(minZ, pos.Z);
        }

        float centerX = (maxX + minX) * 0.5f;
        float centerZ = (maxZ + minZ) * 0.5f;

        for (int i = 0; i < _mapPoints.Count; ++i)
        {
            var pos = _mapPoints[i];

            pos.X = (_mapPoints[i].X - centerX) * _scale;
            pos.Y = 0.0f;
            pos.Z = (_mapPoints[i].Z - centerZ) * _scale;

            _mapPoints[i] = pos;
        }

        Debug.WriteLine("[MAP TRACE] CreationState.RescaleTrack -> CreationState.BitmapTrack");
        return CreationState.BitmapTrack;
    }

    private CreationState TrackMapCreateBitmap()
    {
        var points = new List<Point>();

        float maxX = -999999999999.0f;
        float minX = 999999999999.0f;

        float maxZ = -999999999999.0f;
        float minZ = 999999999999.0f;

        foreach (var pos in _mapPoints)
        {
            maxX = Math.Max(maxX, pos.X);
            minX = Math.Min(minX, pos.X);

            maxZ = Math.Max(maxZ, pos.Z);
            minZ = Math.Min(minZ, pos.Z);
        }

        foreach (var it in _mapPoints)
        {
            float x = it.X - minX;
            float z = it.Z - minZ;

            points.Add(new Point((int)x, (int)z));
        }

        float w = (float)Math.Sqrt((maxX - minX) * (maxX - minX));
        float h = (float)Math.Sqrt((maxZ - minZ) * (maxZ - minZ));

        var track = new Bitmap((int)(w + 1.0f), (int)(h + 1.0f));
        track.MakeTransparent();

        var g = Graphics.FromImage(track);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingMode = CompositingMode.SourceOver;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.DrawLines(new Pen(_mapColor, _thickness), points.ToArray());

        Debug.WriteLine("[MAP TRACE] CreationState.BitmapTrack -> CreationState.End");
        OnMapCreation?.Invoke(null, track);
        return CreationState.End;
    }

    private CreationState LoadMapFromFile()
    {
        var trackName = ACCSharedMemory.Instance.PageFileStatic.Track;
        string path = FileUtil.RaceElementTracks + trackName + ".bin";

        var fs = new FileStream(path, FileMode.Open);
        var br = new BinaryReader(fs);

        while (fs.Position < fs.Length)
        {
            ACCSharedMemory.StructVector3 pos = new();

            pos.X = br.ReadSingle();
            pos.Z = br.ReadSingle();

            _mapPoints.Add(pos);
        }

        br.Close();
        fs.Close();

        return CreationState.RescaleTrack;
    }

    private void WriteMapToFile()
    {
        DirectoryInfo directory = new(FileUtil.RaceElementTracks);
        if (!directory.Exists) directory.Create();

        var trackName = ACCSharedMemory.Instance.PageFileStatic.Track;
        string path = FileUtil.RaceElementTracks + trackName + ".bin";

        var fs = new FileStream(path, FileMode.Create);
        var bw = new BinaryWriter(fs);

        foreach (var it in _mapPoints)
        {
            bw.Write(it.X);
            bw.Write(it.Z);
        }

        bw.Close();
        fs.Close();
    }
}
