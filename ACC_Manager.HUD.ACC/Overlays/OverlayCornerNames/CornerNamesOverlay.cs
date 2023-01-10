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


        private AbstractTrackData CurrentTrack;
        private CachedBitmap _cachedBackground;
        private Font _font;

        public CornerNamesOverlay(Rectangle rectangle) : base(rectangle, "Corner Names")
        {
        }

        public override void BeforeStart()
        {
            _font = FontUtil.FontOrbitron(15);

            this.Height = (int)(_font.Height * 1.5);
            this.Width = 300;

            _cachedBackground = new CachedBitmap((int)(this.Width * this.Scale), (int)(this.Height * this.Scale), g =>
            {
                g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(185, 0, 0, 0)), new Rectangle(0, 0, Width, Height), (int)(3 * this.Scale));
            });

            RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;

            if (CurrentTrack == null)
                CurrentTrack = Tracks.FirstOrDefault(x => x.Key == pageStatic.Track).Value;
        }

        private void Instance_OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e)
        {
            CurrentTrack = Tracks.FirstOrDefault(x => x.Key == pageStatic.Track).Value;
        }

        public override void BeforeStop()
        {
            RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
        }

        public override void Render(Graphics g)
        {
            if (CurrentTrack != null)
            {
                _cachedBackground.Draw(g, (int)(Width * this.Scale), (int)(Height * this.Scale));

                string cornerName = CurrentTrack.CornerNames.FirstOrDefault(x => x.Key.IsInRange(pageGraphics.NormalizedCarPosition)).Value;
                float textWidht = g.MeasureString(cornerName, _font).Width;
                PointF location = new PointF(Width / 2 - textWidht / 2, _font.GetHeight() / 3);
                g.DrawStringWithShadow(cornerName, _font, Brushes.White, location);
            }
        }

        public override bool ShouldRender() => DefaultShouldRender();
    }
}
