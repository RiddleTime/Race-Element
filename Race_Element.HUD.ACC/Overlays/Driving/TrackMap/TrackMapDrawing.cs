using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Drawing;
using System;

using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Broadcast;
using static RaceElement.Data.SetupConverter;
using static RaceElement.HUD.ACC.Overlays.Driving.TrackMap.TrackMapConfiguration;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

public static class TrackMapDrawer
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

        using SolidBrush backgroundBrush = new(color);
        g.FillEllipse(backgroundBrush, outLineSize * 0.5f, outLineSize * 0.5f, diameter, diameter);
        using SolidBrush outlineBrush = new(Color.FromArgb(200, 0, 0, 0));
        using Pen outlinePen = new(outlineBrush, outLineSize);
        g.DrawEllipse(outlinePen, outLineSize * 0.5f, outLineSize * 0.5f, diameter, diameter);

        return bitmap;
    }

    public static Bitmap CreateLineFromPoints(Color color, float thickness, float margin, List<TrackPoint> points, BoundingBox boundaries)
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

        List<PointF> tmpTrack = [];
        foreach (var it in points) tmpTrack.Add(new PointF(it.X, it.Y));

        using Pen linePen = new(color, thickness);
        g.DrawLines(linePen, tmpTrack.ToArray());
        return bitmap;
    }

    public static Bitmap Draw(List<TrackPoint> track, CarRenderData cars, TrackMapCache cache, TrackMapConfiguration conf, float trackMeters)
    {
        // TODO: prevent NullReferenceException when cache.Map is null
        var result = new Bitmap(cache.Map);
        using var g = Graphics.FromImage(result);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingMode = CompositingMode.SourceOver;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.CompositingQuality = CompositingQuality.HighQuality;

        var sessionType = ACCSharedMemory.Instance.PageFileGraphic.SessionType;
        using var font = FontUtil.FontSegoeMono(conf.Others.FontSize);

        foreach (var it in cars.Cars)
        {
            Bitmap bitmap;

            if (sessionType == ACCSharedMemory.AcSessionType.AC_RACE)
            {
                bitmap = GetRaceCarBitmap(cars.Player, it, cache, conf, trackMeters);
            }
            else
            {
                bitmap = GetOtherSessionCarBitmap(it, cache, conf);
            }

            DrawCarOnMap(it, bitmap, conf, g, font);
        }

        if (conf.Pitstop.ShowPitStop)
        {
            var pitTimeMs = (conf.Pitstop.FixedPitTime + conf.Pitstop.PitAdditionalTime) * 1000;
            DrawPitStopOnMap(font, g, cache.PitStop, TrackMapPitPrediction.GetPitStop(track, pitTimeMs));
            DrawPitStopOnMap(font, g, cache.PitStopWithDamage, TrackMapPitPrediction.GetPitStopWithDamage(track, pitTimeMs));
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
            PointF pos = car.Pos.ToPointF();

            pos.X -= bitmap.Width * 0.5f;
            pos.Y -= bitmap.Height * 0.5f;

            g.DrawImage(bitmap, pos);
        }

        if (conf.General.CarLabel != TrackMapLabelText.None)
        {

            string label = string.Empty;

            switch (conf.General.CarLabel)
            {
                case TrackMapLabelText.CarNumber:
                    {
                        if (car.RaceNumber != null) label = car.RaceNumber;
                        break;
                    }
                case TrackMapLabelText.Position:
                    {
                        if (car.RacePosition != null) label = car.RacePosition;
                        break;
                    }

            }

            if (label == string.Empty) return;

            using SolidBrush pen = new(Color.FromArgb(130, Color.Black));
            var size = g.MeasureString(label, font);
            PointF pos = car.Pos.ToPointF();

            pos.Y -= bitmap.Height * 0.5f + size.Height;
            pos.X -= size.Width * 0.5f;

            g.FillRectangle(pen, pos.X, pos.Y, size.Width, size.Height);

            using SolidBrush textBrush = new(Color.WhiteSmoke);

            g.DrawStringWithShadow(label, font, textBrush, pos);

            int defaultAlpha = 220;
            using SolidBrush lineBrush = new(car.CarClass switch
            {
                CarClasses.GT2 => Color.FromArgb(defaultAlpha, 255, 0, 0),
                CarClasses.GT3 => Color.FromArgb(0, 0, 0, 0),
                CarClasses.GT4 => Color.FromArgb(defaultAlpha, 24, 24, 72),
                CarClasses.CUP => Color.FromArgb(defaultAlpha, 30, 61, 26),
                CarClasses.TCX => Color.FromArgb(defaultAlpha, 0, 96, 136),
                CarClasses.CHL => Color.FromArgb(defaultAlpha, 112, 110, 0),
                CarClasses.ST => Color.FromArgb(defaultAlpha, 0, 96, 136),
                _ => Color.WhiteSmoke,
            });
            using Pen linePen = new(lineBrush, 2f);
            g.DrawLine(linePen, new PointF(pos.X + 1, pos.Y + size.Height - 2), new(pos.X - 1 + size.Width, pos.Y + size.Height - 2));
        }
    }

    private static void DrawPitStopOnMap(Font font, Graphics g, Bitmap bitmap, PitStop pitStop)
    {
        if (pitStop == null)
        {
            return;
        }

        {
            PointF p = pitStop.Position.ToPointF();

            p.X -= bitmap.Width * 0.5f;
            p.Y -= bitmap.Height * 0.5f;

            g.DrawImage(bitmap, p);
        }

        {
            string symbol = pitStop.Damage ? "+" : "P";
            SizeF textSize = g.MeasureString(symbol, font);

            var pos = pitStop.Position.ToPointF();
            pos.X -= textSize.Width * 0.5f;
            pos.Y -= textSize.Height * 0.5f;

            using SolidBrush textBrush = new(Color.Black);
            g.DrawStringWithShadow(symbol, font, textBrush, pos);
        }

        if (pitStop.Laps > 0)
        {
            var pos = pitStop.Position.ToPointF();

            var laps = pitStop.Laps.ToString();
            var size = g.MeasureString(laps, font);

            pos.Y -= bitmap.Height * 0.5f + size.Height;
            pos.X -= bitmap.Width * 0.5f;

            using SolidBrush backgroundBrush = new(Color.FromArgb(100, Color.Black));
            g.FillRectangle(backgroundBrush, pos.X, pos.Y, size.Width, size.Height);
            using SolidBrush textBrush = new(Color.WhiteSmoke);
            g.DrawStringWithShadow(laps, font, textBrush, pos);
        }
    }

    private static Bitmap GetRaceCarBitmap(CarOnTrack player, CarOnTrack other, TrackMapCache cache, TrackMapConfiguration conf, float trackMeters)
    {
        if (player == null || other == null)
        {
            return cache.CarDefault;
        }

        if (other.Kmh < conf.General.KmhThreshold)
        {
            return cache.YellowFlag;
        }

        if (other.Position == 1)
        {
            return cache.Leader;
        }

        var playerTrackMeters = player.Laps * trackMeters + player.Spline * trackMeters;
        var otherTrackMeters = other.Laps * trackMeters + other.Spline * trackMeters;

        var trackThreshold = trackMeters - conf.General.LappedThreshold;
        var distanceDiff = playerTrackMeters - otherTrackMeters;

        if (distanceDiff >= trackThreshold)
        {
            return cache.PlayerLapperOthers;
        }

        if (Math.Abs(distanceDiff) >= trackThreshold)
        {
            return cache.OthersLappedPlayer;
        }

        return cache.CarDefault;
    }

    private static Bitmap GetOtherSessionCarBitmap(CarOnTrack car, TrackMapCache cache, TrackMapConfiguration config)
    {
        if (car.Location != CarLocationEnum.Track)
        {
            return cache.CarDefault;
        }

        if (car.Kmh <= config.General.KmhThreshold)
        {
            return cache.YellowFlag;
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
