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
            this.Height = 45;
            this.RefreshRateHz = 1;
        }

        public override void BeforeStart()
        {
            cachedBitmap = new CachedBitmap(this.Width, this.Height, g =>
            {
                Rectangle rectangle = new Rectangle(0, 0, Width - 1, Height - 1);
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(Width, 0), Color.FromArgb(147, Color.Black), Color.FromArgb(137, Color.Black));
                g.FillRoundedRectangle(brush, rectangle, 8);

                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.TextContrast = 1;

                string header = "Race Element is starting";
                Font font16 = FontUtil.FontConthrax(16);
                int font16Height = font16.Height;
                int halfStringLength = (int)(g.MeasureString(header, font16).Width / 2);
                int x = this.Width / 2 - halfStringLength;
                g.DrawStringWithShadow(header, font16, Color.FromArgb(215, Color.White), new Point(x, 0), 2f);
                font16.Dispose();

                string subHeader = "by RiddleTime";
                Font font11 = FontUtil.FontConthrax(11);
                g.DrawStringWithShadow(subHeader, font11, Color.FromArgb(185, Color.White), new Point(x + 2, font16Height));
                font11.Dispose();
            });
        }

        public override void BeforeStop() => cachedBitmap?.Dispose();

        public override bool ShouldRender() => true;

        public override void Render(Graphics g) => cachedBitmap?.Draw(g);
    }
}
