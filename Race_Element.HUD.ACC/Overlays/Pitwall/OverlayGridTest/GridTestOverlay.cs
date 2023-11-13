using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayGridTest
{
    [Overlay(Name = "Grid Test", Description = "Testing of the grid", OverlayType = OverlayType.Pitwall)]
    internal sealed class GridTestOverlay : AbstractOverlay
    {
        private readonly GraphicsGrid testgrid;

        public GridTestOverlay(Rectangle rectangle) : base(rectangle, "Grid Test") => testgrid = new GraphicsGrid(5, 10);

        public override void BeforeStart()
        {
            for (int row = 0; row < testgrid.Rows; row++)
                for (int column = 0; column < testgrid.Columns; column++)
                {
                    GridCell cell = new GridCell(new RectangleF(row * 10, column * 5, 10, 10));

                    cell.CachedBackground = new CachedBitmap((int)cell.Rectangle.Width, (int)cell.Rectangle.Height, g =>
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(170, Color.OrangeRed)), new Rectangle(0, 0, (int)cell.Rectangle.Width, (int)cell.Rectangle.Height));
                    });

                    testgrid.Grid[row][column] = cell;
                }
        }

        public override void Render(Graphics g) => testgrid?.Draw(g);
    }
}
