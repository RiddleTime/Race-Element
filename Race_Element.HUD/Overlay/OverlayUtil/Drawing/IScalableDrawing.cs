using System;
using System.Drawing;

namespace RaceElement.HUD.Overlay.OverlayUtil.Drawing;

public interface IScalableDrawing : IDisposable
{
    public void Draw(Graphics g, float scaling = 1);
}