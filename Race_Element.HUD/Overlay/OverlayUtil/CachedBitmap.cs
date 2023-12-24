using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace RaceElement.HUD.Overlay.OverlayUtil
{
    public sealed class CachedBitmap : IDisposable
    {
        public int Width;
        public int Height;
        public float Opacity { get; set; }
        public delegate void Renderer(Graphics g);

        private Bitmap _bitmap;
        private Renderer _renderer;

        /// <summary>
        /// Creates a cached bitmap using the given renderer
        /// </summary>
        /// <param name="width">The Width of the bitmap</param>
        /// <param name="height">The Height of the bitmap</param>
        /// <param name="renderer">The render function</param>
        /// <param name="preRender">Default true, calls the renderer delegate</param>
        public CachedBitmap(int width, int height, Renderer renderer, float opacity = 1.0f, bool preRender = true)
        {
            this.Width = width;
            this.Height = height;
            this.Opacity = opacity;
            _renderer = renderer;

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
            _bitmap ??= new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);

            lock (_bitmap)
            {
                using (Graphics g = Graphics.FromImage(_bitmap))
                {
                    g.Clear(Color.Transparent);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.CompositingQuality = CompositingQuality.GammaCorrected;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

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
                if (Opacity != 1f)
                {
                    ColorMatrix colormatrix = new() { Matrix33 = Opacity };
                    using ImageAttributes imgAttribute = new();
                    imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(_bitmap, new Rectangle(x, y, width, height), 0, 0, Width, Height, GraphicsUnit.Pixel, imgAttribute);
                }
                else
                    g.DrawImage(_bitmap, x, y, width, height);
            }
        }

        public void Dispose()
        {
            _bitmap?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
