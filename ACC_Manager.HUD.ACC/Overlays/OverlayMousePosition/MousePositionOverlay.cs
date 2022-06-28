using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACCManager.HUD.ACC.Overlays.OverlayMousePosition
{
    public sealed class MousePositionOverlay : AbstractOverlay
    {
        private Bitmap _cursor;

        public MousePositionOverlay(Rectangle rectangle, string Name) : base(rectangle, Name)
        {
            this.Width = 15;
            this.Height = 15;
            this.RefreshRateHz = 60;
        }

        public sealed override void BeforeStart()
        {
            _cursor = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);

            using (Graphics g = Graphics.FromImage(_cursor))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;

                g.DrawEllipse(Pens.White, 5, 5, 5);
                g.FillEllipse(new SolidBrush(Color.FromArgb(120, Color.Red)), 5, 5, 5);
            };
        }

        public sealed override void BeforeStop()
        {
            _cursor.Dispose();
        }

        public sealed override void Render(Graphics g)
        {
            Point cursorPosition = GetCursorPosition();
            this.X = cursorPosition.X - 5;
            this.Y = cursorPosition.Y - 5;

            if (_cursor != null && _cursor.PixelFormat == PixelFormat.Format32bppPArgb)
                g.DrawImage(_cursor, 0, 0, Width, Height);
        }

        public sealed override bool ShouldRender()
        {
            return true;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            // NOTE: If you need error handling
            // bool success = GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }
    }
}
