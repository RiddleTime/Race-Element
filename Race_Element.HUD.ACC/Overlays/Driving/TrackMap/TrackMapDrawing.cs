using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Drawing;

using System.Collections.Generic;
using System.Diagnostics;

using RaceElement.Data.ACC.EntryList;
using RaceElement.Broadcast;

using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

class TrackMapDrawing
{
    private readonly SolidBrush _colorValidForBest = new(Color.Purple);
    private readonly SolidBrush _colorCarLeader = new(Color.DarkGreen);
    private readonly SolidBrush _borderColor = new(Color.Black);

    private Bitmap _bitmap;

    private float _lappedDistanceThreshold = 100.0f;
    private float _trackMeters = 0.0f;

    private bool _showCarNumber;
    private bool _showPitStop;

    private float _fontSize = 10;
    private float _dotSize = 15;

    private Color _mapColor = Color.WhiteSmoke;
    private float _mapThickness = 4;

    private SolidBrush _colorCarOthersLappedPlayer = new(Color.DarkOrange);
    private SolidBrush _colorCarPlayerLapperOthers = new(Color.SteelBlue);

    private SolidBrush _colorCarPlayer = new(Color.Red);
    private SolidBrush _colorCarDefault = new(Color.DarkGray);

    private SolidBrush _colorPitStop = new(Color.Yellow);
    private SolidBrush _colorPitStopWithDamage = new(Color.MediumPurple);

    public TrackMapDrawing SetDotSize(float size)
    {
        _dotSize = size;
        return this;
    }

    public TrackMapDrawing SetFontSize(float size)
    {
        _fontSize = size;
        return this;
    }

    public TrackMapDrawing SetLappedThreshold(float threshold)
    {
        _lappedDistanceThreshold = threshold;
        return this;
    }

    public TrackMapDrawing SetTrackMeter(float meters)
    {
        _trackMeters = meters;
        return this;
    }

    public TrackMapDrawing SetShowCarNumber(bool show)
    {
        _showCarNumber = show;
        return this;
    }

    public TrackMapDrawing SetColorMap(Color color)
    {
        _mapColor = color;
        return this;
    }

    public TrackMapDrawing SetColorPlayer(Color color)
    {
        _colorCarPlayer = new(color);
        return this;
    }

    public TrackMapDrawing SetColorCarDefault(Color color)
    {
        _colorCarDefault = new(color);
        return this;
    }

    public TrackMapDrawing SetMapThickness(float thickness)
    {
        _mapThickness = thickness;
        return this;
    }

    public TrackMapDrawing SetColorPlayerLappedOthers(Color color)
    {
        _colorCarPlayerLapperOthers = new(color);
        return this;
    }

    public TrackMapDrawing SetColorOthersLappedPlayer(Color color)
    {
        _colorCarOthersLappedPlayer = new(color);
        return this;
    }

    public TrackMapDrawing SetShowPitStop(bool show)
    {
        _showPitStop = show;
        return this;
    }

    public TrackMapDrawing SetColorPitStop(Color color)
    {
        _colorPitStop = new(color);
        return this;
    }

    public TrackMapDrawing SetColorPitStopWithDamage(Color color)
    {
        _colorPitStopWithDamage = new(color);
        return this;
    }

    public TrackMapDrawing CreateBitmap(float width, float height, float margin)
    {
        int w = (int)(width + margin + 0.5f);
        int h = (int)(height + margin + 0.5f);

        _bitmap = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
        _bitmap.MakeTransparent();

        return this;
    }

    public Bitmap Draw(List<PointF> cars, List<int> ids, List<PointF> track)
    {
        var g = Graphics.FromImage(_bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingMode = CompositingMode.SourceOver;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.CompositingQuality = CompositingQuality.HighQuality;

        g.DrawLines(new Pen(_mapColor, _mapThickness), track.ToArray());
        return DrawCars(cars, ids, track, g);
    }

    private Bitmap DrawCars(List<PointF> cars, List<int> ids, List<PointF> track, Graphics g)
    {
        var sessionType = ACCSharedMemory.Instance.PageFileGraphic.SessionType;
        var playerCarId = ACCSharedMemory.Instance.PageFileGraphic.PlayerCarID;

        var playerCarData = EntryListTracker.Instance.GetCarData(playerCarId);
        using var font = FontUtil.FontSegoeMono(_fontSize);

        int playerIdx = 0;

        for (int i = 0; i < cars.Count; ++i)
        {
            if (ids[i] == playerCarId)
            {
                playerIdx = i;
                continue;
            }

            var color = _colorCarDefault;
            var currentCarData = EntryListTracker.Instance.GetCarData(ids[i]);

            if (sessionType == ACCSharedMemory.AcSessionType.AC_RACE)
            {
                color = GetRaceCarColor(playerCarData, currentCarData);
            }
            else
            {
                color = GetOtherSessionCarColor(playerCarData, currentCarData);
            }

            DrawCarOnMap(cars[i], g, color, font, currentCarData);
        }

        if (_showPitStop)
        {
            DrawPitStopOnMap(font, g, _colorPitStop, TrackMapPitPrediction.GetPitStop(track));
            DrawPitStopOnMap(font, g, _colorPitStopWithDamage, TrackMapPitPrediction.GetPitStopWithDamage(track));
        }

        DrawCarOnMap(cars[playerIdx], g, _colorCarPlayer, font, playerCarData);
        return _bitmap;
    }

    private void DrawCarOnMap(PointF car, Graphics g, SolidBrush color, Font font, EntryListTracker.CarData carData)
    {
        {
            var outBorder = 3.0f;

            car.X -= _dotSize * 0.5f;
            car.Y -= _dotSize * 0.5f;

            g.FillEllipse(_borderColor, car.X - outBorder * 0.5f, car.Y - outBorder * 0.5f, _dotSize, _dotSize);
            g.FillEllipse(color, car.X, car.Y, _dotSize - outBorder, _dotSize - outBorder);
        }

        if (_showCarNumber && carData != null && carData.CarInfo != null)
        {
            using SolidBrush pen = new (Color.FromArgb(100, Color.Black));
            var id = carData.CarInfo.RaceNumber.ToString();
            var size = g.MeasureString(id, font);

            car.X -= size.Width * 0.25f;
            car.Y -= size.Height;

            g.FillRectangle(pen, car.X, car.Y, size.Width, size.Height);
            g.DrawStringWithShadow(id, font, new SolidBrush(Color.WhiteSmoke), car);
        }
    }

    private void DrawPitStopOnMap(Font font, Graphics g, SolidBrush color, PitStop pitStop)
    {
        if (pitStop == null)
        {
            return;
        }

        var car = pitStop.Position;
        var outBorder = 3.0f;

        car.X -= _dotSize * 0.5f;
        car.Y -= _dotSize * 0.5f;

        g.FillEllipse(_borderColor, car.X - outBorder * 0.5f, car.Y - outBorder * 0.5f, _dotSize, _dotSize);
        g.FillEllipse(color, car.X, car.Y, _dotSize - outBorder, _dotSize - outBorder);

        string symbol = pitStop.Laps > 0 ? "+" : "P";
        g.DrawStringWithShadow(symbol, font, new SolidBrush(Color.Black), car);

        if (pitStop.Laps > 0)
        {
            using SolidBrush pen = new (Color.FromArgb(100, Color.Black));

            var laps = pitStop.Laps.ToString();
            var size = g.MeasureString(laps, font);

            car.X -= size.Width * 0.25f;
            car.Y -= size.Height;

            g.FillRectangle(pen, car.X, car.Y, size.Width, size.Height);
            g.DrawStringWithShadow(laps, font, new SolidBrush(Color.WhiteSmoke), car);
        }
    }

    private SolidBrush GetRaceCarColor(EntryListTracker.CarData playerCarData, EntryListTracker.CarData otherCarData)
    {
        if (playerCarData == null || otherCarData == null)
        {
            return _colorCarDefault;
        }

        if (otherCarData.RealtimeCarUpdate.Position == 1)
        {
            return _colorCarLeader;
        }

        var playerLaps = playerCarData.RealtimeCarUpdate.Laps;
        var otherLaps = otherCarData.RealtimeCarUpdate.Laps;

        var playerTrackMeters = playerLaps * _trackMeters + playerCarData.RealtimeCarUpdate.SplinePosition * _trackMeters;
        var otherTrackMeters = otherLaps * _trackMeters + otherCarData.RealtimeCarUpdate.SplinePosition * _trackMeters;

        if (playerLaps == otherLaps && otherLaps == 0)
        {
            // NOTE(Andrei): This is just to avoid nested if. As currently
            // there is threshold for the distance between cars, we don't
            // want to take into account the distance the first lap.
        }
        else if (playerTrackMeters == 0 || otherTrackMeters == 0)
        {
            if (playerLaps > otherLaps)
            {
                return _colorCarPlayerLapperOthers;
            }

            if (otherLaps > playerLaps)
            {
                return _colorCarOthersLappedPlayer;
            }
        }
        else if (playerLaps >= otherLaps && (playerTrackMeters - otherTrackMeters) >= (_trackMeters - _lappedDistanceThreshold))
        {
            return _colorCarPlayerLapperOthers;
        }
        else if (otherLaps >= playerLaps && (otherTrackMeters - playerTrackMeters) >= (_trackMeters - _lappedDistanceThreshold))
        {
            return _colorCarOthersLappedPlayer;
        }

        return _colorCarDefault;
    }

    private SolidBrush GetOtherSessionCarColor(EntryListTracker.CarData playerCarData, EntryListTracker.CarData otherCarData)
    {
        if (playerCarData == null || otherCarData == null)
        {
            return _colorCarDefault;
        }

        if (otherCarData.RealtimeCarUpdate.CarLocation != CarLocationEnum.Track)
        {
            return _colorCarDefault;
        }

        var playerLap = playerCarData.RealtimeCarUpdate.CurrentLap;
        var otherLap = otherCarData.RealtimeCarUpdate.CurrentLap;

        if (playerLap == null || otherLap == null)
        {
            return _colorCarDefault;
        }

        var otherIsValidForBest = otherLap.IsValidForBest;
        var playerIsValidLap = !playerLap.IsInvalid;
        var otherIsValidLap = !otherLap.IsInvalid;

        if (playerIsValidLap && !otherIsValidLap)
        {
            return _colorCarPlayerLapperOthers;
        }

        if (!playerIsValidLap && otherIsValidLap)
        {
            return otherIsValidForBest ? _colorValidForBest : _colorCarOthersLappedPlayer;
        }

        return otherIsValidForBest ? _colorValidForBest : _colorCarDefault;
    }
}
