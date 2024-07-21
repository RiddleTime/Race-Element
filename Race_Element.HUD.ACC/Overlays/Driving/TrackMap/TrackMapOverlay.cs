using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;

using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;

using RaceElement.Broadcast;
using RaceElement.Util;

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using RaceElement.Data;

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
    private readonly TrackMapCache _mapCache = new();

    private TrackMapCreationJob _miniMapCreationJob;
    private List<TrackPoint> _trackPositions = [];
    private string _trackingProgress;

    private BoundingBox _trackOriginalBoundingBox;
    private BoundingBox _trackBoundingBox;

    private readonly float _margin = 64.0f;
    private float _scale = 0.15f;

    public TrackMapOverlay(Rectangle rectangle) : base(rectangle, "Track Map")
    {
        RefreshRateHz = _config.Others.RefreshInterval;

        _mapCache.Map = null;
        _mapCache.CarPlayer = TrackMapDrawer.CreateCircleWithOutline(_config.MapColors.Player, _config.Others.CarSize + 3, _config.Others.OutCircleSize, Color.Black);

        _mapCache.PitStopWithDamage = TrackMapDrawer.CreateCircleWithOutline(_config.MapColors.PitStopWithDamage, _config.Others.CarSize, _config.Others.OutCircleSize, Color.Black);
        _mapCache.PitStop = TrackMapDrawer.CreateCircleWithOutline(_config.MapColors.PitStop, _config.Others.CarSize, _config.Others.OutCircleSize, Color.Black);
    }

    public override void BeforeStart()
    {
        if (IsPreviewing)
        {
            return;
        }

        _miniMapCreationJob = new TrackMapCreationJob()
        {
            IntervalMillis = 1,
        };

        _miniMapCreationJob.OnMapPositionsCallback += OnMapPositionsCallback;
        _miniMapCreationJob.OnMapProgressCallback += OnMapProgressCallback;

        RaceSessionTracker.Instance.OnNewSessionStarted += OnNewSessionStart;
        _miniMapCreationJob.Run();
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
        return (base.ShouldRender() || RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex)) && (_trackingProgress != null || _mapCache.Map != null);
    }

    public override void Render(Graphics g)
    {
        if (_mapCache.Map != null)
        {
            RenderMap(g);
        }
        else if (_trackingProgress != null)
        {
            RenderTrackingProgress(g);
        }
    }

    private void RenderTrackingProgress(Graphics g)
    {
        using var font = FontUtil.FontSegoeMono(_config.Others.FontSize);
        using SolidBrush pen = new(Color.FromArgb(100, Color.Black));
        var size = g.MeasureString(_trackingProgress, font);

        g.FillRectangle(pen, 0, 0, size.Width, size.Height);
        g.DrawStringWithShadow(_trackingProgress, font, Color.White, new PointF());

        if ((int)size.Width != Width || (int)size.Height != Height)
        {
            Height = (int)size.Height;
            Width = (int)size.Width;
        }
    }

    private void RenderMap(Graphics g)
    {
        var carsOnTrack = GetCarsOnTrack();
        var bitmap = TrackMapDrawer.Draw(_trackPositions, carsOnTrack, _mapCache, _config, broadCastTrackData.TrackMeters);

        if (bitmap.Width != Width || bitmap.Height != Height)
        {
            Height = bitmap.Height;
            Width = bitmap.Width;
        }

        g.DrawImage(bitmap, 0, 0);
    }

    #region Event Listeners

    private void OnNewSessionStart(object sender, DbRaceSession session)
    {
        if (_miniMapCreationJob != null)
        {
            _miniMapCreationJob.CancelJoin();
            _miniMapCreationJob = null;
        }

        _miniMapCreationJob = new TrackMapCreationJob()
        {
            IntervalMillis = 1,
        };

        _miniMapCreationJob.OnMapPositionsCallback += OnMapPositionsCallback;
        _miniMapCreationJob.OnMapProgressCallback += OnMapProgressCallback;

        _trackPositions.Clear();
        _miniMapCreationJob.Run();
    }

    private void OnMapProgressCallback(object sender, string info)
    {
        _trackingProgress = info;
    }

    private void OnMapPositionsCallback(object sender, List<TrackPoint> positions)
    {
        var trackInfo = TrackInfo.Data.GetValueOrDefault(pageStatic.Track.ToLower(), new TrackInfo(0, 0, 0, 0));
        _scale = trackInfo.Scale * _config.General.ScaleFactor;

        _miniMapCreationJob.Cancel();
        _trackOriginalBoundingBox = TrackMapTransform.GetBoundingBox(positions);

        var track = TrackMapTransform.ScaleAndRotate(positions, _trackOriginalBoundingBox, _scale, _config.General.Rotation);
        var boundaries = TrackMapTransform.GetBoundingBox(track);

        for (var i = 0; i < track.Count; ++i)
        {
            var y = track[i].Y - boundaries.Bottom + _margin * 0.5f;
            var x = track[i].X - boundaries.Left + _margin * 0.5f;

            track[i] = new TrackPoint(track[i]) { X = x, Y = y };
        }

        List<TrackPoint> sector1 = new(), sector2 = new(), sector3 = new();
        _trackBoundingBox = boundaries;
        _trackPositions = track;

        {
            int i = 0;
            while (_trackPositions[i].Spline > 0.9f || _trackPositions[i].Spline < 0.0001)
            {
                ++i;
            }

            while (_trackPositions[i].Spline < trackInfo.Sector1End) {
                sector1.Add(_trackPositions[i]);
                ++i;
            }

            while (_trackPositions[i].Spline < trackInfo.Sector2End) {
                sector2.Add(_trackPositions[i]);
                ++i;
            }

            while (i < _trackPositions.Count) {
                sector3.Add(_trackPositions[i]);
                ++i;
            }
        }

        {
            var s1 = TrackMapDrawer.CreateLineFromPoints(_config.MapColors.MapSector1, _config.General.Thickness, _margin, sector1, _trackBoundingBox);
            var s2 = TrackMapDrawer.CreateLineFromPoints(_config.MapColors.MapSector2, _config.General.Thickness, _margin, sector2, _trackBoundingBox);
            var s3 = TrackMapDrawer.CreateLineFromPoints(_config.MapColors.MapSector3, _config.General.Thickness, _margin, sector3, _trackBoundingBox);
            _mapCache.Map = TrackMapDrawer.MixImages(s1, s2, s3, s1.Width, s1.Height);
        }

        Debug.WriteLine($"[MAP] {broadCastTrackData.TrackName} ({pageStatic.Track}) -> [S: {_scale:F3}] [L: {broadCastTrackData.TrackMeters:F3}] [P: {_trackPositions.Count}]");

        if (!_config.Others.SavePreview || pageStatic.Track.Length == 0)
        {
            return;
        }

        var path = FileUtil.RaceElementTracks + pageStatic.Track.ToLower() + ".jpg";
        _mapCache.Map.Save(path);
    }

    #endregion

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
;
                var carModel = ConversionFactory.GetCarModels(it.Value.CarInfo.CarModelType);
                car.CarClass = ConversionFactory.GetConversion(carModel).CarClass;
            }

            var x = it.Value.RealtimeCarUpdate.WorldPosX;
            var y = it.Value.RealtimeCarUpdate.WorldPosY;
            var spline = it.Value.RealtimeCarUpdate.SplinePosition;

            car.RacePosition = it.Value.RealtimeCarUpdate.Position.ToString();
            car.Laps = it.Value.RealtimeCarUpdate.Laps;
            car.Pos = new TrackPoint() { X = x, Y = y, Spline = spline };
            car.Id = it.Key;

            car.Spline = it.Value.RealtimeCarUpdate.SplinePosition;
            car.Kmh = it.Value.RealtimeCarUpdate.Kmh;

            car.Location = it.Value.RealtimeCarUpdate.CarLocation;
            car.Position = it.Value.RealtimeCarUpdate.Position;

            {
                car.Pos = TrackMapTransform.ScaleAndRotate(car.Pos, _trackOriginalBoundingBox, _scale, _config.General.Rotation);
                car.Pos.Y = car.Pos.Y - _trackBoundingBox.Bottom + _margin * 0.5f;
                car.Pos.X = car.Pos.X - _trackBoundingBox.Left + _margin * 0.5f;
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
