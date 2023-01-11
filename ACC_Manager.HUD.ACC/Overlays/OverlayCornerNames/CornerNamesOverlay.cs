using ACC_Manager.Util.DataTypes;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracks;
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
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.HUD.ACC.Overlays.OverlayCornerNames
{
    [Overlay(Name = "Corner Names", Description = "Shows corner/sector names for each track.", OverlayType = OverlayType.Release, Version = 1.00)]
    internal class CornerNamesOverlay : AbstractOverlay
    {
        private readonly CornerNamesConfig _config = new CornerNamesConfig();
        private sealed class CornerNamesConfig : OverlayConfiguration
        {
            public CornerNamesConfig()
            {
                this.AllowRescale = true;
            }
        }
        private const int InitialWidth = 400, InitialHeight = 30;

        private Font _font;
        private CachedBitmap _cachedBackground;
        private AbstractTrackData _currentTrack;

        public CornerNamesOverlay(Rectangle rectangle) : base(rectangle, "Corner Names")
        {
            this.Width = InitialWidth + 1;
            this.Height = InitialHeight + 1;
        }

        public override void BeforeStart()
        {
            _font = FontUtil.FontOrbitron(14);
            _cachedBackground = new CachedBitmap((int)((InitialWidth + 1) * this.Scale), (int)((InitialHeight + 1) * this.Scale), g =>
            {
                Rectangle rectangle = new Rectangle(0, 0, (int)(InitialWidth * this.Scale), (int)(InitialHeight * this.Scale));
                int cornerRadius = (int)(4 * this.Scale);
                g.DrawRoundedRectangle(new Pen(new SolidBrush(Color.FromArgb(185, 0, 0, 0)), 3 * this.Scale), rectangle, cornerRadius);
                g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(185, 0, 0, 0)), rectangle, cornerRadius);
            });

            RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;

            if (_currentTrack == null)
                _currentTrack = Tracks.FirstOrDefault(x => x.Key == pageStatic.Track).Value;
        }

        private void Instance_OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e)
        {
            _currentTrack = Tracks.FirstOrDefault(x => x.Key == pageStatic.Track).Value;
        }

        public override void BeforeStop()
        {
            RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
        }

        public override void Render(Graphics g)
        {
            if (_currentTrack != null)
            {
                _cachedBackground.Draw(g, InitialWidth, InitialHeight);

                string cornerName = _currentTrack.CornerNames.FirstOrDefault(x => x.Key.IsInRange(pageGraphics.NormalizedCarPosition)).Value;
                float textWidht = g.MeasureString(cornerName, _font).Width;
                PointF location = new PointF(InitialWidth / 2 - textWidht / 2, InitialHeight / 2 - _font.Height / 2);
                g.DrawStringWithShadow(cornerName, _font, Color.White, location, 0.75f * this.Scale);
            }
        }

        public override bool ShouldRender() => DefaultShouldRender();
    }
}
