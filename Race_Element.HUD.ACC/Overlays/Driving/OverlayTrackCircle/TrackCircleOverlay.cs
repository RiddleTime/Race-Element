using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayTrackCircle
{
    [Overlay(Name = "Track Circle",
            Description = "Shows the progression of all cars on track in a track circle.")]
    internal sealed class TrackCircleOverlay : AbstractOverlay
    {
        private readonly TrackCircleConfiguration _config = new TrackCircleConfiguration();
        private sealed class TrackCircleConfiguration : OverlayConfiguration
        {
            public TrackCircleConfiguration() => AllowRescale = true;

            [ConfigGrouping("View", "Adjust track circle settings.")]
            public ViewingGroup Viewing { get; set; } = new ViewingGroup();
            public sealed class ViewingGroup
            {
                [ToolTip("Show the Track Circle HUD when spectating.")]
                public bool Spectator { get; set; } = true;
            }
        }
        private const int dimension = 400;

        private readonly Dictionary<int, float> _Progression = new Dictionary<int, float>();

        private CachedBitmap _background;
        private Font _font;

        public TrackCircleOverlay(Rectangle rectangle) : base(rectangle, "Track Circle")
        {
            RefreshRateHz = 3;
        }

        public override void BeforeStart()
        {
            int scaledDimension = (int)(dimension * Scale);
            Width = scaledDimension;
            Height = scaledDimension;

            _background = new CachedBitmap(scaledDimension, scaledDimension, g =>
            {
                float penWidth = 35f;
                Rectangle rect = new Rectangle((int)penWidth, (int)penWidth, (int)(scaledDimension - penWidth * 2), (int)(scaledDimension - penWidth * 2));

                using GraphicsPath gradientPath = new GraphicsPath();
                gradientPath.AddEllipse(0, 0, scaledDimension, scaledDimension);
                using PathGradientBrush pthGrBrush = new PathGradientBrush(gradientPath);
                pthGrBrush.CenterColor = Color.FromArgb(40, 0, 0, 0);
                pthGrBrush.SurroundColors = new Color[] { Color.FromArgb(220, 0, 0, 0) };

                using Pen pen = new Pen(pthGrBrush, penWidth);
                g.DrawArc(pen, rect, 0, 360);
            });
            _font = FontUtil.FontSegoeMono(10);
        }

        public override void BeforeStop()
        {
            _background?.Dispose();
        }

        public override bool ShouldRender()
        {
            if (_config.Viewing.Spectator && RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
                return true;

            return base.ShouldRender();
        }

        public override void Render(Graphics g)
        {
            _background?.Draw(g, (int)(5 * Scale), (int)(5 * Scale), (int)((dimension - 10) * Scale), (int)((dimension - 10) * Scale));

            PointF origin = new PointF(Width / Scale / 2f, Height / Scale / 2f);

            float penWidth = 35f;
            using Pen pen = new Pen(Brushes.White, penWidth);
            using Pen penRed = new Pen(Brushes.Red, penWidth);
            using Pen penYellow = new Pen(Brushes.Yellow, penWidth);
            Rectangle circleRect = new Rectangle((int)(penWidth), (int)(penWidth), (int)(dimension * Scale - penWidth * 2), (int)(dimension * Scale - penWidth * 2));
            g.DrawArc(pen, circleRect, 269, 2);

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            using StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var data = EntryListTracker.Instance.Cars;
            data.Sort((a, b) => a.Value.RealtimeCarUpdate.Position.CompareTo(b.Value.RealtimeCarUpdate.Position));
            data.Reverse();
            foreach (var item in data)
            {
                float carProgression = 0;
                if (!_Progression.TryGetValue(item.Key, out carProgression))
                    _Progression.Add(item.Key, carProgression);

                _Progression[item.Key] = item.Value.RealtimeCarUpdate.SplinePosition;
                carProgression = _Progression[item.Key];

                bool inPitlane = item.Value.RealtimeCarUpdate.CarLocation == Broadcast.CarLocationEnum.Pitlane;

                bool isSpeedZero = item.Value.RealtimeCarUpdate.Kmh < 33;

                int offset = (int)((inPitlane ? 120 : isSpeedZero ? 140 : 20) * Scale);

                Point center = PointOnCircle((dimension * Scale - offset) / 2f, -90 + 360 * carProgression, origin);
                float size = 26;
                Rectangle rect = new Rectangle((int)(center.X - size / 2f), (int)(center.Y - size / 2f), (int)size, (int)size);

                bool isViewingCar = broadCastRealTime.FocusedCarIndex == item.Key;
                g.DrawArc(inPitlane ? Pens.WhiteSmoke : isSpeedZero ? penYellow : isViewingCar ? penRed : pen, circleRect, -90 + 360 * carProgression - 1, .5f);
                using Brush brush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));

                g.FillEllipse(brush, rect);
                Brush textBrush = inPitlane ? Brushes.White : isSpeedZero ? Brushes.Yellow : Brushes.White;
                g.DrawStringWithShadow($"{item.Value.RealtimeCarUpdate.Position}", _font, textBrush, rect, stringFormat);
            }
        }

        public static Point PointOnCircle(float radius, float angleInDegrees, PointF origin)
        {
            // Convert from degrees to radians via multiplication by PI/180        
            float x = (float)(radius * Math.Cos(angleInDegrees * Math.PI / 180F)) + origin.X;
            float y = (float)(radius * Math.Sin(angleInDegrees * Math.PI / 180F)) + origin.Y;

            return new Point((int)x, (int)y);
        }
    }
}
