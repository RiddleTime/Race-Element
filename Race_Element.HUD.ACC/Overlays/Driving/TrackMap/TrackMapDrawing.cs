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

class TrackMapCache
{
    public Bitmap OthersLappedPlayer;
    public Bitmap PlayerLapperOthers;

    public Bitmap CarDefault;
    public Bitmap CarPlayer;

    public Bitmap PitStopWithDamage;
    public Bitmap PitStop;

    public Bitmap ValidForBest;
    public Bitmap Leader;

    public Bitmap Map;
}

class TrackMapDrawer
{
    public static Bitmap CreateCircleWithOutline(Color color, float diameter, float outLineSize)
    {
        var w = diameter + outLineSize + 1.5f;
        var h = diameter + outLineSize + 1.5f;

        var bitmap = new Bitmap((int)w, (int)h, PixelFormat.Format32bppPArgb);
        bitmap.MakeTransparent();

        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingMode = CompositingMode.SourceOver;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.CompositingQuality = CompositingQuality.HighQuality;

        g.FillEllipse(new SolidBrush(color), outLineSize * 0.5f, outLineSize * 0.5f, diameter, diameter);
        g.DrawEllipse(new Pen(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), outLineSize), outLineSize * 0.5f, outLineSize * 0.5f, diameter, diameter);

        return bitmap;
    }

    public static Bitmap CreateLineFromPoints(Color color, float thickness, float margin, List<PointF> points, BoundingBox boundaries)
    {
        var w = Math.Sqrt(Math.Pow(boundaries.Right - boundaries.Left, 2)) + margin + 1.5;
        var h = Math.Sqrt(Math.Pow(boundaries.Bottom - boundaries.Top, 2)) + margin + 1.5;

        var bitmap = new Bitmap((int)w, (int)h, PixelFormat.Format32bppPArgb);
        bitmap.MakeTransparent();

        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingMode = CompositingMode.SourceOver;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.CompositingQuality = CompositingQuality.HighQuality;

        g.DrawLines(new Pen(color, thickness), points.ToArray());
        return bitmap;
    }

    public static Bitmap Draw(List<PointF> track,CarRenderData cars, TrackMapCache cache, TrackMapConfiguration conf, float trackMeters)
    {

        var result = new Bitmap(cache.Map);
        using var g = Graphics.FromImage(result);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingMode = CompositingMode.SourceOver;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.CompositingQuality = CompositingQuality.HighQuality;

        var sessionType = ACCSharedMemory.Instance.PageFileGraphic.SessionType;
        using var font = FontUtil.FontSegoeMono(conf.General.FontSize);

        foreach (var it in cars.Cars)
        {
            Bitmap bitmap;

            if (sessionType == ACCSharedMemory.AcSessionType.AC_RACE)
            {
                bitmap = GetRaceCarBitmap(cars.Player, it, cache, conf, trackMeters);
            }
            else
            {
                bitmap = GetOtherSessionCarBitmap(it, cache);
            }

            DrawCarOnMap(it, bitmap, conf, g, font);
        }

        if (conf.General.ShowPitStop)
        {
            DrawPitStopOnMap(font, g, cache.PitStop, TrackMapPitPrediction.GetPitStop(track));
            DrawPitStopOnMap(font, g, cache.PitStopWithDamage, TrackMapPitPrediction.GetPitStopWithDamage(track));
        }

        if (cars.Player != null)
        {
            // Note(Andrei): To avoid crash when render the track preview
            DrawCarOnMap(cars.Player, cache.CarPlayer, conf, g, font);
        }

        return result;
    }

    private static void DrawCarOnMap(CarOnTrack car, Bitmap bitmap, TrackMapConfiguration conf, Graphics g, Font font)
    {
        {
            PointF pos = car.Coord;

            pos.X -= bitmap.Width * 0.5f;
            pos.Y -= bitmap.Height * 0.5f;

            g.DrawImage(bitmap, pos);
        }

        if (conf.General.ShowCarNumber && car.RaceNumber != null)
        {
            using SolidBrush pen = new (Color.FromArgb(100, Color.Black));
            var size = g.MeasureString(car.RaceNumber, font);
            PointF pos = car.Coord;

            pos.Y -= bitmap.Height * 0.5f + size.Height;
            pos.X -= bitmap.Width * 0.5f;

            g.FillRectangle(pen, pos.X, pos.Y, size.Width, size.Height);
            g.DrawStringWithShadow(car.RaceNumber, font, new SolidBrush(Color.WhiteSmoke), pos);
        }
    }

    private static void DrawPitStopOnMap(Font font, Graphics g, Bitmap bitmap, PitStop pitStop)
    {
        if (pitStop == null)
        {
            return;
        }

        {
            PointF p = pitStop.Position;

            p.X -= bitmap.Width * 0.5f;
            p.Y -= bitmap.Height * 0.5f;

            g.DrawImage(bitmap, p);
        }

        {
            string symbol = pitStop.Damage ? "+" : "P";
            SizeF textSize = g.MeasureString(symbol, font);

            var pos = pitStop.Position;
            pos.X -= textSize.Width * 0.5f;
            pos.Y -= textSize.Height * 0.5f;

            g.DrawStringWithShadow(symbol, font, new SolidBrush(Color.Black), pos);
        }

        if (pitStop.Laps > 0)
        {
            using SolidBrush color = new (Color.FromArgb(100, Color.Black));
            var pos = pitStop.Position;

            var laps = pitStop.Laps.ToString();
            var size = g.MeasureString(laps, font);

            pos.Y -= bitmap.Height * 0.5f + size.Height;
            pos.X -= bitmap.Width * 0.5f;

            g.FillRectangle(color, pos.X, pos.Y, size.Width, size.Height);
            g.DrawStringWithShadow(laps, font, new SolidBrush(Color.WhiteSmoke), pos);
        }
    }

    private static Bitmap GetRaceCarBitmap(CarOnTrack player, CarOnTrack other, TrackMapCache cache, TrackMapConfiguration conf, float trackMeters)
    {
        if (player == null || other == null)
        {
            return cache.CarDefault;
        }

        if (other.Position == 1)
        {
            return cache.Leader;
        }

        var playerLaps = player.Laps;
        var otherLaps = other.Laps;

        var playerTrackMeters = playerLaps * trackMeters + player.Spline * trackMeters;
        var otherTrackMeters = otherLaps * trackMeters + other.Spline * trackMeters;

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
                return cache.PlayerLapperOthers;
            }

            if (otherLaps > playerLaps)
            {
                return cache.OthersLappedPlayer;
            }
        }
        else if (playerLaps >= otherLaps && (playerTrackMeters - otherTrackMeters) >= (trackMeters - conf.General.LappedThreshold))
        {
            return cache.PlayerLapperOthers;
        }
        else if (otherLaps >= playerLaps && (otherTrackMeters - playerTrackMeters) >= (trackMeters - conf.General.LappedThreshold))
        {
            return cache.OthersLappedPlayer;
        }

        return cache.CarDefault;
    }

    private static Bitmap GetOtherSessionCarBitmap(CarOnTrack car, TrackMapCache cache)
    {
        if (car.Location != CarLocationEnum.Track)
        {
            return cache.CarDefault;
        }

        if (!car.IsValid)
        {
            return cache.PlayerLapperOthers;
        }

        if (car.Delta < 0 && car.IsValidForBest)
        {
            return cache.ValidForBest;
        }

        return cache.CarDefault;
    }
}
