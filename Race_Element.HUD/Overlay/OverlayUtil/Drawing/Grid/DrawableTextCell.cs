using System.Drawing;

namespace RaceElement.HUD.Overlay.OverlayUtil.Drawing;

public sealed class DrawableTextCell : AbstractDrawableCell
{
    public Brush TextBrush { get; set; }
    public StringFormat StringFormat { get; set; }

    private readonly Font Font;
    private string Text;

    public DrawableTextCell(RectangleF rect, Font font) : base(rect)
    {
        Font = font;
        TextBrush = Brushes.White;
        StringFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.None,
        };
    }

    public bool UpdateText(string text, bool forced = false)
    {
        if (Text == text && !forced)
            return false;

        Text = text;

        CachedForeground.SetRenderer(g =>
        {
            if (Font != null)
            {
                g.TextContrast = 2;
                RectangleF rect = new(0, 0, Rectangle.Width, Rectangle.Height);
                g.DrawStringWithShadow(text, Font, TextBrush, rect, StringFormat);
            }
        });

        return true;
    }
}
