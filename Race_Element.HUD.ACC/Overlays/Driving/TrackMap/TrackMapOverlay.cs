using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;

using RaceElement.HUD.Overlay.Internal;
using RaceElement.Broadcast;
using RaceElement.Util;

using System.Collections.Generic;
using System.Drawing;
using System;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

#if DEBUG
[Overlay(Name = "Track Map",
    Description = "Shows a track map",
    OverlayCategory = OverlayCategory.Track,
    OverlayType = OverlayType.Drive,
    Authors = ["Andrei Jianu"]
)]
#endif
internal sealed class TrackMapOverlay : AbstractOverlay
{
    private  readonly TrackMapConfiguration _config = new();

    private TrackMapCreationJob _miniMapCreationJob;
    private List<PointF> _trackPositions = [];

    private BoundingBox _trackOriginalBoundingBox;
    private BoundingBox _trackBoundingBox;

    private readonly float _margin = 64.0f;
    private float _scale = 1.0f;

    private float _height = 1.0f;
    private float _width = 1.0f;

    public TrackMapOverlay(Rectangle rectangle) : base(rectangle, "Track Map")
    {
        RefreshRateHz = 6;
    }

    public override void BeforeStart()
    {
        if (IsPreviewing)
        {
            return;
        }

        _miniMapCreationJob = new TrackMapCreationJob()
        {
            IntervalMillis = 4,
        };

        _miniMapCreationJob.OnMapPositionsCallback += OnMapPositionsCallback;
        _miniMapCreationJob.Run();

        RaceSessionTracker.Instance.OnNewSessionStarted += OnNewSessionStart;
    }

    public override void BeforeStop()
    {
        if (IsPreviewing)
        {
            return;
        }

        _miniMapCreationJob?.CancelJoin();
        _miniMapCreationJob = null;

        _trackPositions.Clear();
        _trackPositions = [];
    }

    public override bool ShouldRender()
    {
        return base.ShouldRender() && _trackPositions.Count > 0 && EntryListTracker.Instance.Cars.Count > 0;
    }

    public override void Render(Graphics g)
    {
        var bitmap = CreateBitmapForCarsAndTrack();

        if (bitmap.Width != Width || bitmap.Height != Height)
        {
            Height = bitmap.Height;
            Width = bitmap.Width;
        }

        g.DrawImage(bitmap, 0, 0);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void OnNewSessionStart(object sender, DbRaceSession session)
    {
        if (_miniMapCreationJob != null)
        {
            _miniMapCreationJob.CancelJoin();
            _miniMapCreationJob = null;
        }

        _miniMapCreationJob = new TrackMapCreationJob()
        {
            IntervalMillis = 4,
        };

        _trackPositions.Clear();

        _miniMapCreationJob.OnMapPositionsCallback += OnMapPositionsCallback;
        _miniMapCreationJob.Run();
    }

    private void OnMapPositionsCallback(object sender, List<PointF> positions)
    {
        _miniMapCreationJob.Cancel();
        _trackOriginalBoundingBox = TrackMapTransform.GetBoundingBox(positions);

        {
            var scaled = TrackMapTransform.ScaleAndRotate(positions, _trackOriginalBoundingBox, 1, _config.Map.Rotation);
            var bounds = TrackMapTransform.GetBoundingBox(scaled);

            var width = (float)Math.Sqrt(Math.Pow(bounds.Right - bounds.Left, 2));
            _scale = _config.Map.MaxWidth / width;
        }

        var track = TrackMapTransform.ScaleAndRotate(positions, _trackOriginalBoundingBox, _scale, _config.Map.Rotation);
        var boundaries = TrackMapTransform.GetBoundingBox(track);

        for (var i = 0; i < track.Count; ++i)
        {
            var y = track[i].Y - boundaries.Bottom + _margin * 0.5f;
            var x = track[i].X - boundaries.Left + _margin * 0.5f;

            track[i] = new PointF(x, y);
        }

        _trackBoundingBox = boundaries;
        _trackPositions = track;

        _height = (float)Math.Sqrt(Math.Pow(_trackBoundingBox.Bottom - _trackBoundingBox.Top, 2));
        _width= (float)Math.Sqrt(Math.Pow(_trackBoundingBox.Right - _trackBoundingBox.Left, 2));

        if (!_config.Map.SavePreview)
        {
            return;
        }

        var pageFileStatic = ACCSharedMemory.Instance.PageFileStatic;
        if (pageFileStatic.Track.Length == 0) return;

        var minimap = CreateBitmapForCarsAndTrack(new CarRenderData(), _trackPositions);
        var path = FileUtil.RaceElementTracks + pageFileStatic.Track + ".jpg";
        minimap.Save(path);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private Bitmap CreateBitmapForCarsAndTrack()
    {
        CarRenderData carsOnTrack = new();

        foreach (var it in EntryListTracker.Instance.Cars)
        {
            CarOnTrack car = new();

            car.IsValidForBest = false;
            car.IsValid = false;

            if (it.Value.RealtimeCarUpdate.CurrentLap != null)
            {
                car.IsValidForBest = it.Value.RealtimeCarUpdate.CurrentLap.IsValidForBest;
                car.IsValid = !it.Value.RealtimeCarUpdate.CurrentLap.IsInvalid;
                car.Delta = it.Value.RealtimeCarUpdate.Delta;
            }

            if (it.Value.CarInfo != null)
            {
                car.RaceNumber = it.Value.CarInfo.RaceNumber.ToString();
            }

            var x = it.Value.RealtimeCarUpdate.WorldPosX;
            var y = it.Value.RealtimeCarUpdate.WorldPosY;

            car.Laps = it.Value.RealtimeCarUpdate.Laps;
            car.Coord = new PointF(x, y);
            car.Id = it.Key;

            car.Spline = it.Value.RealtimeCarUpdate.SplinePosition;
            car.Location = it.Value.RealtimeCarUpdate.CarLocation;
            car.Position = it.Value.RealtimeCarUpdate.Position;

            {
                car.Coord = TrackMapTransform.ScaleAndRotate(car.Coord, _trackOriginalBoundingBox, _scale, _config.Map.Rotation);
                car.Coord.Y = car.Coord.Y - _trackBoundingBox.Bottom + _margin * 0.5f;
                car.Coord.X = car.Coord.X - _trackBoundingBox.Left + _margin * 0.5f;
            }

            if (car.Id == ACCSharedMemory.Instance.PageFileGraphic.PlayerCarID)
            {
                carsOnTrack.Player = car;
            }
            else
            {
                carsOnTrack.Cars.Add(car);
            }
        }

        carsOnTrack.Cars.Sort((a, b) =>
        {
            if (a.Location != CarLocationEnum.Track && b.Location != CarLocationEnum.Track)
            {
                return 0;
            }

            if (a.Location != CarLocationEnum.Track)
            {
                return -1;
            }

            if (b.Location != CarLocationEnum.Track)
            {
                return 1;
            }

            return b.Position - a.Position;
        });

        return CreateBitmapForCarsAndTrack(carsOnTrack, _trackPositions);
    }

    private Bitmap CreateBitmapForCarsAndTrack(CarRenderData cars, List<PointF> track)
    {
        TrackMapDrawing drawer = new();
        drawer.SetDotSize(_config.Other.CarSize)
            .SetFontSize(_config.Other.FontSize);

        drawer.SetLappedThreshold(_config.Other.LappedThreshold)
            .SetTrackMeter(broadCastTrackData.TrackMeters);

        drawer.SetShowCarNumber(_config.Car.ShowCarNumber)
            .SetShowPitStop(_config.Car.ShowPitStop)
            .SetMapThickness(_config.Map.Thickness)
            .SetColorMap(_config.Map.Color);

        drawer.SetColorPlayer(_config.Car.PlayerColor)
            .SetColorLeader(_config.Car.LeaderColor)
            .SetColorCarDefault(_config.Car.DefaultColor)
            .SetColorImprovingLap(_config.Car.ImprovingLapColor)
            .SetColorPlayerLappedOthers(_config.Other.PlayerLappedOthersColor)
            .SetColorOthersLappedPlayer(_config.Other.OthersLappedPlayerColor);

        drawer.SetColorPitStop(_config.Car.PitStopColor)
            .SetColorPitStopWithDamage(_config.Car.PitStopWithDamageColor);

        drawer.CreateBitmap(_width, _height, _margin);
        return drawer.Draw(cars, track);
    }
}
