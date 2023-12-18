using RaceElement.Data.ACC.EntryList;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayTrackBar
{
    [Overlay(Name = "Track Bar",
             Description = "A bar displaying a flat and zoomed in version of the Track Circle HUD.")]
    internal sealed class TrackBarOverlay : AbstractOverlay
    {

        private readonly TrackBarConfiguration _config = new TrackBarConfiguration();
        private sealed class TrackBarConfiguration : OverlayConfiguration
        {
            public TrackBarConfiguration() { AllowRescale = true; }

            [ConfigGrouping("Bar", "Adjust things like fidelity on the track bar.")]
            public BarGrouping Bar { get; set; } = new BarGrouping();
            public sealed class BarGrouping
            {
                [ToolTip("Adjust the visible range (5 up to 50%) of the track.")]
                [IntRange(5, 50, 1)]
                public int Range { get; set; } = 10;
            }
        }


        private Rectangle BarRect;
        private CachedBitmap _cachedBackground;
        private float _range;
        private Font font;

        public TrackBarOverlay(Rectangle rectangle) : base(rectangle, "Track Bar")
        {
            RefreshRateHz = 10;
        }

        public override void BeforeStart()
        {
            _range = _config.Bar.Range / 100f;

            BarRect = new Rectangle(0, 0, 500, 100);
            font = FontUtil.FontSegoeMono(10);

            _cachedBackground = new CachedBitmap(BarRect.Width, BarRect.Height, g =>
            {
                g.FillRoundedRectangle(Brushes.Black, BarRect, 4);
            });

            Width = BarRect.Width;
            Height = BarRect.Height;
        }

        public override void BeforeStop()
        {
            _cachedBackground?.Dispose();
        }

        public override void Render(Graphics g)
        {
            _cachedBackground?.Draw(g);


            if (EntryListTracker.Instance.Cars.Count == 0) return;

            var spectatingCar = EntryListTracker.Instance.Cars.First(x => x.Key == broadCastRealTime.FocusedCarIndex);
            float spectatingPosition = spectatingCar.Value.RealtimeCarUpdate.SplinePosition;

            float halfRange = _range / 2f;
            float minSpline = spectatingPosition - halfRange;
            float maxSpline = spectatingPosition + halfRange;

            bool adjustedUp = false;
            if (minSpline < 0)
            {
                minSpline += 1;
                maxSpline += 1;
                adjustedUp = true;
            }

            var data = EntryListTracker.Instance.Cars.Where(x =>
            {
                float pos = x.Value.RealtimeCarUpdate.SplinePosition;

                if (adjustedUp && pos < 0.5)
                    pos += 1;
                if (!adjustedUp && pos < minSpline)
                    pos += 1;

                return pos < maxSpline && pos > minSpline;
            });


            //Debug.WriteLine($"Found {data.Count()} cars in range\nRange: {minSpline:F2} - {maxSpline:F2}");
            foreach (var entry in data)
            {
                float pos = entry.Value.RealtimeCarUpdate.SplinePosition;
                if (adjustedUp && pos < 0.5) pos += 1;
                if (pos < minSpline) pos += 1;
                float correctedPos = maxSpline - pos;
                bool isSpectatingCar = broadCastRealTime.FocusedCarIndex == entry.Key;

                float correctedPercentage = (correctedPos * 100) / _range / 100;
                if (isSpectatingCar) correctedPercentage = 0.5f;
                //Debug.WriteLine($"pos:{pos} --> cor:{correctedPos} --> perc:{correctedPercentage:F3}");

                int x = BarRect.Width - (int)(BarRect.Width * correctedPercentage);

                bool isInPits = entry.Value.RealtimeCarUpdate.CarLocation == Broadcast.CarLocationEnum.Pitlane;

                Pen pen = isSpectatingCar ? Pens.Red : isInPits ? Pens.Cyan : Pens.White;
                if (!isInPits && !isSpectatingCar && entry.Value.RealtimeCarUpdate.Kmh < 33)
                    pen = Pens.Yellow;
                g.DrawLine(pen, new Point(x, 0), new Point(x, BarRect.Height));
                if (!isSpectatingCar)
                    g.DrawStringWithShadow($"{entry.Value.RealtimeCarUpdate.Position}", font, Color.White, new Point(x, BarRect.Height / 2));

                //Debug.WriteLine($"{entry.Value.RealtimeCarUpdate.SplinePosition:F2}");
            }

        }
    }
}
