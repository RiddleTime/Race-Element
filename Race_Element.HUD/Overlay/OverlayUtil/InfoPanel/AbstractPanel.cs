using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using System;
using System.Drawing;

namespace RaceElement.HUD.Overlay.OverlayUtil.InfoPanel
{
    public abstract class AbstractPanel : IDrawable
    {
        public RectangleF Rectangle;
        public CachedBitmap CachedBackground;
        public CachedBitmap CachedForeground;

        public abstract void Draw(Graphics g);
        public void Dispose()
        {
            CachedBackground?.Dispose();
            CachedForeground?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
