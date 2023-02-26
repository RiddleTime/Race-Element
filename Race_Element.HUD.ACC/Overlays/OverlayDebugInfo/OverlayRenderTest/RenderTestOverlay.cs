using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.OverlayAbc
{
    [Overlay(Name = "Render Test", Description = "Just to test", OverlayType = OverlayType.Debug)]
    internal sealed class RenderTestOverlay : AbstractOverlay
    {
        private readonly AbcConfiguration _config = new AbcConfiguration();
        private sealed class AbcConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("test", "Description here")]
            public TestGrouping Test { get; set; } = new TestGrouping();
            public class TestGrouping
            {
                [IntRange(1, 500, 1)]
                public int Herz { get; set; } = 50;
            }

            public AbcConfiguration() => AllowRescale = true;
        }

        public RenderTestOverlay(Rectangle rectangle) : base(rectangle, "Render Test")
        {
            Width = 300;
            Height = 300;
        }

        private Font font;

        public override void BeforeStart()
        {
            font = FontUtil.FontSegoeMono(15);
            this.RefreshRateHz = _config.Test.Herz;
        }

        public override void BeforeStop()
        {
            font?.Dispose();
        }

        public override bool ShouldRender() => true;

        private float timer = 0;


        DateTime _lastTime;
        private int _framesRendered = 0;
        private double _fps;

        public override void Render(Graphics g)
        {
            Matrix transform = g.Transform;
            float sinus = (float)(Math.Sin(timer / 2) * Width / this.Scale / 4);
            transform.Translate(sinus, sinus);
            transform.RotateAt(timer * 10, new PointF((int)(Width / this.Scale / 2), (int)(Height / this.Scale / 2)));
            transform.Shear(-sinus / 500, sinus / 500);

            g.Transform = transform;

            int boxSize = 50;
            Rectangle box = new Rectangle((int)(Width / this.Scale / 2 - boxSize / 2), (int)(Height / this.Scale / 2 - boxSize / 2), boxSize, boxSize);

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRectangle(Brushes.White, box);

            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.DrawStringWithShadow("Render", font, Brushes.OrangeRed, box, new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            });

            timer += 0.2f;

            g.ResetTransform();

            UpdateFps();
            DrawFpsCounter(g);
        }

        private void UpdateFps()
        {
            _framesRendered++;

            if ((DateTime.Now - _lastTime).TotalSeconds >= 2)
            {
                _fps = _framesRendered / 2d;
                _framesRendered = 0;
                _lastTime = DateTime.Now;
            }
        }

        private void DrawFpsCounter(Graphics g)
        {
            Rectangle fpsRect = new Rectangle(0, 0, 120, font.Height);
            g.FillRectangle(new SolidBrush(Color.FromArgb(185, Color.Black)), fpsRect);
            g.DrawStringWithShadow($"{_fps:F1} FPS", font, Brushes.White, fpsRect);
        }
    }
}
