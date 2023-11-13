using System;
using System.Drawing;

namespace RaceElement.HUD.Overlay.OverlayUtil.Drawing
{
    public abstract class AbstractDrawableCell : IScalableDrawing
    {
        public RectangleF Rectangle { get; private set; }
        public CachedBitmap CachedBackground { get; set; }
        public CachedBitmap CachedForeground { get; set; }

        public AbstractDrawableCell(RectangleF rect)
        {
            Rectangle = rect;
            CachedBackground = new CachedBitmap((int)rect.Width, (int)rect.Height, g => { });
            CachedForeground = new CachedBitmap((int)rect.Width, (int)rect.Height, g => { });
        }

        public void Draw(Graphics g, float scaling)
        {
            CachedBackground?.Draw(g, (int)Math.Round(Rectangle.X * scaling), (int)Math.Round(Rectangle.Y * scaling), (int)Math.Round(CachedBackground.Width * scaling), (int)Math.Round(CachedBackground.Height * scaling));
            CachedForeground?.Draw(g, (int)Math.Round(Rectangle.X * scaling), (int)Math.Round(Rectangle.Y * scaling), (int)Math.Round(CachedForeground.Width * scaling), (int)Math.Round(CachedForeground.Height * scaling));
        }

        public void Dispose()
        {
            CachedBackground?.Dispose();
            CachedForeground?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
