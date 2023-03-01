using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracks;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using static RaceElement.Data.ACC.Tracks.TrackData;
using RaceElement.HUD.Overlay.OverlayUtil.InfoPanel;
using System.Diagnostics;
using RaceElement.Util.SystemExtensions;
using System;

namespace ACCManager.HUD.ACC.Overlays.OverlayCornerNames
{
    [Overlay(Name = "Track Corners",
        Description = "Shows corner/sector names for each track.",
        OverlayType = OverlayType.Release,
        OverlayCategory = OverlayCategory.Track,
        Version = 1.00)]
    internal sealed class TrackCornersOverlay : AbstractOverlay
    {
        private readonly CornerNamesConfig _config = new CornerNamesConfig();
        private sealed class CornerNamesConfig : OverlayConfiguration
        {
            [ConfigGrouping("Names", "Configure options specific to the Corner Names HUD.")]
            public CornerNamesGrouping CornerNames { get; set; } = new CornerNamesGrouping();
            public class CornerNamesGrouping
            {
                [ToolTip("Show corner names in addition to the already displayin corner numbers.\nNot Every corner has got a name and some tracks don't have corner names at all.")]
                public bool Names { get; set; } = true;
            }

            public CornerNamesConfig()
            {
                this.AllowRescale = true;
            }
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

        public override void BeforeStart()
        {
            _font = FontUtil.FontSegoeMono(14f * this.Scale);

            int lineHeight = _font.Height + 1;
            int unscaledHeaderWidth = 48;

            int headerWidth = (int)(unscaledHeaderWidth * this.Scale);

            RectangleF headerRect = new RectangleF(0, 0, headerWidth, lineHeight);
            StringFormat headerFormat = new StringFormat() { Alignment = StringAlignment.Center };
            StringFormat valueFormat = new StringFormat() { Alignment = StringAlignment.Center };

            CachedBitmap headerBackground = new CachedBitmap(headerWidth, lineHeight, g =>
            {
                Color accentColor = Color.FromArgb(25, 255, 0, 0);
                Rectangle panelRect = new Rectangle(0, 0, headerWidth, lineHeight);
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, _config.CornerNames.Names ? 0 : RoundingRadius, 0, RoundingRadius);
                g.FillPath(new SolidBrush(Color.FromArgb(225, 10, 10, 10)), path);
                g.DrawLine(new Pen(accentColor), 0 + RoundingRadius / 2, lineHeight, headerWidth, lineHeight - 1);
            });

            _cornerNumberHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            headerRect.Offset(0, lineHeight);

            if (_config.CornerNames.Names)
            {
                RectangleF valueRect = new RectangleF(headerWidth, 0, 10, lineHeight);
                CachedBitmap valueBackground = GetCachedValueBackGround(10, lineHeight);
                _cornerTextValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
                valueRect.Offset(0, lineHeight);
            }

            this.Height = (int)(headerRect.Top / Scale);

            RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;
            RaceSessionTracker.Instance.OnRaceWeekendEnded += Instance_OnRaceWeekendEnded;
        }

        private void Instance_OnRaceWeekendEnded(object sender, RaceElement.Data.ACC.Database.RaceWeekend.DbRaceWeekend e)
        {
            //_currentTrack = null;
            //UpdateWidth();
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
                Rectangle panelRect = new Rectangle(0, 0, valueWidth, lineHeight);
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, RoundingRadius, 0, 0);
                using SolidBrush brush = new SolidBrush(Color.FromArgb(225, 0, 0, 0));
                g.FillPath(brush, path);
                g.DrawLine(new Pen(accentColor), 0, lineHeight - 1, valueWidth, lineHeight - 1);
            });
        }

        private void UpdateWidth()
        {
            if (pageGraphics.Status != RaceElement.ACCSharedMemory.AcStatus.AC_LIVE)
            {
                return;
            }
            Debug.WriteLine("Updating Width");
            _currentTrack = Tracks.FirstOrDefault(x => x.Key == pageStatic.Track).Value;
            Debug.WriteLine(_currentTrack);

            this.Width = (int)(_cornerNumberHeader.Rectangle.Width);
            Debug.WriteLine($"Header Width: {Width}");

            if (_config.CornerNames.Names)
            {
                _cornerTextValue.CachedBackground?.Dispose();

                float maxTextWidth = GetMaxTextWidth();
                maxTextWidth.ClipMin(10);
                maxTextWidth = (float)Math.Ceiling(maxTextWidth);
                Debug.WriteLine(maxTextWidth);

                _cornerTextValue.CachedBackground = GetCachedValueBackGround((int)(maxTextWidth), _font.Height + 1);
                _cornerTextValue.Rectangle.Width = _cornerTextValue.CachedBackground.Width;
                this.Width += (int)(_cornerTextValue.Rectangle.Width);
            }

            Debug.WriteLine($"Total Width: {Width}");
        }

        private float GetMaxTextWidth()
        {
            float _maxTextWidth = 0;

            CachedBitmap b = new CachedBitmap(1, 1, g =>
            {
                if (_currentTrack != null)
                    foreach ((int, string) value in _currentTrack.CornerNames.Values)
                    {
                        float textWidth = g.MeasureString(value.Item2.ToString(), _font).Width;

                        if (textWidth > _maxTextWidth)
                            _maxTextWidth = textWidth;
                    }
            });
            b.Dispose();

            return _maxTextWidth;
        }

        public override void BeforeStop()
        {
            RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
            RaceSessionTracker.Instance.OnRaceWeekendEnded -= Instance_OnRaceWeekendEnded;
            _cornerNumberHeader?.Dispose();
            _cornerTextValue?.Dispose();
            _font?.Dispose();
        }

        public override void Render(Graphics g)
        {
            string cornerNumber = "";
            string cornerName = "";

            if (_currentTrack == null)
                UpdateWidth();

            if (_currentTrack != null)
            {
                (int, string) corner = _currentTrack.CornerNames.FirstOrDefault(x => x.Key.IsInRange(pageGraphics.NormalizedCarPosition)).Value;
                if (corner.Item1 != 0)
                {
                    cornerNumber = $"{corner.Item1}";

                    if (_config.CornerNames.Names)
                        cornerName = corner.Item2;
                }
            }

            _cornerNumberHeader?.Draw(g, cornerNumber, Scale);
            if (_config.CornerNames.Names)
                _cornerTextValue?.Draw(g, cornerName, Scale);
        }

    }

}
