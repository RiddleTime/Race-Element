using System;
using System.Drawing;

namespace RaceElement.HUD.Overlay.OverlayUtil.Drawing
{
    public interface IDrawable : IDisposable
    {
        void Draw(Graphics g);
    }
}