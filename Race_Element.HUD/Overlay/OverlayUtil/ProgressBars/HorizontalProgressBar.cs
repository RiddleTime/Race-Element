using System;
using System.Drawing;

namespace RaceElement.HUD.Overlay.OverlayUtil.ProgressBars;

public class HorizontalProgressBar
{
    // dimension
    private int _width;
    private int _height;
    public float Scale { private get; set; } = 1f;

    // values
    public double Min { private get; set; } = 0;
    public double Max { private get; set; } = 1;
    public double Value { private get; set; } = 0;

    // style
    public bool LeftToRight { private get; set; } = true;
    public bool DrawBackground { private get; set; } = false;
    public bool Rounded { private get; set; }
    public float Rounding { private get; set; } = 3;
    public Brush OutlineBrush { private get; set; } = Brushes.White;
    public Brush FillBrush { private get; set; } = Brushes.OrangeRed;
    public Brush BackgroundBrush { private get; set; } = new SolidBrush(Color.FromArgb(96, Color.Black));

    private CachedBitmap _cachedOutline;
    private CachedBitmap _cachedBackground;

    public HorizontalProgressBar(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public void Draw(Graphics g, int x, int y)
    {
        if (_cachedOutline == null)
            RenderCachedOutline();

        if (DrawBackground)
        {
            if (_cachedBackground == null)
                RenderCachedBackground();

            _cachedBackground?.Draw(g, x, y, _width, _height);
        }

        double percent = Value / Max;

        int scaledHeight = (int)(_height * Scale);
        int scaledWidth = (int)(_width * Scale);

        int fillWidth = (int)(scaledWidth * percent);
        int adjustedX = 0;

        if (!LeftToRight)
        {
            adjustedX = (scaledWidth - fillWidth);
        }

        CachedBitmap barBitmap = new(scaledWidth + 1, scaledHeight + 1, bg =>
        {
            if (Rounded)
            {
                if (percent >= 0.035f)
                {
                    bg.FillRoundedRectangle(FillBrush, new Rectangle(adjustedX, 0, fillWidth, scaledHeight), (int)(Rounding * Scale));
                }
            }
            else
                bg.FillRectangle(FillBrush, new Rectangle(adjustedX, 0, fillWidth, scaledHeight));
        });

        barBitmap?.Draw(g, x, y, _width, _height);
        _cachedOutline?.Draw(g, x, y, _width, _height);
    }

    private void RenderCachedBackground()
    {
        int scaledWidth = (int)(_width * Scale);
        int scaledHeight = (int)(_height * Scale);
        if (Rounded)
            _cachedBackground = new CachedBitmap(scaledWidth + 1, scaledHeight + 1, g => g.FillRoundedRectangle(BackgroundBrush, new Rectangle(0, 0, scaledWidth, scaledHeight), (int)(Rounding * Scale)));
        else
            _cachedBackground = new CachedBitmap(scaledWidth + 1, scaledHeight + 1, g => g.FillRectangle(BackgroundBrush, new Rectangle(0, 0, scaledWidth, scaledHeight)));
    }

    private void RenderCachedOutline()
    {
        int scaledWidth = (int)(_width * Scale);
        int scaledHeight = (int)(_height * Scale);
        if (Rounded)
            _cachedOutline = new CachedBitmap(scaledWidth + 1, scaledHeight + 1, g => g.DrawRoundedRectangle(new Pen(OutlineBrush, 1 * Scale), new Rectangle(0, 0, scaledWidth, scaledHeight), (int)(Rounding * Scale)));
        else
            _cachedOutline = new CachedBitmap(scaledWidth + 1, scaledHeight + 1, g => g.DrawRectangle(new Pen(OutlineBrush, 1 * Scale), new Rectangle(0, 0, scaledWidth, scaledHeight)));
    }

    public void DisposeBitmaps()
    {
        _cachedOutline?.Dispose();
        _cachedBackground?.Dispose();
    }
}
