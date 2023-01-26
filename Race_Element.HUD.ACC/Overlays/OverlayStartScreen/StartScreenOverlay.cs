using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace RaceElement.HUD.ACC.Overlays.OverlayStartScreen
{
    public class StartScreenOverlay : AbstractOverlay
    {
        private CachedBitmap cachedBitmap;

        public StartScreenOverlay(Rectangle rectangle) : base(rectangle, "Start Screen")
        {
            this.X = rectangle.X;
            this.Y = rectangle.Y;
            this.Width = 350;
            this.Height = 30;
            this.RefreshRateHz = 1;
        }

        public override void BeforeStart()
        {
            cachedBitmap = new CachedBitmap(this.Width, this.Height, g =>
            {
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(Width, 0), Color.FromArgb(225, Color.Black), Color.FromArgb(185, Color.Black));
                g.FillRoundedRectangle(brush, new Rectangle(0, 0, Width, Height), 6);

                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                Font font = FontUtil.FontConthrax(15);
                g.DrawStringWithShadow("Race Element is starting...", font, Brushes.White, new Point(0, Height / 2 - font.Height / 2));
                font.Dispose();
            });
        }

        public override void BeforeStop() => cachedBitmap?.Dispose();

        public override bool ShouldRender() => true;

        public override void Render(Graphics g) => cachedBitmap?.Draw(g);
    }
}
