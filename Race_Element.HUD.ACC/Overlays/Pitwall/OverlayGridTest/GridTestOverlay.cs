using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using System;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayGridTest
{
    [Overlay(Name = "Grid Test", Description = "Testing of the grid", OverlayType = OverlayType.Pitwall)]
    internal sealed class GridTestOverlay : AbstractOverlay
    {
        private readonly GraphicsGrid testgrid;

        public GridTestOverlay(Rectangle rectangle) : base(rectangle, "Grid Test") => testgrid = new GraphicsGrid(10, 10);

        public override void BeforeStart()
        {
            int columnWidth = 10;
            int columnHeight = 10;
            Random rand = new Random();
            for (int row = 0; row < testgrid.Rows; row++)
                for (int column = 0; column < testgrid.Columns; column++)
                {
                    GridCell cell = new GridCell(new RectangleF(row * columnWidth, column * columnHeight, columnWidth, columnHeight));

                    cell.CachedBackground = new CachedBitmap((int)cell.Rectangle.Width, (int)cell.Rectangle.Height, g =>
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255))), new Rectangle(0, 0, (int)cell.Rectangle.Width, (int)cell.Rectangle.Height));
                    });

                    testgrid.Grid[row][column] = cell;
                }
        }

        public override void Render(Graphics g) => testgrid?.Draw(g);
    }
}
