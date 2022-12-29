using System.Drawing;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.Overlay.Internal
{
    public class WindowStructs
    {
        /// <summary>
        /// Monitor information.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public uint size;
            public RECT monitor;
            public RECT work;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.X);
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public override string ToString()
            {
                return $"X: {left}, Y: {top}, Width: {right - left}, Height: {bottom - top}";
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;
        }
    }
}
