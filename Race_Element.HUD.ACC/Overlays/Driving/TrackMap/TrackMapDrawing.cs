using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Drawing;

using System.Collections.Generic;
using System.Diagnostics;

using RaceElement.Data.ACC.EntryList;
using RaceElement.Broadcast.Structs;

using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

class TrackMapDrawing
{
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
        if (_bitmap == null)
        {
            Debug.WriteLine("TrackMapDrawing.Draw Please call TrackMapDrawing.CreateBitmap before draw");
            return null;
        }

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
        int playerIdx = 0;
        using var font = FontUtil.FontSegoeMono(_fontSize);

        var playerCarId = ACCSharedMemory.Instance.PageFileGraphic.PlayerCarID;
        var playerCarData = EntryListTracker.Instance.GetCarData(playerCarId);

        for (int i = 0; i < cars.Count; ++i)
        {
            if (ids[i] == playerCarId)
            {
                playerIdx = i;
                continue;
            }

            var color = _colorCarDefault;
            var currentCarData = EntryListTracker.Instance.GetCarData(ids[i]);

            if (playerCarData != null && currentCarData != null)
            {
                var playerLaps = playerCarData.RealtimeCarUpdate.Laps;
                var otherLaps = currentCarData.RealtimeCarUpdate.Laps;

                var otherTrackMeters = otherLaps * _trackMeters + currentCarData.RealtimeCarUpdate.SplinePosition * _trackMeters;
                var playerTrackMeters = playerLaps * _trackMeters + playerCarData.RealtimeCarUpdate.SplinePosition * _trackMeters;

                if (playerLaps == otherLaps && otherLaps == 0)
                {
                    // NOTE(Andrei): This is just to avoid nested if. As currently
                    // there is threshold for the distance between cars, we don't
                    // want to take into account the distance the first lap.
                }
                else if (playerLaps >= otherLaps && (playerTrackMeters - otherTrackMeters) >= (_trackMeters - _lappedDistanceThreshold))
                {
                    color = _colorCarPlayerLapperOthers;
                }
                else if (otherLaps >= playerLaps && (otherTrackMeters - playerTrackMeters) >= (_trackMeters - _lappedDistanceThreshold))
                {
                    color = _colorCarOthersLappedPlayer;
                }
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
}
