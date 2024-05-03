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

internal struct BoundingBox
{
    public float minX, maxX;
    public float minY, maxY;
}

public class TrackMapCreationJob : AbstractLoopJob
{
    private enum CreationState
    {
        Start,
        TraceTrack,
        ScaleAndRotateTrack,
        BitmapTrack,
        LoadFromFile,
        End
    }

    private readonly List<PointF> _mapPoints = new();
    private CreationState _mapTrackingState;

    public EventHandler<Bitmap> OnMapCreation;
    private Color _mapColor;

    private int _completedLaps;
    private int _prevPacketId;

    private float _rotation;
    private float _carNormPosition;
    private float _thickness;
    private float _scale;

    public TrackMapCreationJob()
    {
        _mapTrackingState = CreationState.Start;
        _mapColor = Color.DarkGray;

        _rotation = 0.1f;
        _thickness = 0.5f;
        _scale = 0.1f;

        _carNormPosition = ACCSharedMemory.Instance.PageFileGraphic.NormalizedCarPosition;
        _completedLaps = ACCSharedMemory.Instance.PageFileGraphic.CompletedLaps;
        _prevPacketId = ACCSharedMemory.Instance.PageFileGraphic.PacketId;

        Debug.WriteLine("[MAP TRACE] CreationState.Start");
    }

    public float Rotation
    {
        set { _rotation = value; }
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

            case CreationState.ScaleAndRotateTrack:
            {
                _mapTrackingState = TrackMapRescaleAndRotate();
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

        var pos = pageGraphics.CarCoordinates[0];
        _mapPoints.Add(new PointF(pos.X, pos.Z));

        if (_carNormPosition > pageGraphics.NormalizedCarPosition)
        {
            Debug.WriteLine("[MAP TRACE] CreationState.TraceTrack -> CreationState.RescaleTrack");
            WriteMapToFile();
            return CreationState.ScaleAndRotateTrack;
        }

        _carNormPosition = pageGraphics.NormalizedCarPosition;
        return CreationState.TraceTrack;
    }

    private CreationState TrackMapRescaleAndRotate()
    {
        var boundingBox = GetBoundingBox(_mapPoints);
        float centerX = (boundingBox.maxX + boundingBox.minX) * 0.5f;
        float centerY = (boundingBox.maxY + boundingBox.minY) * 0.5f;

        for (int i = 0; i < _mapPoints.Count; ++i)
        {
            var pos = _mapPoints[i];

            pos.X = (_mapPoints[i].X - centerX) * _scale;
            pos.Y = (_mapPoints[i].Y - centerY) * _scale;

            var x = pos.X * Math.Cos(Deg2Rad(_rotation)) - pos.Y * Math.Sin(Deg2Rad(_rotation));
            var y = pos.X * Math.Sin(Deg2Rad(_rotation)) + pos.Y * Math.Cos(Deg2Rad(_rotation));

            _mapPoints[i] = new PointF((float)x, (float)y);
        }

        Debug.WriteLine("[MAP TRACE] CreationState.RescaleTrack -> CreationState.BitmapTrack");
        return CreationState.BitmapTrack;
    }

    private CreationState TrackMapCreateBitmap()
    {
        var boundingBox = GetBoundingBox(_mapPoints);

        for (int i = 0; i < _mapPoints.Count; ++i)
        {
            float x = _mapPoints[i].X - boundingBox.minX;
            float y = _mapPoints[i].Y - boundingBox.minY;

            _mapPoints[i] = new PointF(x, y);
        }

        float w = (float)Math.Sqrt((boundingBox.maxX - boundingBox.minX) * (boundingBox.maxX - boundingBox.minX));
        float h = (float)Math.Sqrt((boundingBox.maxY - boundingBox.minY) * (boundingBox.maxY - boundingBox.minY));

        var track = new Bitmap((int)(w + 1.0f), (int)(h + 1.0f));
        track.MakeTransparent();

        var g = Graphics.FromImage(track);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingMode = CompositingMode.SourceOver;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.DrawLines(new Pen(_mapColor, _thickness), _mapPoints.ToArray());

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
            PointF pos = new();

            pos.X = br.ReadSingle();
            pos.Y = br.ReadSingle();

            _mapPoints.Add(pos);
        }

        br.Close();
        fs.Close();

        return CreationState.ScaleAndRotateTrack;
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
            bw.Write(it.Y);
        }

        bw.Close();
        fs.Close();
    }

    private double Deg2Rad(double deg)
    {
        return deg * (Math.PI / 180.0);
    }

    private BoundingBox GetBoundingBox(List<PointF> points)
    {
        BoundingBox result = new();

        result.maxX = -999999999999.0f;
        result.minX = 999999999999.0f;

        result.maxY = -999999999999.0f;
        result.minY = 999999999999.0f;

        foreach (var it in points)
        {
            result.maxX = Math.Max(result.maxX, it.X);
            result.minX = Math.Min(result.minX, it.X);

            result.maxY = Math.Max(result.maxY, it.Y);
            result.minY = Math.Min(result.minY, it.Y);
        }

        return result;
    }
}
