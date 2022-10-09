using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Media.Media3D;

namespace ACCManager.HUD.Overlay.OverlayUtil
{
    public class VerticalProgressBar
    {
        // dimension
        private int _width;
        private int _height;

        // values
        public double Min { private get; set; } = 0;
        public double Max { private get; set; } = 1;
        public double Value { private get; set; } = 0;

        // style
        public bool Rounded { private get; set; }
        public Brush OutlineBrush { private get; set; } = Brushes.White;
        public Brush FillBrush { private get; set; } = Brushes.OrangeRed;

        private CachedBitmap _cachedOutline;

        public VerticalProgressBar(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void Draw(Graphics g, int x, int y)
        {
            if (_cachedOutline == null)
                RenderCachedOutline();

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            double percent = Value / Max;

            if (Rounded)
            {
                if (percent >= 0.03f)
                    g.FillRoundedRectangle(FillBrush, new Rectangle(x, y + _height - (int)(_height * percent), _width, (int)(_height * percent)), 3);
            }
            else
                g.FillRectangle(FillBrush, new Rectangle(x, y + _height - (int)(_height * percent), _width, (int)(_height * percent)));

            _cachedOutline?.Draw(g, x, y, _width, _height);
            g.SmoothingMode = previous;
        }

        private void RenderCachedOutline()
        {
            if (Rounded)
                _cachedOutline = new CachedBitmap(_width, _height, g => g.DrawRoundedRectangle(new Pen(OutlineBrush), new Rectangle(0, 0, _width, _height), 3));
            else
                _cachedOutline = new CachedBitmap(_width, _height, g => g.DrawRectangle(new Pen(OutlineBrush), new Rectangle(0, 0, _width, _height)));
        }
    }
}
