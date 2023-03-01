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

namespace ACCManager.HUD.ACC.Overlays.OverlayCornerNames
{
#if DEBUG
    [Overlay(Name = "Track Corners",
        Description = "Shows corner/sector names for each track.",
        OverlayType = OverlayType.Release,
        OverlayCategory = OverlayCategory.Track,
        Version = 1.00)]
#endif
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

        private readonly int InitialWidth = 450;
        private int InitialHeight;
        private Font _font;

        private PanelText _cornerNumberHeader;
        private PanelText _cornerTextValue;

        private AbstractTrackData _currentTrack;

        private float _maxTextWidth = 0;

        public TrackCornersOverlay(Rectangle rectangle) : base(rectangle, "Track Corners")
        {
            //this.Width = InitialWidth + 1;

        }

        public override void BeforeStart()
        {
            if (!_config.CornerNames.Names)
                this.Width = 48;

            _font = FontUtil.FontSegoeMono(14f * this.Scale);

            int lineHeight = _font.Height + 1;
            int unscaledHeaderWidth = 48;
            int unscaledValueWidth = 1;

            int headerWidth = (int)(unscaledHeaderWidth * this.Scale);
            int valueWidth = (int)(unscaledValueWidth * this.Scale);
            int roundingRadius = (int)(6 * this.Scale);

            RectangleF headerRect = new RectangleF(0, 0, headerWidth, lineHeight);
            RectangleF valueRect = new RectangleF(headerWidth, 0, valueWidth, lineHeight);
            StringFormat headerFormat = new StringFormat() { Alignment = StringAlignment.Center };
            StringFormat valueFormat = new StringFormat() { Alignment = StringAlignment.Near };

            Color accentColor = Color.FromArgb(25, 255, 0, 0);
            CachedBitmap headerBackground = new CachedBitmap(headerWidth, lineHeight, g =>
            {
                Rectangle panelRect = new Rectangle(0, 0, headerWidth, lineHeight);
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, 0, 0, roundingRadius);
                g.FillPath(new SolidBrush(Color.FromArgb(225, 10, 10, 10)), path);
                g.DrawLine(new Pen(accentColor), 0 + roundingRadius / 2, lineHeight, headerWidth, lineHeight - 1);
            });
            CachedBitmap valueBackground = new CachedBitmap(valueWidth, lineHeight, g =>
            {
                Rectangle panelRect = new Rectangle(0, 0, valueWidth, lineHeight);
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, roundingRadius, 0, 0);
                g.FillPath(new SolidBrush(Color.FromArgb(225, 0, 0, 0)), path);
                g.DrawLine(new Pen(accentColor), 0, lineHeight - 1, valueWidth, lineHeight - 1);
            });

            _cornerNumberHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _cornerTextValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);

            this.Height = (int)(headerRect.Top / this.Scale);

            RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;

            if (_currentTrack == null)
                UpdateWidth();
        }

        private void Instance_OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e)
        {
            UpdateWidth();
        }

        private void UpdateWidth()
        {
            _currentTrack = Tracks.FirstOrDefault(x => x.Key == pageStatic.Track).Value;
            _cornerTextValue?.SetWidth((int)GetMaxTextWidth());
        }

        private float GetMaxTextWidth()
        {
            _maxTextWidth = 0;
            CachedBitmap b = new CachedBitmap((int)((InitialWidth + 1) * this.Scale), (int)((InitialHeight + 1) * this.Scale), g =>
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
            _cornerNumberHeader?.Dispose();
            _cornerTextValue?.Dispose();
            _font?.Dispose();
        }

        public override void Render(Graphics g)
        {
            string header = "?";
            string name = string.Empty;

            if (_currentTrack != null)
            {
                (int, string) cornerName = _currentTrack.CornerNames.FirstOrDefault(x => x.Key.IsInRange(pageGraphics.NormalizedCarPosition)).Value;
                if (cornerName.Item1 != 0)
                {

                    header = $"{cornerName.Item1}";
                    if (_config.CornerNames.Names)
                        name = cornerName.Item2;
                }
            }

            _cornerNumberHeader?.Draw(g, header, Scale);
            if (_config.CornerNames.Names)
                _cornerTextValue?.Draw(g, name, Scale);
        }

    }

}
