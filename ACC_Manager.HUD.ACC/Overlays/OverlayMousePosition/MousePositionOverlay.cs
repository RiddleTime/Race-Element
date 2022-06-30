using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using System.Runtime.InteropServices;

namespace ACCManager.HUD.ACC.Overlays.OverlayMousePosition
{
    public sealed class MousePositionOverlay : AbstractOverlay
    {
        private CachedBitmap _cachedCursor;
        private IKeyboardMouseEvents _globalKbmHook;

        private readonly CachedBitmap.Renderer MouseDownRenderer = g =>
        {
            g.DrawEllipse(Pens.White, 5, 5, 5);
            g.DrawEllipse(Pens.White, 5, 5, 3);
            g.FillEllipse(new SolidBrush(Color.FromArgb(140, Color.Red)), 5, 5, 5);
        };
        private readonly CachedBitmap.Renderer MouseUpRenderer = g =>
        {
            g.DrawEllipse(Pens.White, 5, 5, 5);
            g.DrawEllipse(Pens.White, 5, 5, 3);
            g.FillEllipse(new SolidBrush(Color.FromArgb(140, Color.White)), 5, 5, 5);
        };

        public MousePositionOverlay(Rectangle rectangle, string Name) : base(rectangle, Name)
        {
            this.Width = 11;
            this.Height = 11;
            this.RefreshRateHz = 60;
        }

        public sealed override void BeforeStart()
        {
            _cachedCursor = new CachedBitmap(Width, Height, MouseUpRenderer);
            _globalKbmHook = Hook.GlobalEvents();
            _globalKbmHook.MouseDown += GlobalMouseDown;
            _globalKbmHook.MouseUp += GlobalMouseUp;
            _globalKbmHook.MouseMove += GlobalMouseMove;

            this.X = GetCursorPosition().X - 5;
            this.Y = GetCursorPosition().Y - 5;
        }

        private void GlobalMouseMove(object sender, MouseEventArgs e)
        {
            this.X = e.Location.X - 5;
            this.Y = e.Location.Y - 5;
        }

        private void GlobalMouseUp(object sender, MouseEventArgs e)
        {
            _cachedCursor.SetRenderer(MouseUpRenderer);
        }

        private void GlobalMouseDown(object sender, MouseEventArgs e)
        {
            _cachedCursor.SetRenderer(MouseDownRenderer);
        }

        public sealed override void BeforeStop()
        {
            _globalKbmHook.MouseDown -= GlobalMouseDown;
            _globalKbmHook.MouseUp -= GlobalMouseUp;
            _globalKbmHook.MouseMove -= GlobalMouseMove;
            _globalKbmHook.Dispose();

            if (_cachedCursor != null)
                _cachedCursor.Dispose();
        }

        public sealed override void Render(Graphics g)
        {
            if (_cachedCursor != null)
                _cachedCursor.Draw(g, Width, Height);
        }

        public sealed override bool ShouldRender()
        {
            return true;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        private static Point GetCursorPosition()
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

