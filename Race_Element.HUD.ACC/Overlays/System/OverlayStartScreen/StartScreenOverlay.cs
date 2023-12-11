using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Buffers.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Unglide;

namespace RaceElement.HUD.ACC.Overlays.OverlayStartScreen
{
#if DEBUG
    [Overlay(Name = "Start Screen",
     Description = "Shows a start screen",
     Version = 1.00,
     OverlayType = OverlayType.Pitwall)]
#endif
    public sealed class StartScreenOverlay : AbstractOverlay
    {
        public string Version { get; set; }

        private CachedBitmap _cachedBitmap;
        private CachedBitmap _cachedText;
        private CachedBitmap _slider;
        private Tweener tweener;
        private DateTime tweenStart;

        private const int SliderWidth = 280;
        private int sliderX = -SliderWidth;

        public StartScreenOverlay(Rectangle rectangle) : base(rectangle, "Start Screen")
        {
            this.X = rectangle.X;
            this.Y = rectangle.Y;
            this.Width = 360;
            this.Height = 55;
            this.RefreshRateHz = 30;
        }

        public override void BeforeStart()
        {
            _cachedBitmap = new CachedBitmap(this.Width, this.Height, g =>
            {
                Rectangle rectangle = new Rectangle(0, 0, Width - 1, Height - 1);
                using HatchBrush hatchBrush = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.FromArgb(230, Color.Black), Color.FromArgb(185, Color.Black));
                g.FillRoundedRectangle(hatchBrush, rectangle, 8);
            }, opacity: 0);

            _cachedText = new CachedBitmap(this.Width, this.Height, g =>
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.TextContrast = 1;

                string header = $"Race Element {Version}";
                Font font16 = FontUtil.FontConthrax(18);
                int font16Height = font16.Height;
                int halfStringLength = (int)(g.MeasureString(header, font16).Width / 2);
                int x = this.Width / 2 - halfStringLength;
                g.DrawStringWithShadow(header, font16, Color.FromArgb(215, Color.White), new Point(x, 0), 2f, new StringFormat() { LineAlignment = StringAlignment.Near });
                font16.Dispose();

                string subHeader = "by Reinier Klarenberg";
                Font font11 = FontUtil.FontConthrax(13);
                g.DrawStringWithShadow(subHeader, font11, Color.FromArgb(185, Color.White), new Point(x + 2, font16Height));
                font11.Dispose();
            }, opacity: 0);

            _slider = new CachedBitmap(SliderWidth, Height, g =>
            {


                RectangleF rect = new RectangleF(0, 0, SliderWidth, Height / 1.9f);
                using GraphicsPath gradientPath = new GraphicsPath();
                gradientPath.AddEllipse(rect);
                using PathGradientBrush pthGrBrush = new PathGradientBrush(gradientPath);
                pthGrBrush.SurroundColors = new Color[] { Color.FromArgb(0, 255, 0, 0) };
                pthGrBrush.CenterColor = Color.FromArgb(180, 255, 0, 0);

                using Pen pen = new Pen(pthGrBrush, 2);
                int spacing = 6;
                int lines = (int)Math.Floor(SliderWidth / (double)spacing);
                for (int i = 0; i < lines * 2; i++)
                {
                    int baseX = -SliderWidth / 3 + i * spacing;
                    g.DrawLine(pen, new Point(baseX, 0), new Point(baseX + SliderWidth / 2, Height));
                }

                //g.FillRoundedRectangle(pthGrBrush, new Rectangle(0, 0, SliderWidth, Height), 8);
            }, opacity: 0);

            tweener = new Tweener();
            tweener.AddTween(tweener.Tween(_cachedBitmap, new { Opacity = 1f }, 1.3f).Ease(Ease.ExpoIn));
            tweener.AddTween(tweener.Tween(_cachedText, new { Opacity = 1f }, 1.3f).Ease(Ease.ExpoIn));
            tweener.AddTween(tweener.Tween(_slider, new { Opacity = 1f }, 2f).Ease(Ease.ExpoIn));
            tweenStart = DateTime.Now;
        }

        public override void BeforeStop()
        {
            _cachedBitmap?.Dispose();
            _cachedText?.Dispose();
            _slider?.Dispose();
        }

        public override bool ShouldRender() => true;

        public override void Render(Graphics g)
        {
            tweener.Update((float)DateTime.Now.Subtract(tweenStart).TotalSeconds);

            _cachedBitmap?.Draw(g);

            if (sliderX > Width) sliderX = -SliderWidth;
            _slider?.Draw(g, new Point(sliderX, 0));
            sliderX += 30;

            _cachedText?.Draw(g);
        }
    }
}
