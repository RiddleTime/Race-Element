using System;
using System.Drawing;

namespace RaceElement.HUD.Overlay.OverlayUtil.Drawing;

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

    public void Draw(Graphics g, float scaling = 1)
    {
        CachedBackground?.Draw(g, (int)Math.Ceiling(Rectangle.X * scaling), (int)Math.Ceiling(Rectangle.Y * scaling), (int)Math.Ceiling(CachedBackground.Width * scaling), (int)Math.Ceiling(CachedBackground.Height * scaling));
        CachedForeground?.Draw(g, (int)Math.Ceiling(Rectangle.X * scaling), (int)Math.Ceiling(Rectangle.Y * scaling), (int)Math.Ceiling(CachedForeground.Width * scaling), (int)Math.Ceiling(CachedForeground.Height * scaling));
    }

    public void Dispose()
    {
        CachedBackground?.Dispose();
        CachedForeground?.Dispose();
        GC.SuppressFinalize(this);
    }
}
