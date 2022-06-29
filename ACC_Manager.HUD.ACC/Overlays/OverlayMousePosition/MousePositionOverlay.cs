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
        private CachedBitmap _cachedCursor;

        public MousePositionOverlay(Rectangle rectangle, string Name) : base(rectangle, Name)
        {
            this.Width = 11;
            this.Height = 11;
            this.RefreshRateHz = 60;
        }

        public sealed override void BeforeStart()
        {
            _cachedCursor = new CachedBitmap(Width, Height, g =>
            {
                g.DrawEllipse(Pens.White, 5, 5, 5);
                g.DrawEllipse(Pens.White, 5, 5, 3);
                g.FillEllipse(new SolidBrush(Color.FromArgb(140, Color.Red)), 5, 5, 5);
            });
        }

        public sealed override void BeforeStop()
        {
            if (_cachedCursor != null)
                _cachedCursor.Dispose();
        }

        public sealed override void Render(Graphics g)
        {
            Point cursorPosition = GetCursorPosition();
            this.X = cursorPosition.X - 5;
            this.Y = cursorPosition.Y - 5;

            _cachedCursor.Draw(g, Width, Height);
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
