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

    private readonly float _outLineBorder = 2.0f;
    private readonly float _margin = 64.0f;

    private float _trackLength;
    private float _scale;

    public TrackMapOverlay(Rectangle rectangle) : base(rectangle, "Track Map")
    {
        RefreshRateHz = _config.Others.RefreshInterval;
        _trackLength = 0.0f;
        _scale = 0.15f;

        _mapCache.Map = null;
        _mapCache.YellowFlag = TrackMapDrawer.CreateCircleWithOutline(Color.Yellow, _config.Others.CarSize, _outLineBorder);

        _mapCache.OthersLappedPlayer = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.OthersLappedPlayer, _config.Others.CarSize, _outLineBorder);
        _mapCache.PlayerLapperOthers = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.PlayerLappedOthers, _config.Others.CarSize, _outLineBorder);

        _mapCache.PitStopWithDamage = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.PitStopWithDamage, _config.Others.CarSize, _outLineBorder);
        _mapCache.PitStop = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.PitStop, _config.Others.CarSize, _outLineBorder);

        _mapCache.CarDefault = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.Default, _config.Others.CarSize, _outLineBorder);
        _mapCache.CarPlayer = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.Player, _config.Others.CarSize + 3, _outLineBorder);

        _mapCache.ValidForBest = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.ImprovingLap, _config.Others.CarSize, _outLineBorder);
        _mapCache.Leader = TrackMapDrawer.CreateCircleWithOutline(_config.Colors.Leader, _config.Others.CarSize, _outLineBorder);
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
        var shouldRender = (_trackingProgress != null) || (_trackPositions.Count > 0 && EntryListTracker.Instance.Cars.Count > 0);
        return base.ShouldRender() && shouldRender;
    }

    public override void Render(Graphics g)
    {
        if (_trackingProgress != null && _trackPositions.Count == 0)
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

            return;
        }

        var carsOnTrack = GetCarsOnTrack();
        var bitmap = TrackMapDrawer.Draw(_trackPositions, carsOnTrack, _mapCache, _config, _trackLength);

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
        var trackInfo = TrackInfo.Data.GetValueOrDefault(pageStatic.Track.ToLower(), new TrackInfo(0, 0, 0));
        _scale = trackInfo.Scale * _config.General.ScaleFactor;
        _trackLength = trackInfo.LengthMeters;

        _miniMapCreationJob.Cancel();
        _trackOriginalBoundingBox = TrackMapTransform.GetBoundingBox(positions);

        var track = TrackMapTransform.ScaleAndRotate(positions, _trackOriginalBoundingBox, _scale, _config.General.Rotation);
        var boundaries = TrackMapTransform.GetBoundingBox(track);

        for (var i = 0; i < track.Count; ++i)
        {
            var y = track[i].Y - boundaries.Bottom + _margin * 0.5f;
            var x = track[i].X - boundaries.Left + _margin * 0.5f;

            track[i] = new(track[i]) { X = x, Y = y };
        }

        _trackBoundingBox = boundaries;
        _trackPositions = track;

        _mapCache.Map = TrackMapDrawer.CreateLineFromPoints(_config.Colors.Map, _config.General.Thickness, _margin, _trackPositions, _trackBoundingBox);
        Debug.WriteLine("[MAP] " + pageStatic.Track.ToLower() + " -> [" + _scale + "] [" + _trackLength + "] [" + _trackPositions.Count + "]");

        if (!_config.Others.SavePreview)
        {
            return;
        }

        var pageFileStatic = ACCSharedMemory.Instance.PageFileStatic;
        if (pageFileStatic.Track.Length == 0) return;

        var path = FileUtil.RaceElementTracks + pageFileStatic.Track.ToLower() + ".jpg";
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
            }

            var x = it.Value.RealtimeCarUpdate.WorldPosX;
            var y = it.Value.RealtimeCarUpdate.WorldPosY;
            var spline = it.Value.RealtimeCarUpdate.SplinePosition;

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
