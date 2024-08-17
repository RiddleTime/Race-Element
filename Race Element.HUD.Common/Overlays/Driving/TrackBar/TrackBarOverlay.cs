using RaceElement.Data.Common.SimulatorData;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Text;

namespace RaceElement.HUD.Common.Overlays.Driving.OverlayTrackBar;

[Overlay(Name = "Track Bar",
         Description = "A bar displaying a flat and zoomed in version of the Track Circle HUD.",
Authors = ["Reinier Klarenberg"])]
internal sealed class TrackBarOverlay : CommonAbstractOverlay
{
    private readonly TrackBarConfiguration _config = new();
    private sealed class TrackBarConfiguration : OverlayConfiguration
    {
        public TrackBarConfiguration() { GenericConfiguration.AllowRescale = true; }

        [ConfigGrouping("View", "Adjust track circle settings.")]
        public ViewingGroup Viewing { get; init; } = new ViewingGroup();
        public sealed class ViewingGroup
        {
            [ToolTip("Show the Track Circle HUD when spectating.")]
            public bool Spectator { get; init; } = true;
        }

        [ConfigGrouping("Bar", "Adjust things like fidelity on the track bar.")]
        public BarGrouping Bar { get; init; } = new BarGrouping();
        public sealed class BarGrouping
        {
            [ToolTip("Adjust the visible range (5 up to 50%) of the track.")]
            [IntRange(5, 50, 1)]
            public int Range { get; init; } = 10;
        }
    }

    private Rectangle BarRect;
    private CachedBitmap _cachedBackground;
    private float _range;
    private Font font;

    public TrackBarOverlay(Rectangle rectangle) : base(rectangle, "Track Bar")
    {
        RefreshRateHz = 6;
    }

    public sealed override void SetupPreviewData()
    {
        SessionData.Instance.FocusedCarIndex = 1;
        var car1 = new CarInfo(1);
        car1.TrackPercentCompleted = 1.0f;
        car1.Position = 1;
        car1.CarLocation = CarInfo.CarLocationEnum.Track;
        SessionData.Instance.AddOrUpdateCar(1, car1);
        CarInfo car2 = new CarInfo(2);
        car2.TrackPercentCompleted = 1.03f;
        car2.Position = 2;
        car2.CarLocation = CarInfo.CarLocationEnum.Track;
        SessionData.Instance.AddOrUpdateCar(2, car2);
    }

    public sealed override void BeforeStart()
    {
        _range = _config.Bar.Range / 100f;

        BarRect = new Rectangle(0, 0, 400, 60);
        font = FontUtil.FontSegoeMono(10);

        _cachedBackground = new CachedBitmap(BarRect.Width, BarRect.Height, g =>
        {
            using Brush bg = new SolidBrush(Color.FromArgb(170, Color.Black));
            g.FillRoundedRectangle(bg, BarRect, 4);
        });

        Width = BarRect.Width;
        Height = BarRect.Height;
    }

    public sealed override void BeforeStop()
    {
        _cachedBackground?.Dispose();
    }

    public sealed override bool ShouldRender()
    {
        /* TODO
        if (_config.Viewing.Spectator && RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
            return true;
            */

        return base.ShouldRender();
    }

    public sealed override void Render(Graphics g)
    {
        _cachedBackground?.Draw(g);

        if (SessionData.Instance.Cars.Count == 0) return;

        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        
        var spectatingCar = SessionData.Instance.Cars[SessionData.Instance.FocusedCarIndex];
        float spectatingSplinePosition = spectatingCar.Value.TrackPercentCompleted;

        float halfRange = _range / 2f;
        float minSpline = spectatingSplinePosition - halfRange;
        float maxSpline = spectatingSplinePosition + halfRange;

        bool adjustedUp = false;
        if (minSpline < 0)
        {
            minSpline += 1;
            maxSpline += 1;
            adjustedUp = true;
        }

        var data = SessionData.Instance.Cars.Where(x =>
        {
            float pos = x.Value.TrackPercentCompleted;

            if (adjustedUp && pos < 0.5)
                pos += 1;
            if (!adjustedUp && pos < minSpline)
                pos += 1;

            return pos < maxSpline && pos > minSpline;
        });

        // Debug.WriteLine($"Found {data.Count()} cars in range\nRange: {minSpline:F2} - {maxSpline:F2}");
        foreach (var entry in data)
        {
            float pos = entry.Value.TrackPercentCompleted;
            if (adjustedUp && pos < 0.5) pos += 1;
            if (pos < minSpline) pos += 1;
            float correctedPos = maxSpline - pos;
            bool isSpectatingCar = SessionData.Instance.FocusedCarIndex == entry.Key;

            float correctedPercentage = (correctedPos * 100) / _range / 100;
            if (isSpectatingCar) correctedPercentage = 0.5f;
            // Debug.WriteLine($"pos:{pos} --> cor:{correctedPos} --> perc:{correctedPercentage:F3}");

            int x = BarRect.Width - (int)(BarRect.Width * correctedPercentage);

            bool isInPits = (entry.Value.CarLocation == CarInfo.CarLocationEnum.Pitlane);

            Pen pen = isSpectatingCar ? Pens.Red : isInPits ? Pens.Green : Pens.White;
            if (!isInPits && !isSpectatingCar && entry.Value.Kmh < 33)
                pen = Pens.Yellow;
            int y = isInPits ? 25 : 5;
            g.DrawLine(pen, new Point(x, y), new Point(x, BarRect.Height - y));

            if (!isSpectatingCar)
            {
                using Brush brush = new SolidBrush(Color.FromArgb(120, Color.Black));
                g.FillRectangle(brush, new Rectangle(x + 1, y, 18, 15));

                Color textColor = Color.White;

                if (SessionData.Instance.SessionType == RaceSessionType.Race || SessionData.Instance.SessionType == RaceSessionType.Race)
                {
                    if (entry.Value.Laps < spectatingCar.Value.Laps)
                        textColor = Color.Cyan;
                    if (entry.Value.Laps > spectatingCar.Value.Laps)
                        textColor = Color.Orange;
                }

                g.DrawStringWithShadow($"{entry.Value.RaceNumber}", font, textColor, new Point(x, y));
            }

            // Debug.WriteLine($"{entry.Value.SplinePosition:F2}");
        }

        /* TODO: we don't have corner info for all sims
        AbstractTrackData current = GetCurrentTrack(pageStatic.Track);
        if (current == null && RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
            current = GetCurrentTrackByFullName(broadCastTrackData.TrackName);

        if (current != null)
        {
            foreach (var corner in current.CornerNames)
            {
                float min = spectatingSplinePosition - halfRange;
                float max = spectatingSplinePosition + halfRange;
                if (corner.Value.Item1 != -1 && corner.Key.To > min && corner.Key.From < max)
                {
                    float corrected1 = max - corner.Key.From;
                    float corrected2 = max - corner.Key.To;

                    float percentageFrom = (corrected1 * 100) / _range / 100;
                    float percentageTo = (corrected2 * 100) / _range / 100;

                    int xFrom = BarRect.Width - (int)(BarRect.Width * percentageFrom);
                    int xTo = BarRect.Width - (int)(BarRect.Width * percentageTo);
                    using Brush bg = new SolidBrush(Color.FromArgb(50, Color.White));
                    Rectangle bounds = new(xFrom, BarRect.Height - 20, xTo - xFrom, 20);
                    g.FillRoundedRectangle(bg, bounds, 8);

                    int centerX = (xTo + xFrom) / 2;
                    centerX.ClipMin(10);
                    centerX.ClipMax(BarRect.Width - 20);
                    g.DrawStringWithShadow($"T{corner.Value.Item1}", font, Color.Orange, new Point(centerX - 10, BarRect.Height - 16));
                }
            }
        } */

    }
}
