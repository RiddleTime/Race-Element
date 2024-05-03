using RaceElement.HUD.Overlay.Internal;
using RaceElement.Util;

using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

internal struct BoundingBox
{
    public float Left, Right;
    public float Bottom, Top;
}

#if DEBUG
[Overlay(Name = "Track Map",
    Description = "Shows a track map",
    OverlayCategory = OverlayCategory.Track,
    OverlayType = OverlayType.Drive)]
#endif
internal sealed class TrackMapOverlay : AbstractOverlay
{
    private  readonly TrackMapConfiguration _config = new();
    private TrackMapCreationJob _minimapCreationJob;

    private List<PointF> _trackPositions;
    private Bitmap _trackMiniMap;

    public TrackMapOverlay(Rectangle rectangle) : base(rectangle, "Track Map")
    {
        RefreshRateHz = 1;
    }

    public override void BeforeStart()
    {
        if (IsPreviewing)
        {
            return;
        }

        _minimapCreationJob = new TrackMapCreationJob()
        {
            IntervalMillis = 4,
        };

        _minimapCreationJob.OnMapCreation += OnMiniMapCreated;
        _minimapCreationJob.Run();
    }

    public override void BeforeStop()
    {
        if (IsPreviewing)
        {
            return;
        }

        _minimapCreationJob?.CancelJoin();
        _trackMiniMap?.Dispose();

        _trackPositions.Clear();
        _trackPositions = null;

        _minimapCreationJob = null;
        _trackMiniMap = null;
    }

    public override bool ShouldRender()
    {
        return _trackMiniMap != null;
    }

    public override void Render(Graphics g)
    {
        g.DrawImage(_trackMiniMap, new Point());
    }

    private void OnMiniMapCreated(object sender, List<PointF> positions)
    {
        _minimapCreationJob.Cancel();

        _trackPositions = positions;
        _trackMiniMap = CreateMiniMap(ScaleAndRotate(positions, _config.Map.Scale, _config.Map.Rotation));


        Height = _trackMiniMap.Height;
        Width = _trackMiniMap.Width;

        if (_config.Map.SavePreview && ACCSharedMemory.Instance.PageFileStatic.Track.Length != 0)
        {
            var trackName = ACCSharedMemory.Instance.PageFileStatic.Track.Trim() + ".jpg";
            string path = FileUtil.RaceElementTracks + trackName;
            _trackMiniMap.Save(path);
        }
    }

    private Bitmap CreateMiniMap(List<PointF> positions)
    {
        var boundingBox = GetBoundingBox(positions);
        List<PointF> pos = new();

        for (int i = 0; i < positions.Count; ++i)
        {
            float x = positions[i].X - boundingBox.Left;
            float y = positions[i].Y - boundingBox.Bottom;

            pos.Add(new PointF(x, y));
        }

        float w = (float)Math.Sqrt((boundingBox.Right - boundingBox.Left) * (boundingBox.Right - boundingBox.Left));
        float h = (float)Math.Sqrt((boundingBox.Top - boundingBox.Bottom) * (boundingBox.Top - boundingBox.Bottom));

        var track = new Bitmap((int)(w + 10.0f), (int)(h + 10.0f));
        track.MakeTransparent();

        var g = Graphics.FromImage(track);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingMode = CompositingMode.SourceOver;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.DrawLines(new Pen(_config.Map.Color, _config.Map.Thickness), pos.ToArray());

        return track;
    }

    private List<PointF> ScaleAndRotate(List<PointF> positions, float scale, float rotation)
    {
        var boundingBox = GetBoundingBox(positions);
        List<PointF> result = new();

        float centerX = (boundingBox.Right + boundingBox.Left) * 0.5f;
        float centerY = (boundingBox.Top + boundingBox.Bottom) * 0.5f;

        foreach (var it in positions)
        {
            PointF pos = new();

            pos.X = (it.X - centerX) * scale;
            pos.Y = (it.Y - centerY) * scale;

            var x = pos.X * Math.Cos(Double.DegreesToRadians(rotation)) - pos.Y * Math.Sin(Double.DegreesToRadians(rotation));
            var y = pos.X * Math.Sin(Double.DegreesToRadians(rotation)) + pos.Y * Math.Cos(Double.DegreesToRadians(rotation));

            result.Add(new PointF((float)x, (float)y));
        }

        return result;
    }

    private BoundingBox GetBoundingBox(List<PointF> positions)
    {
        BoundingBox result = new();

        if (positions.Count > 0)
        {
            result.Right = positions[0].X;
            result.Left = positions[0].X;

            result.Top = positions[0].Y;
            result.Bottom = positions[0].Y;
        }

        foreach (var it in positions)
        {
            result.Right = Math.Max(result.Right, it.X);
            result.Left = Math.Min(result.Left, it.X);

            result.Top = Math.Max(result.Top, it.Y);
            result.Bottom = Math.Min(result.Bottom, it.Y);
        }

        return result;
    }
}
