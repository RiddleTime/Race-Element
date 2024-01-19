using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.InfoPanel;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.HUD.ACC.Overlays.OverlayCornerNames;


[Overlay(Name = "Track Corners",
    Description = "Shows corner/sector names for each track.",
    OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Track,
    Version = 1.00)]
internal sealed class TrackCornersOverlay : AbstractOverlay
{
    private readonly CornerNamesConfig _config = new();
    private sealed class CornerNamesConfig : OverlayConfiguration
    {
        [ConfigGrouping("Names", "Configure options specific to the Corner Names HUD.")]
        public CornerNamesGrouping CornerNames { get; set; } = new CornerNamesGrouping();
        public class CornerNamesGrouping
        {
            [ToolTip("Show corner names in addition to the already displayin corner numbers.\nNot Every corner has got a name and some tracks don't have corner names at all.")]
            public bool Names { get; init; } = true;

            [ToolTip("Show the HUD when spectating.")]
            public bool Spectator { get; init; } = true;
        }

        public CornerNamesConfig() => this.AllowRescale = true;
    }

    private Font _font;
    private PanelText _cornerNumberHeader;
    private PanelText _cornerTextValue;
    private AbstractTrackData _currentTrack;

    private int RoundingRadius => (int)(6 * Scale);

    public TrackCornersOverlay(Rectangle rectangle) : base(rectangle, "Track Corners")
    {
        RefreshRateHz = 3;
    }

    public override void SetupPreviewData()
    {
        pageStatic.Track = "Spa";
        pageGraphics.NormalizedCarPosition = 0.0472972f;
    }

    public override void BeforeStart()
    {
        _font = FontUtil.FontSegoeMono(14f * this.Scale);

        int lineHeight = _font.Height;
        int unscaledHeaderWidth = 48;

        int headerWidth = (int)(unscaledHeaderWidth * this.Scale);

        RectangleF headerRect = new(0, 0, headerWidth, lineHeight);
        StringFormat headerFormat = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
        StringFormat valueFormat = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };

        CachedBitmap headerBackground = new(headerWidth, lineHeight, g =>
        {
            Color accentColor = Color.FromArgb(25, 255, 0, 0);
            Rectangle panelRect = new(0, 0, headerWidth, lineHeight);
            using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, _config.CornerNames.Names ? 0 : RoundingRadius, 0, RoundingRadius);
            using LinearGradientBrush brush = new(panelRect, Color.FromArgb(185, 0, 0, 0), Color.FromArgb(255, 10, 10, 10), LinearGradientMode.BackwardDiagonal);
            g.FillPath(brush, path);
            g.DrawLine(new Pen(accentColor), 0 + RoundingRadius / 2, lineHeight, headerWidth, lineHeight - 1);
        });

        _cornerNumberHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
        headerRect.Offset(0, lineHeight);

        if (_config.CornerNames.Names)
        {
            RectangleF valueRect = new(headerWidth, 0, 10, lineHeight);
            _cornerTextValue = new PanelText(_font, GetCachedValueBackGround(10, lineHeight), valueRect) { StringFormat = valueFormat };
            valueRect.Offset(0, lineHeight);
        }

        this.Height = (int)(headerRect.Top / Scale);

        RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;
        RaceSessionTracker.Instance.OnRaceWeekendEnded += Instance_OnRaceWeekendEnded;
    }

    private void Instance_OnRaceWeekendEnded(object sender, RaceElement.Data.ACC.Database.RaceWeekend.DbRaceWeekend e)
    {
        _currentTrack = null;
    }

    private void Instance_OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e)
    {
        _currentTrack = null;
        UpdateWidth();
    }

    public CachedBitmap GetCachedValueBackGround(int valueWidth, int lineHeight)
    {
        return new CachedBitmap(valueWidth, lineHeight, g =>
        {
            Color accentColor = Color.FromArgb(25, 255, 0, 0);
            Rectangle panelRect = new(0, 0, valueWidth, lineHeight);
            using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, RoundingRadius, 0, 0);
            using LinearGradientBrush brush = new(panelRect, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(185, 0, 0, 0), LinearGradientMode.ForwardDiagonal);
            g.FillPath(brush, path);
            g.DrawLine(new Pen(accentColor), 0, lineHeight - 1, valueWidth, lineHeight - 1);
        });
    }

    private void UpdateWidth()
    {
        if (!(pageGraphics.Status == RaceElement.ACCSharedMemory.AcStatus.AC_PAUSE || pageGraphics.Status == RaceElement.ACCSharedMemory.AcStatus.AC_LIVE))
            return;

        _currentTrack = Tracks.FirstOrDefault(x => x.GameName == pageStatic.Track);

        this.Width = (int)_cornerNumberHeader.Rectangle.Width;

        if (_config.CornerNames.Names)
        {
            _cornerTextValue.CachedBackground?.Dispose();

            float maxTextWidth = GetMaxTextWidth();
            maxTextWidth.ClipMin(10);
            maxTextWidth = (float)Math.Ceiling(maxTextWidth);

            _cornerTextValue.CachedBackground = GetCachedValueBackGround((int)maxTextWidth, _font.Height + 1);
            _cornerTextValue.Rectangle.Width = _cornerTextValue.CachedBackground.Width;

            this.Width += (int)_cornerTextValue.Rectangle.Width;
        }
    }

    private float GetMaxTextWidth()
    {
        float maxTextWidth = 0;

        CachedBitmap b = new(1, 1, g =>
        {
            if (_currentTrack != null)
                foreach ((int, string) value in _currentTrack.CornerNames.Values)
                {
                    float textWidth = g.MeasureString(value.Item2.ToString(), _font).Width;

                    if (textWidth > maxTextWidth)
                        maxTextWidth = textWidth;
                }
        });
        b.Dispose();

        return maxTextWidth;
    }

    public override void BeforeStop()
    {
        RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
        RaceSessionTracker.Instance.OnRaceWeekendEnded -= Instance_OnRaceWeekendEnded;

        _cornerNumberHeader?.Dispose();
        _cornerTextValue?.Dispose();
        _font?.Dispose();
    }

    public override bool ShouldRender()
    {
        if (_config.CornerNames.Spectator && RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
            return true;

        return base.ShouldRender();
    }

    public override void Render(Graphics g)
    {
        string cornerNumber = "";
        string cornerName = "";

        if (_currentTrack == null)
            UpdateWidth();

        if (_currentTrack?.GameName != pageStatic.Track)
            UpdateWidth();

        float carPosition = pageGraphics.NormalizedCarPosition;

        if (_config.CornerNames.Spectator)
        {
            int focusedIndex = broadCastRealTime.FocusedCarIndex;
            if (RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, focusedIndex))
                lock (EntryListTracker.Instance.Cars)
                {
                    if (EntryListTracker.Instance.Cars.Count != 0)
                    {
                        var car = EntryListTracker.Instance.Cars.First(car => car.Value.RealtimeCarUpdate.CarIndex == focusedIndex);
                        carPosition = car.Value.RealtimeCarUpdate.SplinePosition;
                    }
                }
        }

        if (_currentTrack != null)
        {
            var items = _currentTrack.CornerNames.Where(x => x.Key.IsInRange(carPosition));
            if (items.Any())
            {

                (int, string) corner = items.First().Value;
                if (corner.Item1 > 0)
                {
                    cornerNumber = $"{corner.Item1}";

                    if (_config.CornerNames.Names)
                        cornerName = corner.Item2;
                }
                if (corner.Item1 == -1)
                    if (_config.CornerNames.Names)
                        cornerName = corner.Item2;
            }
        }

        _cornerNumberHeader?.Draw(g, cornerNumber, Scale);
        if (_config.CornerNames.Names)
            _cornerTextValue?.Draw(g, cornerName, Scale);
    }

}
