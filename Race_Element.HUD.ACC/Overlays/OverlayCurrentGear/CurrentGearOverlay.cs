using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace RaceElement.HUD.ACC.Overlays.OverlayCurrentGear
{
    [Overlay(Name = "Current Gear", Version = 1.00, OverlayType = OverlayType.Release,
    Description = "Shows the selected gear.")]
    internal sealed class CurrentGearOverlay : AbstractOverlay
    {
        private readonly CurrentGearConfiguration _config = new CurrentGearConfiguration();
        private sealed class CurrentGearConfiguration : OverlayConfiguration
        {
            public CurrentGearConfiguration() { AllowRescale = true; }
        }

        private const int InitialSize = 80;
        private readonly List<CachedBitmap> gearBitmaps = new List<CachedBitmap>();

        public CurrentGearOverlay(Rectangle rectangle) : base(rectangle, "Current Gear")
        {
            Width = InitialSize;
            Height = InitialSize;
            RefreshRateHz = 20;
        }

        public override void BeforeStart()
        {
            Font font = FontUtil.FontConthrax(50 * this.Scale);
            HatchBrush hatchBrush = new HatchBrush(HatchStyle.LightDownwardDiagonal, Color.FromArgb(225, Color.Black), Color.FromArgb(185, Color.Black));

            Rectangle renderRectangle = new Rectangle(0, 0, (int)(InitialSize * this.Scale), (int)(InitialSize * this.Scale));
            for (int i = 0; i <= 7; i++)
            {
                string gear;
                switch (i)
                {
                    case 0: gear = "R"; break;
                    case 1: gear = "N"; break;
                    default: gear = $"{i - 1}"; break;
                }

                gearBitmaps.Add(new CachedBitmap((int)(InitialSize * this.Scale) + 1, (int)(InitialSize * this.Scale) + 1, g =>
                {
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    g.TextContrast = 1;

                    g.FillRoundedRectangle(hatchBrush, renderRectangle, (int)(6 * this.Scale));

                    int textWidth = (int)g.MeasureString(gear, font).Width;
                    g.DrawStringWithShadow(gear, font, Color.White, new Point(renderRectangle.Width / 2 - textWidth / 2, (int)(renderRectangle.Height / 2 - font.Height / 2.18)), 1.5f * this.Scale);
                }));
            }

            font.Dispose();
            hatchBrush.Dispose();
        }

        public override void BeforeStop()
        {
            foreach (CachedBitmap cachedBitmap in gearBitmaps)
                cachedBitmap?.Dispose();
        }

        public override bool ShouldRender() => DefaultShouldRender();
        public override void Render(Graphics g) => gearBitmaps[pagePhysics.Gear]?.Draw(g, InitialSize, InitialSize);
    }
}
