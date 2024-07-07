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
    public static Bitmap CreateCircleWithOutline(Color color, float diameter, float outLineSize, Color outline)
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

        using SolidBrush outlineBrush = new(Color.FromArgb(200, outline));
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
            Color outlineColor = Color.Black;

            if (sessionType == ACCSharedMemory.AcSessionType.AC_RACE)
            {
                outlineColor = GetRaceCarOutlineColor(cars.Player, it, conf, trackMeters);
            }
            else
            {
                outlineColor = GetOtherSessionCarOutlineColor(it, conf);
            }

            var bitmap = CreateCircleWithOutline(GetCarBitmap(it.CarClass, conf), conf.Others.CarSize, conf.Others.OutCircleSize, outlineColor);
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

    private static Color GetCarBitmap(CarClasses cls, TrackMapConfiguration conf)
    {
        return cls switch
        {
            CarClasses.GT2 => conf.CarColors.GT2,
            CarClasses.GT3 => conf.CarColors.GT2,
            CarClasses.GT4 => conf.CarColors.GT3,
            CarClasses.CUP => conf.CarColors.CUP,
            CarClasses.TCX => conf.CarColors.TCX,
            CarClasses.CHL => conf.CarColors.CHL,
            CarClasses.ST  => conf.CarColors.ST,
            _ => conf.MapColors.Default
        };
    }

    private static void DrawCarOnMap(CarOnTrack car, Bitmap bitmap, TrackMapConfiguration conf, Graphics g, Font font)
    {
        {
            PointF pos = car.Pos.ToPointF();

            pos.X -= bitmap.Width * 0.5f;
            pos.Y -= bitmap.Height * 0.5f;

            g.DrawImage(bitmap, pos);
        }

        if (conf.General.CarLabel == TrackMapLabelText.None)
        {
            return;
        }

        string label = string.Empty;

        switch (conf.General.CarLabel)
        {
            case TrackMapLabelText.CarNumber:
            {
                label = car.RaceNumber;
            } break;

            case TrackMapLabelText.Position:
            {
                label = car.RacePosition;
            } break;
        }

        if (label == string.Empty)
        {
            return;
        }

        {
            using SolidBrush pen = new(Color.FromArgb(120, 0, 0, 0));

            var size = g.MeasureString(label, font);
            PointF pos = car.Pos.ToPointF();

            pos.Y -= bitmap.Height * 0.5f + size.Height;
            pos.X -= size.Width * 0.5f;

            var clr = Color.FromArgb(255, 255 - pen.Color.R, 255 - pen.Color.G, 255 - pen.Color.B);
            g.FillRectangle(pen, pos.X, pos.Y, size.Width, size.Height);

            using SolidBrush textBrush = new(clr);
            g.DrawStringWithShadow(label, font, textBrush, pos);
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

    private static Color GetRaceCarOutlineColor(CarOnTrack player, CarOnTrack other, TrackMapConfiguration conf, float trackMeters)
    {
        if (player == null || other == null)
        {
            return conf.MapColors.Default;
        }

        if (other.Kmh < conf.General.KmhThreshold)
        {
            return Color.Yellow;
        }

        if (other.Position == 1)
        {
            return conf.MapColors.Leader;
        }

        var playerTrackMeters = player.Laps * trackMeters + player.Spline * trackMeters;
        var otherTrackMeters = other.Laps * trackMeters + other.Spline * trackMeters;

        var trackThreshold = trackMeters - conf.General.LappedThreshold;
        var distanceDiff = playerTrackMeters - otherTrackMeters;

        if (distanceDiff >= trackThreshold)
        {
            return conf.MapColors.PlayerLappedOthers;
        }

        if (Math.Abs(distanceDiff) >= trackThreshold)
        {
            return conf.MapColors.OthersLappedPlayer;
        }

        return Color.Black;
    }

    private static Color GetOtherSessionCarOutlineColor(CarOnTrack car, TrackMapConfiguration config)
    {
        if (car.Location != CarLocationEnum.Track)
        {
            return config.MapColors.Default;
        }

        if (car.Kmh <= config.General.KmhThreshold)
        {
            return Color.Yellow;
        }

        if (!car.IsValid)
        {
            return config.MapColors.PlayerLappedOthers;
        }

        if (car.Delta < 0 && car.IsValidForBest)
        {
            return config.MapColors.ImprovingLap;
        }

        return Color.Black;
    }
}
