using RaceElement.Data.ACC.EntryList;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;

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
        }
        private const int dimension = 300;

        private readonly Dictionary<int, float> _Progression = new Dictionary<int, float>();

        private CachedBitmap _background;
        private Font _font;

        public TrackCircleOverlay(Rectangle rectangle) : base(rectangle, "Track Circle")
        {
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
                using Pen pen = new Pen(Brushes.Black, penWidth);
                g.DrawArc(pen, rect, 0, 360);
            });
            _font = FontUtil.FontSegoeMono(11);
        }

        public override void BeforeStop()
        {
            _background?.Dispose();
            _font?.Dispose();
        }

        public override void Render(Graphics g)
        {
            _background?.Draw(g, 5, 5, (int)((dimension - 10) * Scale), (int)((dimension - 10) * Scale));

            PointF origin = new PointF(Width / Scale / 2f, Height / Scale / 2f);

            float penWidth = 40f;
            using Pen pen = new Pen(Brushes.White, penWidth);
            g.DrawArc(pen, new Rectangle((int)penWidth, (int)penWidth, (int)(dimension - penWidth * 2), (int)(dimension - penWidth * 2)), 269, 2);

            using StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            foreach (var item in EntryListTracker.Instance.Cars)
            {
                float carProgression = 0;
                if (!_Progression.TryGetValue(item.Key, out carProgression))
                    _Progression.Add(item.Key, carProgression);

                _Progression[item.Key] = item.Value.RealtimeCarUpdate.SplinePosition;
                carProgression = _Progression[item.Key];

                Point center = PointOnCircle((dimension - 80) * Scale / 2, -90 + 360 * carProgression, origin);
                float size = 30;
                Rectangle rect = new Rectangle((int)(center.X - size / 2f), (int)(center.Y - size / 2f), (int)size, (int)size);
                g.DrawArc(item.Value.RealtimeCarUpdate.CarLocation == Broadcast.CarLocationEnum.Pitlane ? Pens.Red : Pens.White, rect, 0, 360);

                g.DrawStringWithShadow($"{item.Value.RealtimeCarUpdate.Position}", _font, Brushes.White, rect, stringFormat);
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
