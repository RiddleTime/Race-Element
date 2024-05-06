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
    private bool _showCarNumber = false;
    private Bitmap _bitmap = null;

    private float _fontSize = 10;
    private float _dotSize = 15;

    private readonly SolidBrush _borderColor = new(Color.Black);

    private Color _mapColor = Color.WhiteSmoke;
    private float _mapThickness = 4;

    private SolidBrush _colorCarOthersLappedPlayer = new(Color.DarkOrange);
    private SolidBrush _colorCarPlayerLapperOthers = new(Color.SteelBlue);

    private SolidBrush _playerCarColor = new(Color.Red);
    private SolidBrush _defaultCarColor = new(Color.DarkGray);

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
        _playerCarColor = new(color);
        return this;
    }

    public TrackMapDrawing SetColorCarDefault(Color color)
    {
        _defaultCarColor = new(color);
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

    public TrackMapDrawing CreateBitmap(float width, float height, float margin)
    {
        int w = (int)(width + margin + 0.5f);
        int h = (int)(height + margin + 0.5f);

        _bitmap = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
        _bitmap.MakeTransparent();

        return this;
    }

    public Bitmap Draw(List<PointF> cars, List<PointF> track, TrackData broadCastTrackData)
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
        return DrawCars(cars, g, broadCastTrackData);
    }

    private Bitmap DrawCars(List<PointF> cars, Graphics g, TrackData broadCastTrackData)
    {
        int playerIdx = 0;
        using Font font = FontUtil.FontSegoeMono(_fontSize);

        var pageFileGraphic = ACCSharedMemory.Instance.PageFileGraphic;
        var playerCarData = EntryListTracker.Instance.GetCarData(pageFileGraphic.PlayerCarID);

        for (int i = 0; i < cars.Count; ++i)
        {
            if (pageFileGraphic.CarIds[i] == pageFileGraphic.PlayerCarID)
            {
                playerIdx = i;
                continue;
            }

            var car = cars[i];
            var color = _defaultCarColor;

            var idx = pageFileGraphic.CarIds[i];
            var currentCarData = EntryListTracker.Instance.GetCarData(idx);

            if (playerCarData != null && currentCarData != null)
            {
                var playerLaps = playerCarData.RealtimeCarUpdate.Laps;
                var otherLaps = currentCarData.RealtimeCarUpdate.Laps;

                var trackMeters = broadCastTrackData.TrackMeters;
                var otherTrackMeters = otherLaps * trackMeters + currentCarData.RealtimeCarUpdate.SplinePosition * trackMeters;
                var playerTrackMeters = playerLaps * trackMeters + playerCarData.RealtimeCarUpdate.SplinePosition * trackMeters;

                if (playerLaps > otherLaps && (playerTrackMeters - otherTrackMeters) >= trackMeters)
                {
                    color = _colorCarPlayerLapperOthers;
                }
                else if (otherLaps > playerLaps && (otherTrackMeters - playerTrackMeters) >= trackMeters)
                {
                    color = _colorCarOthersLappedPlayer;
                }
            }

            DrawCarsOnMap(car, g, color, font, currentCarData);
        }

        if (playerIdx < cars.Count)
        {
            DrawCarsOnMap(cars[playerIdx], g, _playerCarColor, font, playerCarData);
        }

        return _bitmap;
    }

    private void DrawCarsOnMap(PointF car, Graphics g, SolidBrush color, Font font, EntryListTracker.CarData carData)
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
}
