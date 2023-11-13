using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RaceElement.HUD.Overlay.OverlayUtil.Drawing
{
    public sealed class DrawableTextCell : AbstractDrawableCell
    {
        private string Text;
        private Font Font;

        public Brush TextBrush { get; set; } = Brushes.White;
        public StringFormat StringFormat { get; set; } = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        public DrawableTextCell(RectangleF rect, Font font) : base(rect)
        {
            Font = font;
        }

        public void UpdateText(string text)
        {
            if (Text == text)
                return;

            Text = text;

            CachedForeground.SetRenderer(g =>
            {
                if (Font != null)
                {
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.TextContrast = 1;
                    RectangleF rect = new RectangleF(0, 0, Rectangle.Width, Rectangle.Height);
                    g.DrawStringWithShadow(text, Font, TextBrush, rect, StringFormat);
                }
            });
        }

        public void Dispose()
        {
            base.Dispose();
            Font?.Dispose();
        }
    }
}
