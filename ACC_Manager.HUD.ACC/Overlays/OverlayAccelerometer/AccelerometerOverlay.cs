using ACC_Manager.Util.NumberExtensions;
using ACCManager.Data.ACC.Session;
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

        private readonly InfoPanel _panel = new InfoPanel(10, 65) { DrawBackground = false, DrawRowLines = false };
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
        }

        public sealed override void BeforeStart()
        {

        }
        public sealed override void BeforeStop() { }

        public sealed override void Render(Graphics g)
        {
            SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(140, Color.Black));
            //Draws the HUD window
            if (this._config.ShowText)
                g.FillRectangle(backgroundBrush, new Rectangle(0, 0, this._gMeterSize + 25, this._gMeterSize + 25));
            else
                g.FillEllipse(backgroundBrush, new Rectangle(1, 1, this._gMeterSize - 2, this._gMeterSize - 2));

            DrawGMeter(_gMeterX, _gMeterY, _gMeterSize, g);

            if (this._config.ShowText)
            {
                _panel.AddLine("X ", $"{pagePhysics.AccG[0]:F2}");
                _panel.AddLine("Y ", $"{pagePhysics.AccG[2]:F2}");

                _panel.Draw(g);
            }
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
            int gMeterX = x;
            int gMeterY = y;


            //Draws the lines and circles
            Pen AccPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1);
            Pen AccPen2 = new Pen(Color.FromArgb(30, 255, 255, 255), 3);
            Pen AccPen3 = new Pen(Color.FromArgb(100, 255, 255, 255), 4);
            Pen AccPen4 = new Pen(Color.FromArgb(200, 200, 200, 200), 5);

            g.DrawLine(AccPen, 0 + gMeterX, gMeterY + size / 2, gMeterX + size, gMeterY + size / 2);
            g.DrawLine(AccPen, gMeterX + size / 2, gMeterY, gMeterX + size / 2, gMeterY + size);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.DrawEllipse(AccPen4, gMeterX + 2, gMeterY + 2, size - 4, size - 4);
            g.DrawEllipse(AccPen3, gMeterX + size / 6, gMeterY + size / 6, (size / 3) * 2, (size / 3) * 2);
            g.DrawEllipse(AccPen2, gMeterX + size / 3, gMeterY + size / 3, size / 3, size / 3);


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

            if (this._config.ShowTrace)
            {
                lock (_trace)
                {
                    Point currentPoint = new Point(gDotPosX, gDotPosY);
                    switch (_trace.Count)
                    {
                        case 0: _trace.AddFirst(currentPoint); break;
                        default:
                            {
                                for (int i = 0; i < _trace.Count; i++)
                                {
                                    Point traceItem = _trace.ElementAt(i);
                                    int alpha = 90 - i * 5;
                                    alpha.Clip(0, 255);
                                    g.FillEllipse(new SolidBrush(Color.FromArgb(alpha, 242, 82, 2)), new Rectangle(traceItem.X, traceItem.Y, gDotSize, gDotSize));
                                }

                                if (!_trace.Last.Equals(new Point(gDotPosX, gDotPosY)))
                                    _trace.AddFirst(new Point(gDotPosX, gDotPosY));

                                break;
                            }
                    }


                }
            }
        }

        public sealed override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif

            bool shouldRender = true;
            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            if (pageGraphics.GlobalRed)
                shouldRender = false;

            if (RaceSessionState.IsPreSession(pageGraphics.GlobalRed, broadCastRealTime.Phase))
                shouldRender = true;

            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE)
                shouldRender = false;

            return shouldRender;
        }
    }
}
