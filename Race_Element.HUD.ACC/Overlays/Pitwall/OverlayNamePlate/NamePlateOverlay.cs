using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayNamePlate;

#if DEBUG
[Overlay(Name = "Name plate", Description = "A plate/bar")]
#endif
internal sealed class NamePlateOverlay : AbstractOverlay
{
    private readonly NamePlateConfiguration _config = new();
    private sealed class NamePlateConfiguration : OverlayConfiguration
    {

    }

    private CachedBitmap _background;
    private Font _fontBig;
    private Font _fontSmall;

    public NamePlateOverlay(Rectangle rectangle) : base(rectangle, "Name Plate")
    {
        Width = 800;
        Height = 150;
    }

    public override void BeforeStart()
    {
        _fontBig = FontUtil.FontSegoeMono(25);
        _fontSmall = FontUtil.FontSegoeMono(13);

        _background = new CachedBitmap(Width, Height, g =>
        {
            int diagDist = 20;
            int x = 5;
            int y = 5;
            int width = x + 750;
            int height = y + 110;

            List<Point> points = [
                new(x, diagDist),
                new(diagDist, y),
                new(width, y),
                new(width, height - diagDist),
                new(width - diagDist, height),
                new(x, height),
            ];
            using GraphicsPath path = new();
            path.AddPolygon(points.ToArray());

            g.FillPath(Brushes.Black, path);

            using SolidBrush outlineBrush = new(Color.FromArgb(255, 96, 96, 96));
            using Pen pen = new(outlineBrush, 4.5f);
            g.DrawPath(pen, path);

        });
    }

    public override void Render(Graphics g)
    {
        _background?.Draw(g, new(0, 0));


        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        g.DrawStringWithShadow("René Buttler", _fontBig, Brushes.White, new PointF(Width / 2, 25), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        g.DrawStringWithShadow("OVALTAKE Racing Team", _fontBig, Brushes.White, new PointF(Width / 2, 50), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });


    }
}
