using System;
using System.Drawing;

namespace RaceElement.HUD.Overlay.OverlayUtil.Drawing
{
    public class GridCell : IDrawable
    {
        public RectangleF Rectangle { get; private set; }
        public CachedBitmap CachedBackground;
        public CachedBitmap CachedForeground;

        public GridCell(RectangleF rect)
        {
            Rectangle = rect;
            CachedBackground = new CachedBitmap((int)rect.Width, (int)rect.Height, g => { });
            CachedForeground = new CachedBitmap((int)rect.Width, (int)rect.Height, g => { });
        }

        public void Draw(Graphics g)
        {
            CachedBackground?.Draw(g, (int)Rectangle.X, (int)Rectangle.Y, CachedBackground.Width, CachedBackground.Height);
            CachedForeground?.Draw(g, (int)Rectangle.X, (int)Rectangle.Y, CachedForeground.Width, CachedForeground.Height);
        }

        public void Dispose()
        {
            CachedBackground?.Dispose();
            CachedForeground?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
