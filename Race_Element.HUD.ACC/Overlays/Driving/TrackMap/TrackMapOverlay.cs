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

    private readonly float _margin = 24.0f;

    public TrackMapOverlay(Rectangle rectangle) : base(rectangle, "Track Map")
    {
        RefreshRateHz = 30;
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

        _minimapCreationJob.OnMapPositionsCallback += OnMapPositionsCallback;
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
        g.DrawImage(_trackMiniMap, 0, 0);
        g.DrawImage(CreateBitmapForCarsOnTrack(), 0, 0);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private Bitmap CreateBitmapForCarsOnTrack()
    {
        var pageFileGraphic = ACCSharedMemory.Instance.PageFileGraphic;
        List<PointF> cars = new();

        for (int i = 0; i < pageFileGraphic.ActiveCars; ++i)
        {
            float x = pageFileGraphic.CarCoordinates[i].X;
            float y = pageFileGraphic.CarCoordinates[i].Z;

            cars.Add(new PointF(x, y));
        }

        var boundaries = GetBoundingBox(_trackPositions);
        cars = ScaleAndRotate(cars, boundaries, _config.Map.Scale, _config.Map.Rotation);

        var track = ScaleAndRotate(_trackPositions, boundaries, _config.Map.Scale, _config.Map.Rotation);
        boundaries = GetBoundingBox(track);

        return CreateBitmapForCarsOnTrack(cars, track, boundaries);
    }

    private Bitmap CreateBitmapForCarsOnTrack(List<PointF> cars, List<PointF> track, BoundingBox boundaries)
    {
        for (int i = 0; i < cars.Count; ++i)
        {
            float x = cars[i].X - boundaries.Left + _margin * 0.5f;
            float y = cars[i].Y - boundaries.Bottom + _margin * 0.5f;

            cars[i] = new PointF(x, y);
        }

        for (int i = 0; i < track.Count; ++i)
        {
            float x = track[i].X - boundaries.Left + _margin * 0.5f;
            float y = track[i].Y - boundaries.Bottom + _margin * 0.5f;

            track[i] = new PointF(x, y);
        }

        float w = (float)Math.Sqrt((boundaries.Right - boundaries.Left) * (boundaries.Right - boundaries.Left));
        float h = (float)Math.Sqrt((boundaries.Top - boundaries.Bottom) * (boundaries.Top - boundaries.Bottom));

        var carsOnTrack = new Bitmap((int)(w + _margin), (int)(h + _margin));
        carsOnTrack.MakeTransparent();

        var g = Graphics.FromImage(carsOnTrack);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingMode = CompositingMode.SourceOver;
        g.CompositingQuality = CompositingQuality.HighQuality;

        for (int i = 0; i < cars.Count; ++i)
        {
            Color color = _config.Car.OthersColor;
            var pageFileGraphic = ACCSharedMemory.Instance.PageFileGraphic;

            if (pageFileGraphic.CarIds[i] == pageFileGraphic.PlayerCarID)
            {
                color = _config.Car.PlayerColor;
            }

            var it = FindClosedTrackPoint(cars[i], track);
            //var it = cars[i];
            g.FillEllipse(new SolidBrush(color), it.X, it.Y, _config.Car.Scale, _config.Car.Scale);

        }

        return carsOnTrack;
    }

    private PointF FindClosedTrackPoint(PointF car, List<PointF> track)
    {
        if (track.Count == 0)
        {
            return car;
        }

        PointF result = car;
        var distance = Math.Pow(car.X - track[0].X, 2) + Math.Pow(car.Y - track[0].Y, 2);

        foreach (var it in track)
        {
            var currentDistance = Math.Pow(car.X - it.X, 2) + Math.Pow(car.Y - it.Y, 2);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                result = it;
            }
        }

        return distance > 5 ? car : result;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void OnMapPositionsCallback(object sender, List<PointF> positions)
    {
        var pageFileStatic = ACCSharedMemory.Instance.PageFileStatic;
        _minimapCreationJob.Cancel();

        _trackPositions = positions;
        _trackMiniMap = CreateMiniMap(ScaleAndRotate(positions, _config.Map.Scale, _config.Map.Rotation));

        Height = _trackMiniMap.Height;
        Width = _trackMiniMap.Width;

        if (!_config.Map.SavePreview)
        {
            return;
        }

        if (pageFileStatic.Track.Length == 0)
        {
            return;
        }

        string path = FileUtil.RaceElementTracks + pageFileStatic + ".jpg";
        _trackMiniMap.Save(path);
    }

    private Bitmap CreateMiniMap(List<PointF> positions)
    {
        var boundingBox = GetBoundingBox(positions);
        List<PointF> pos = new();

        for (int i = 0; i < positions.Count; ++i)
        {
            float x = positions[i].X - boundingBox.Left + _margin * 0.5f;
            float y = positions[i].Y - boundingBox.Bottom + _margin * 0.5f;

            pos.Add(new PointF(x, y));
        }

        float w = (float)Math.Sqrt((boundingBox.Right - boundingBox.Left) * (boundingBox.Right - boundingBox.Left));
        float h = (float)Math.Sqrt((boundingBox.Top - boundingBox.Bottom) * (boundingBox.Top - boundingBox.Bottom));

        var track = new Bitmap((int)(w + _margin), (int)(h + _margin));
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
        var boundings = GetBoundingBox(positions);
        return ScaleAndRotate(positions, boundings, scale, rotation);
    }

    private List<PointF> ScaleAndRotate(List<PointF> positions, BoundingBox boundings, float scale, float rotation)
    {
        var rot = Double.DegreesToRadians(rotation);
        float centerX = (boundings.Right + boundings.Left) * 0.5f;
        float centerY = (boundings.Top + boundings.Bottom) * 0.5f;

        List<PointF> result = new();
        foreach (var it in positions)
        {
            PointF pos = new();

            pos.X = (it.X - centerX) * scale;
            pos.Y = (it.Y - centerY) * scale;

            var x = pos.X * Math.Cos(rot) - pos.Y * Math.Sin(rot);
            var y = pos.X * Math.Sin(rot) + pos.Y * Math.Cos(rot);

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
