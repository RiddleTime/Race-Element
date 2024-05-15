using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;

using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;

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
    private readonly TrackMapConfiguration _config = new();
    private TrackMapCache _mapCache = new();

    private TrackMapCreationJob _miniMapCreationJob;
    private List<PointF> _trackPositions = [];
    private string _trackingProgress;

    private BoundingBox _trackOriginalBoundingBox;
    private BoundingBox _trackBoundingBox;

    private readonly float _outLineBorder = 2.0f;
    private readonly float _margin = 64.0f;

    private float _scale = 1.0f;

    public TrackMapOverlay(Rectangle rectangle) : base(rectangle, "Track Map")
    {
        RefreshRateHz = _config.General.RefreshInterval;

        _mapCache.Map = null;
        _mapCache.YellowFlag = TrackMapDrawer.CreateCircleWithOutline(Color.Yellow, _config.General.CarSize, _outLineBorder);

        _mapCache.OthersLappedPlayer = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.OthersLappedPlayer, _config.General.CarSize, _outLineBorder);
        _mapCache.PlayerLapperOthers = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.PlayerLappedOthers, _config.General.CarSize, _outLineBorder);

        _mapCache.PitStopWithDamage = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.PitStopWithDamage, _config.General.CarSize, _outLineBorder);
        _mapCache.PitStop = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.PitStop, _config.General.CarSize, _outLineBorder);

        _mapCache.CarDefault = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.Default, _config.General.CarSize, _outLineBorder);
        _mapCache.CarPlayer = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.Player, _config.General.CarSize, _outLineBorder);

        _mapCache.ValidForBest = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.ImprovingLap, _config.General.CarSize, _outLineBorder);
        _mapCache.Leader = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.Leader, _config.General.CarSize, _outLineBorder);
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
        _miniMapCreationJob.OnMapProgressCallback += OnMapProgressCallback;
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
        var shouldRender = (_trackingProgress != null) || (_trackPositions.Count > 0 && EntryListTracker.Instance.Cars.Count > 0);
        return base.ShouldRender() && shouldRender;
    }

    public override void Render(Graphics g)
    {
        if (_trackingProgress != null && _trackPositions.Count == 0)
        {
            using var font = FontUtil.FontSegoeMono(_config.General.FontSize);
            var size = g.MeasureString(_trackingProgress, font);

            g.DrawStringWithShadow(_trackingProgress, font, Color.White, new PointF());

            if ((int)size.Width != Width || (int)size.Height != Height)
            {
                Height = (int)size.Height;
                Width = (int)size.Width;
            }

            return;
        }

        var carsOnTrack = GetCarsOnTrack();
        var bitmap = TrackMapDrawer.Draw(_trackPositions, carsOnTrack, _mapCache, _config, broadCastTrackData.TrackMeters);

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
        _miniMapCreationJob.OnMapProgressCallback += OnMapProgressCallback;
        _miniMapCreationJob.Run();
    }

    private void OnMapProgressCallback(object sender, string info)
    {
        _trackingProgress = info;
    }

    private void OnMapPositionsCallback(object sender, List<PointF> positions)
    {
        _miniMapCreationJob.Cancel();
        _trackOriginalBoundingBox = TrackMapTransform.GetBoundingBox(positions);

        {
            var scaled = TrackMapTransform.ScaleAndRotate(positions, _trackOriginalBoundingBox, 1, _config.General.Rotation);
            var bounds = TrackMapTransform.GetBoundingBox(scaled);

            var width = (float)Math.Sqrt(Math.Pow(bounds.Right - bounds.Left, 2));
            _scale = _config.General.MaxWidth / width;
        }

        var track = TrackMapTransform.ScaleAndRotate(positions, _trackOriginalBoundingBox, _scale, _config.General.Rotation);
        var boundaries = TrackMapTransform.GetBoundingBox(track);

        for (var i = 0; i < track.Count; ++i)
        {
            var y = track[i].Y - boundaries.Bottom + _margin * 0.5f;
            var x = track[i].X - boundaries.Left + _margin * 0.5f;

            track[i] = new PointF(x, y);
        }

        _trackBoundingBox = boundaries;
        _trackPositions = track;

        _mapCache.Map = TrackMapDrawer.CreateLineFromPoints(_config.Colors.Map, _config.General.Thickness, _margin, _trackPositions, _trackBoundingBox);

        if (!_config.General.SavePreview)
        {
            return;
        }

        var pageFileStatic = ACCSharedMemory.Instance.PageFileStatic;
        if (pageFileStatic.Track.Length == 0) return;

        var path = FileUtil.RaceElementTracks + pageFileStatic.Track.ToLower() + ".jpg";
        _mapCache.Map.Save(path);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private CarRenderData GetCarsOnTrack()
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
                car.Coord = TrackMapTransform.ScaleAndRotate(car.Coord, _trackOriginalBoundingBox, _scale, _config.General.Rotation);
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

        return carsOnTrack;
    }
}
