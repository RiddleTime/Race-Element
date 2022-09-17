using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ACCManager.HUD.Overlay.OverlayUtil
{
    public class CachedBitmap : IDisposable
    {
        public readonly int Width;
        public readonly int Height;
        public delegate void Renderer(Graphics g);

        private readonly Bitmap _bitmap;
        private Renderer _renderer;

        /// <summary>
        /// Creates a cached bitmap using the given renderer
        /// </summary>
        /// <param name="width">The Width of the bitmap</param>
        /// <param name="height">The Height of the bitmap</param>
        /// <param name="renderer">The render function</param>
        /// <param name="preRender">Default true, calls the renderer delegate</param>
        public CachedBitmap(int width, int height, Renderer renderer, bool preRender = true)
        {
            this.Width = width;
            this.Height = height;
            _renderer = renderer;
            _bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);

            if (preRender)
                Render();


        }

        /// <summary>
        /// Sets the renderer
        /// </summary>
        /// <param name="renderer">The render function</param>
        /// <param name="render">Default true, calls the renderer delegate</param>
        public void SetRenderer(Renderer renderer, bool render = true)
        {
            if (_renderer == renderer)
                return;

            _renderer = renderer;

            if (render)
                Render();
        }

        public void Render()
        {
            lock (_bitmap)
            {
                using (Graphics g = Graphics.FromImage(_bitmap))
                {
                    g.Clear(Color.Transparent);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.CompositingQuality = CompositingQuality.GammaCorrected;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    _renderer(g);
                }
            }
        }

        public void Draw(Graphics g)
        {
            Draw(g, 0, 0, Width, Height);
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
            if (_bitmap == null)
                return;

            lock (_bitmap)
            {
                g.DrawImage(_bitmap, x, y, width, height);
            }
        }

        public void Dispose()
        {
            _bitmap.Dispose();
        }
    }
}
