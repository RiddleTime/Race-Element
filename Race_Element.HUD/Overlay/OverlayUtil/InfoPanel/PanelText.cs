using System;
using System.Drawing;
using System.Drawing.Text;

namespace RaceElement.HUD.Overlay.OverlayUtil.InfoPanel;

public class PanelText 
{
    public RectangleF Rectangle;
    public CachedBitmap CachedBackground;

    private readonly Font _font;

    private CachedBitmap _cachedPanelText;



    // text properties
    public StringFormat StringFormat { get; set; } = new StringFormat()
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center
    };
    public Brush Brush { get; set; } = Brushes.White;

    private string _text;

    public PanelText(Font font, CachedBitmap cachedBackground, RectangleF rect, string text = " ")
    {
        Rectangle = rect;
        _font = font;
        _text = text;
        CachedBackground = cachedBackground;
    }

    public void Draw(Graphics g, string text, float scale)
    {
        CachedBackground?.Draw(g, (int)(Rectangle.X / scale), (int)(Rectangle.Y / scale), (int)(Rectangle.Width / scale), (int)(Rectangle.Height / scale));

        if (!_text.Equals(text) || _cachedPanelText == null)
        {
            _cachedPanelText = new CachedBitmap((int)Rectangle.Width, (int)Rectangle.Height, g =>
            {
                if (g == null)
                    return;

                RectangleF relativeRectangle = Rectangle;
                relativeRectangle.X = 0;
                relativeRectangle.Y = 0;

                if (_font != null)
                {
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    g.TextContrast = 1;
                    g.DrawStringWithShadow(text, _font, Brush, relativeRectangle, StringFormat);
                }
            });
        }
        _text = text;

        _cachedPanelText?.Draw(g, (int)(Rectangle.X / scale), (int)(Rectangle.Y / scale), (int)(Rectangle.Width / scale), (int)(Rectangle.Height / scale));
    }

    public void Dispose()
    {
        CachedBackground?.Dispose();
        _cachedPanelText?.Dispose();
        GC.SuppressFinalize(this);
    }
}
