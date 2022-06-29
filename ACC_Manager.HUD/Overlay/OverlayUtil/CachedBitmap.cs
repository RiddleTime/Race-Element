using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.Overlay.OverlayUtil
{
    public class CachedBitmap : IDisposable
    {
        public readonly int Width;
        public readonly int Height;
        public delegate void Render(Graphics g);

        private readonly Bitmap _bitmap;
        private readonly Render _renderer;

        public CachedBitmap(int width, int height, Render renderer)
        {
            this.Width = width;
            this.Height = height;
            _renderer = renderer;
            _bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            PreRender();
        }

        private void PreRender()
        {
            lock (_bitmap)
            {
                using (Graphics g = Graphics.FromImage(_bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.CompositingQuality = CompositingQuality.GammaCorrected;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    _renderer(g);
                }
            }
        }

        public void Draw(Graphics g, Point p)
        {
            Draw(g, p.X, p.Y, Width, Height);
        }

        public void Draw(Graphics g, int width, int height)
        {
            Draw(g, 0, 0, width, height);
        }

        public void Draw(Graphics g, int x, int y, int width, int height)
        {
            lock (_bitmap)
                g.DrawImage(_bitmap, x, y, width, height);
        }

        public void Dispose()
        {
            _bitmap.Dispose();
        }
    }
}
