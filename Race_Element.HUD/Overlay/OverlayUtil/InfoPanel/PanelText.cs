using System.Drawing;
using System.Drawing.Text;

namespace RaceElement.HUD.Overlay.OverlayUtil.InfoPanel
{
    public class PanelText
    {
        private readonly Font _font;
        private readonly RectangleF _rect;

        private readonly CachedBitmap _cachedBackground;
        private CachedBitmap _cachedPanelText;

        // text properties
        public StringFormat StringFormat { get; set; } = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        public Brush Brush { get; set; } = Brushes.White;

        private string _text;

        public PanelText(Font font, CachedBitmap cachedBackground, RectangleF rectangle, string text = " ")
        {
            _rect = rectangle;
            _font = font;
            _text = text;
            _cachedBackground = cachedBackground;
        }

        public void Draw(Graphics g, string text, float scale)
        {
            _cachedBackground?.Draw(g, (int)(_rect.X / scale), (int)(_rect.Y / scale), (int)(_rect.Width / scale), (int)(_rect.Height / scale));

            if (!_text.Equals(text) || _cachedPanelText == null)
            {
                _cachedPanelText = new CachedBitmap((int)_rect.Width, (int)_rect.Height, g =>
                {
                    if (g == null)
                        return;

                    RectangleF relativeRectangle = _rect;
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

            _cachedPanelText?.Draw(g, (int)(_rect.X / scale), (int)(_rect.Y / scale), (int)(_rect.Width / scale), (int)(_rect.Height / scale));
        }

        public void Dispose()
        {
            _cachedPanelText?.Dispose();
        }
    }
}
