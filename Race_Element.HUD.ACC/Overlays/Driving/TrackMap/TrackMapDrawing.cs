using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Drawing;
using System;

using System.Collections.Generic;
using RaceElement.Broadcast;

using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

class CarOnTrack
{
    public string RaceNumber;

    public CarLocationEnum Location;
    public PointF Coord;

    public bool IsValidForBest;
    public bool IsValid;

    public float Spline;
    public int Position;

    public int Delta;
    public int Laps;
    public int Id;
}

class CarRenderData
{
    public List<CarOnTrack> Cars = [];
    public CarOnTrack Player;
}

class TrackMapDrawing
{
    private readonly SolidBrush _borderColor = new(Color.Black);
    private Bitmap _bitmap;

    private SolidBrush _colorValidForBest = new(Color.SeaGreen);
    private SolidBrush _colorCarLeader = new(Color.SeaGreen);

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

    private SolidBrush _colorPitStop = new(Color.MediumOrchid);
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

    public TrackMapDrawing SetMapThickness(float thickness)
    {
        _mapThickness = thickness;
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

    public TrackMapDrawing SetShowPitStop(bool show)
    {
        _showPitStop = show;
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

    public TrackMapDrawing SetColorLeader(Color color)
    {
        _colorCarLeader = new(color);
        return this;
    }

    public TrackMapDrawing SetColorImprovingLap(Color color)
    {
        _colorValidForBest = new(color);
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

    public Bitmap Draw(CarRenderData cars, List<PointF> track)
    {
        var g = Graphics.FromImage(_bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingMode = CompositingMode.SourceOver;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.CompositingQuality = CompositingQuality.HighQuality;

        g.DrawLines(new Pen(_mapColor, _mapThickness), track.ToArray());
        return DrawCars(cars, track, g);
    }

    private Bitmap DrawCars(CarRenderData cars, List<PointF> track, Graphics g)
    {
        var sessionType = ACCSharedMemory.Instance.PageFileGraphic.SessionType;
        using var font = FontUtil.FontSegoeMono(_fontSize);

        foreach (var it in cars.Cars)
        {
            SolidBrush color;

            if (sessionType == ACCSharedMemory.AcSessionType.AC_RACE)
            {
                color = GetRaceCarColor(cars.Player, it);
            }
            else
            {
                color = GetOtherSessionCarColor(it);
            }

            DrawCarOnMap(it, g, color, font);
        }

        if (_showPitStop)
        {
            DrawPitStopOnMap(font, g, _colorPitStop, TrackMapPitPrediction.GetPitStop(track));
            DrawPitStopOnMap(font, g, _colorPitStopWithDamage, TrackMapPitPrediction.GetPitStopWithDamage(track));
        }

        if (cars.Player != null)
        {
            // Note(Andrei): To avoid crash when render the track preview
            DrawCarOnMap(cars.Player, g, _colorCarPlayer, font);
        }

        return _bitmap;
    }

    private void DrawCarOnMap(CarOnTrack car, Graphics g, SolidBrush color, Font font)
    {
        {
            var outBorder = 3.0f;
            PointF pos = car.Coord;

            pos.X -= _dotSize * 0.5f;
            pos.Y -= _dotSize * 0.5f;

            g.FillEllipse(_borderColor, pos.X - outBorder * 0.5f, pos.Y - outBorder * 0.5f, _dotSize, _dotSize);
            g.FillEllipse(color, pos.X, pos.Y, _dotSize - outBorder, _dotSize - outBorder);
        }

        if (_showCarNumber && car.RaceNumber != null)
        {
            using SolidBrush pen = new (Color.FromArgb(100, Color.Black));
            var size = g.MeasureString(car.RaceNumber, font);
            PointF pos = car.Coord;

            pos.Y -= _dotSize * 0.5f + size.Height;
            pos.X -= _dotSize * 0.5f;

            g.FillRectangle(pen, pos.X, pos.Y, size.Width, size.Height);
            g.DrawStringWithShadow(car.RaceNumber, font, new SolidBrush(Color.WhiteSmoke), pos);
        }
    }

    private void DrawPitStopOnMap(Font font, Graphics g, SolidBrush color, PitStop pitStop)
    {
        if (pitStop == null)
        {
            return;
        }

        string symbol = pitStop.Laps > 0 ? "+" : "P";
        SizeF textSize = g.MeasureString(symbol, font);

        var car = pitStop.Position;
        var outBorder = 3.0f;

        car.X -= _dotSize * 0.5f;
        car.Y -= _dotSize * 0.5f;

        g.FillEllipse(_borderColor, car.X - outBorder * 0.5f, car.Y - outBorder * 0.5f, _dotSize, _dotSize);
        g.FillEllipse(color, car.X, car.Y, _dotSize - outBorder, _dotSize - outBorder);

        car.Y += Math.Abs(_dotSize - textSize.Height) * 0.5f;
        car.X += Math.Abs(_dotSize - textSize.Width) * 0.5f;

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

    private SolidBrush GetRaceCarColor(CarOnTrack player, CarOnTrack other)
    {
        if (player == null || other == null)
        {
            return _colorCarDefault;
        }

        if (other.Position == 1)
        {
            return _colorCarLeader;
        }

        var playerLaps = player.Laps;
        var otherLaps = other.Laps;

        var playerTrackMeters = playerLaps * _trackMeters + player.Spline * _trackMeters;
        var otherTrackMeters = otherLaps * _trackMeters + other.Spline * _trackMeters;

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

    private SolidBrush GetOtherSessionCarColor(CarOnTrack car)
    {
        if (car.Location != CarLocationEnum.Track)
        {
            return _colorCarDefault;
        }

        if (!car.IsValid)
        {
            return _colorCarPlayerLapperOthers;
        }

        if (car.Delta < 0 && car.IsValidForBest)
        {
            return _colorValidForBest;
        }

        return _colorCarDefault;
    }
}
