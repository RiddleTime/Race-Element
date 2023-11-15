using System.Drawing;

namespace RaceElement.HUD.Overlay.OverlayUtil.Drawing
{
    public sealed class DrawableTextCell : AbstractDrawableCell
    {
        private string Text;
        private readonly Font Font;

        public Brush TextBrush { get; set; }
        public StringFormat StringFormat { get; set; }

        public DrawableTextCell(RectangleF rect, Font font) : base(rect)
        {
            Font = font;
            TextBrush = Brushes.White;
            StringFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
        }

        public void UpdateText(string text, bool forced = false)
        {
            if (Text == text && !forced)
                return;

            Text = text;

            CachedForeground.SetRenderer(g =>
            {
                if (Font != null)
                {
                    g.TextContrast = 2;
                    Rectangle rect = new Rectangle(0, 0, (int)Rectangle.Width, (int)Rectangle.Height);
                    g.DrawStringWithShadow(text, Font, TextBrush, rect, StringFormat);
                }
            });
        }
    }
}
