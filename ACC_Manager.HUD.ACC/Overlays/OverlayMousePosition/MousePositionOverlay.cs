﻿using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACCManager.HUD.ACC.Overlays.OverlayMousePosition
{
    public sealed class MousePositionOverlay : AbstractOverlay
    {
        public MousePositionOverlay(Rectangle rectangle, string Name) : base(rectangle, Name)
        {
            this.Width = ScreenWidth;
            this.Height = ScreenHeight;
            this.RefreshRateHz = 144;
        }

        public sealed override void BeforeStart()
        {
        }

        public sealed override void BeforeStop()
        {
        }

        public sealed override void Render(Graphics g)
        {
            Point cursorPosition = GetCursorPosition();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawEllipse(Pens.White, cursorPosition.X, cursorPosition.Y, 5);
            g.FillEllipse(new SolidBrush(Color.FromArgb(120, Color.Red)), cursorPosition.X, cursorPosition.Y, 5);
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
