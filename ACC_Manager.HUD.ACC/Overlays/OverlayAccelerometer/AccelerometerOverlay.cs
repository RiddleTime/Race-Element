using ACC_Manager.Util.SystemExtensions;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayAccelerometer
{
    internal sealed class AccelerometerOverlay : AbstractOverlay
    {
        private AccelleroConfig _config = new AccelleroConfig();
        private class AccelleroConfig : OverlayConfiguration
        {
            [ToolTip("Displays fading dots representing history of the g-forces.")]
            internal bool ShowTrace { get; set; } = true;

            [ToolTip("Displays the lateral and longitudinal g-forces as text.")]
            internal bool ShowText { get; set; } = false;

            public AccelleroConfig()
            {
                this.AllowRescale = true;
            }
        }

        private readonly InfoPanel _panel;
        private const int MaxG = 3;
        private int _gMeterX = 22;
        private int _gMeterY = 22;
        private int _gMeterSize = 200;
        private readonly LinkedList<Point> _trace = new LinkedList<Point>();

        public AccelerometerOverlay(Rectangle rectangle) : base(rectangle, "Accelerometer Overlay")
        {
            this.Width = 225;
            this.Height = this.Width;
            this.RefreshRateHz = 20;

            if (!this._config.ShowText)
            {
                this.Width = _gMeterSize + 1;
                this.Height = this.Width;
                _gMeterX = 0;
                _gMeterY = 0;
            }
            else
                _panel = new InfoPanel(10, 62) { DrawBackground = true, DrawRowLines = false };
        }

        private Bitmap _background = null;
        public sealed override void BeforeStart()
        {
            _background = new Bitmap(this.Width, this.Height);
          
            using (Graphics g = Graphics.FromImage(_background))
            {
                SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(140, Color.Black));

                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (this._config.ShowText)
                    g.FillEllipse(backgroundBrush, new Rectangle(_gMeterX, _gMeterY, _gMeterSize, _gMeterSize));
                else
                    g.FillEllipse(backgroundBrush, new Rectangle(0, 0, this.Width - 1, this.Height - 1));

                //Draws the lines and circles
                Pen AccPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1);
                Pen AccPen2 = new Pen(Color.FromArgb(30, 255, 255, 255), 3);
                Pen AccPen3 = new Pen(Color.FromArgb(100, 255, 255, 255), 4);
                Pen AccPen4 = new Pen(Color.FromArgb(200, 200, 200, 200), 5);

                int size = _gMeterSize;
                int x = _gMeterX;
                int y = _gMeterY;

                g.SmoothingMode = SmoothingMode.HighQuality;

                g.DrawLine(AccPen, x, y + size / 2, X + size, y + size / 2);
                g.DrawLine(AccPen, x + size / 2, y, x + size / 2, y + size);
                g.DrawEllipse(AccPen4, x + 2, y + 2, size - 4, size - 4);
                g.DrawEllipse(AccPen3, x + size / 6, y + size / 6, (size / 3) * 2, (size / 3) * 2);
                g.DrawEllipse(AccPen2, x + size / 3, y + size / 3, size / 3, size / 3);
            }

            GC.Collect();
        }

        public sealed override void BeforeStop()
        {
            _background.Dispose();
        }

        public sealed override void Render(Graphics g)
        {
            if (_background != null)
                g.DrawImage(_background, 0, 0);

            if (this._config.ShowText)
            {
                _panel.AddLine("X ", $"{pagePhysics.AccG[0]:F2}");
                _panel.AddLine("Y ", $"{pagePhysics.AccG[2]:F2}");

                _panel.Draw(g);
            }

            DrawGMeter(_gMeterX, _gMeterY, _gMeterSize, g);
        }

        /// <summary>
        /// Returns a percentage, mininum -100% and max 100%
        /// </summary>
        /// <param name="max"></param>
        /// <param name="value"></param>
        /// <returns>a value between -1 and 1 (inclusive)</returns>
        public float GetPercentage(float max, float value)
        {
            float percentage = value * 100 / max / 100;
            percentage.Clip(-1, 1);
            return percentage;
        }

        private void DrawGMeter(int x, int y, int size, Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            //Draws the 'dot'
            int gDotSize = 14;

            double xPercentage = GetPercentage(MaxG, pagePhysics.AccG[0]);
            double yPercentage = GetPercentage(MaxG, pagePhysics.AccG[2]);

            double direction = Math.Atan2(xPercentage, yPercentage);
            double magnitude = Math.Sqrt(xPercentage * xPercentage + yPercentage * yPercentage);
            magnitude.Clip(-1, 1);

            double horizontalPlacement = Math.Sin(direction) * magnitude;
            double verticalPlacement = Math.Cos(direction) * magnitude;
            verticalPlacement.Clip(-1, 1);
            horizontalPlacement.Clip(-1, 1);

            PointF middle = new PointF(x + size / 2, y + size / 2);
            int gDotPosX = (int)(middle.X + (size / 2 * horizontalPlacement) - (gDotSize / 2));
            int gDotPosY = (int)(middle.Y + (size / 2 * verticalPlacement) - (gDotSize / 2));

            g.FillEllipse(new SolidBrush(Color.FromArgb(242, 82, 2)), new Rectangle(gDotPosX, gDotPosY, gDotSize, gDotSize));

            if (_config.ShowTrace)
            {
                lock (_trace)
                {
                    _trace.AddFirst(new Point(gDotPosX, gDotPosY));
                    if (_trace.Count > 10)
                        _trace.RemoveLast();


                    for (int i = 0; i < _trace.Count; i++)
                    {
                        Point traceItem = _trace.ElementAt(i);
                        g.FillEllipse(new SolidBrush(Color.FromArgb(90 - i * 5, 242, 82, 2)), new Rectangle(traceItem.X, traceItem.Y, gDotSize, gDotSize));
                    }
                }
            }
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
