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

namespace ACCManager.HUD.ACC.Overlays.OverlayCornerNames
{
#if DEBUG
    [Overlay(Name = "Corners", 
        Description = "Shows corner/sector names for each track.", 
        OverlayType = OverlayType.Release,
        OverlayCategory = OverlayCategory.Track,
        Version = 1.00)]
#endif
    internal class CornersOverlay : AbstractOverlay
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
        private readonly int InitialHeight;
        private readonly Font _font;

        private CachedBitmap _cachedBackground;
        private AbstractTrackData _currentTrack;

        private float _maxTextWidth = 0;

        public CornersOverlay(Rectangle rectangle) : base(rectangle, "Corners")
        {
            this.Width = InitialWidth + 1;

            _font = FontUtil.FontOrbitron(13 * this.Scale);
            InitialHeight = (int)(_font.Height * 1.3 + 1);
            this.Height = InitialHeight;
        }

        public override void BeforeStart()
        {

            if (_currentTrack == null)
                _currentTrack = TrackData.Tracks.FirstOrDefault(x => x.Key == pageStatic.Track).Value;

            RenderBackground();

            RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;

            if (_currentTrack == null)
                _currentTrack = Tracks.FirstOrDefault(x => x.Key == pageStatic.Track).Value;
        }

        private void Instance_OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e)
        {
            _currentTrack = Tracks.FirstOrDefault(x => x.Key == pageStatic.Track).Value;
            RenderBackground();
        }

        private void SetMaxTextWidth()
        {
            _maxTextWidth = 0;
            new CachedBitmap((int)((InitialWidth + 1) * this.Scale), (int)((InitialHeight + 1) * this.Scale), g =>
                 {
                     if (_currentTrack != null)
                     {
                         foreach ((int, string) value in _currentTrack.CornerNames.Values)
                         {
                             StringBuilder builder = new StringBuilder();
                             builder.Append(" ");
                             builder.Append(value.Item1);
                             if (_config.CornerNames.Names && value.Item2 != string.Empty)
                                 builder.Append(" - " + value.Item2);
                             builder.Append(" ");

                             float textWidth = g.MeasureString(builder.ToString(), _font).Width;

                             if (textWidth > _maxTextWidth)
                                 _maxTextWidth = textWidth;
                         }
                     }
                 }).Render();
        }

        private void RenderBackground()
        {
            SetMaxTextWidth();
            _cachedBackground = new CachedBitmap((int)((InitialWidth + 1) * this.Scale), (int)((InitialHeight + 1) * this.Scale), g =>
            {
                Rectangle rectangle = new Rectangle(0, 0, (int)(_maxTextWidth * this.Scale), (int)(InitialHeight * this.Scale));
                int cornerRadius = (int)(4 * this.Scale);
                g.DrawRoundedRectangle(new Pen(new SolidBrush(Color.FromArgb(185, 0, 0, 0)), 3 * this.Scale), rectangle, cornerRadius);
                g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(185, 0, 0, 0)), rectangle, cornerRadius);
            });
        }

        public override void BeforeStop()
        {
            RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
        }

        public override void Render(Graphics g)
        {
            if (_currentTrack != null)
            {
                (int, string) cornerName = _currentTrack.CornerNames.FirstOrDefault(x => x.Key.IsInRange(pageGraphics.NormalizedCarPosition)).Value;

                _cachedBackground.Draw(g, 0, 0, InitialWidth, InitialHeight);

                if (cornerName.Item1 != 0)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(" ");
                    builder.Append(cornerName.Item1);
                    if (_config.CornerNames.Names && cornerName.Item2 != string.Empty)
                        builder.Append(" - " + cornerName.Item2);
                    builder.Append(" ");
                    string text = builder.ToString();

                    float textWidth = g.MeasureString(text, _font).Width;
                    PointF location = new PointF(_maxTextWidth / 2 - textWidth / 2, InitialHeight / 2 - _font.Height / 2);
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.DrawStringWithShadow(text, _font, Color.White, location, 0.75f * this.Scale);
                }
            }
        }

    }
}
