using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
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
        private CachedBitmap _slider;
        private Tweener tweener;
        private DateTime tweenStart;

        private const int SliderWidth = 80;
        private int sliderX = -SliderWidth;

        public StartScreenOverlay(Rectangle rectangle) : base(rectangle, "Start Screen")
        {
            this.X = rectangle.X;
            this.Y = rectangle.Y;
            this.Width = 320;
            this.Height = 45;
            this.RefreshRateHz = 30;
        }

        public override void BeforeStart()
        {
            _cachedBitmap = new CachedBitmap(this.Width, this.Height, g =>
            {
                Rectangle rectangle = new Rectangle(0, 0, Width - 1, Height - 1);
                HatchBrush hatchBrush = new HatchBrush(HatchStyle.LightDownwardDiagonal, Color.FromArgb(225, Color.Black), Color.FromArgb(185, Color.Black));
                g.FillRoundedRectangle(hatchBrush, rectangle, 8);
                hatchBrush.Dispose();

                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.TextContrast = 1;

                string header = $"Race Element {Version}";
                Font font16 = FontUtil.FontConthrax(16);
                int font16Height = font16.Height;
                int halfStringLength = (int)(g.MeasureString(header, font16).Width / 2);
                int x = this.Width / 2 - halfStringLength;
                g.DrawStringWithShadow(header, font16, Color.FromArgb(215, Color.White), new Point(x, 0), 2f);
                font16.Dispose();

                string subHeader = "by Reinier Klarenberg";
                Font font11 = FontUtil.FontConthrax(11);
                g.DrawStringWithShadow(subHeader, font11, Color.FromArgb(185, Color.White), new Point(x + 2, font16Height));
                font11.Dispose();
            }, opacity: 0);

            _slider = new CachedBitmap(SliderWidth, Height, g =>
            {
                RectangleF rect = new RectangleF(0, 0, SliderWidth, Height);
                GraphicsPath gradientPath = new GraphicsPath();
                gradientPath.AddEllipse(rect);
                PathGradientBrush pthGrBrush = new PathGradientBrush(gradientPath);
                pthGrBrush.SurroundColors = new Color[] { Color.FromArgb(0, 255, 0, 0) };
                pthGrBrush.CenterColor = Color.FromArgb(70, 255, 0, 0);

                g.FillRoundedRectangle(pthGrBrush, new Rectangle(0, 0, SliderWidth, Height), 8);
            }, opacity: 0);

            tweener = new Tweener();
            tweener.AddTween(tweener.Tween(_cachedBitmap, new { Opacity = 1f }, 1).Ease(Ease.BackIn));
            tweener.AddTween(tweener.Tween(_slider, new { Opacity = 1f }, 2).Ease(Ease.ExpoIn));
            tweenStart = DateTime.Now;
        }

        public override void BeforeStop()
        {
            _cachedBitmap?.Dispose();
            _slider?.Dispose();
        }

        public override bool ShouldRender() => true;

        public override void Render(Graphics g)
        {
            tweener.Update((float)DateTime.Now.Subtract(tweenStart).TotalSeconds);
            _cachedBitmap?.Draw(g);
            if (sliderX > Width - SliderWidth / 2) sliderX = -SliderWidth;
            _slider.Draw(g, new Point(sliderX, 0));
            sliderX += 10;
        }
    }
}
