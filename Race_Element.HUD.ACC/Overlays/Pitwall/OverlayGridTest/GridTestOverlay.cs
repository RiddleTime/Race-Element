using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using System;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayGridTest
{
#if DEBUG
    [Overlay(Name = "Grid Test", Description = "Testing of the grid", OverlayType = OverlayType.Release)]
#endif
    internal sealed class GridTestOverlay : AbstractOverlay
    {
        private readonly GridTestConfiguration _config = new GridTestConfiguration();
        private sealed class GridTestConfiguration : OverlayConfiguration
        {
            public GridTestConfiguration() => AllowRescale = true;

            [ConfigGrouping("Grid", "Customize rows and columns")]
            public GridGrouping Grid { get; set; } = new GridGrouping();
            public sealed class GridGrouping
            {
                [IntRange(1, 50, 1)]
                public int Rows { get; set; } = 20;

                [IntRange(1, 50, 1)]
                public int Columns { get; set; } = 20;
            }

        }

        private readonly GraphicsGrid testgrid;

        public GridTestOverlay(Rectangle rectangle) : base(rectangle, "Grid Test") => testgrid = new GraphicsGrid(_config.Grid.Rows, _config.Grid.Columns);

        public override void BeforeStart()
        {
            int columnWidth = 10;
            int columnHeight = 10;
            Random rand = new Random();

            for (int row = 0; row < testgrid.Rows; row++)
                for (int column = 0; column < testgrid.Columns; column++)
                {
                    DrawableCell cell = new DrawableCell(new RectangleF(row * columnWidth, column * columnHeight, columnWidth, columnHeight));

                    cell.CachedBackground = new CachedBitmap((int)cell.Rectangle.Width, (int)cell.Rectangle.Height, g =>
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255))), new Rectangle(0, 0, (int)cell.Rectangle.Width, (int)cell.Rectangle.Height));
                    });

                    testgrid.Grid[row][column] = cell;
                }

            Width = testgrid.Rows * columnWidth;
            Height = testgrid.Columns * columnHeight;
        }

        public override void BeforeStop() => testgrid?.Dispose();

        public override void Render(Graphics g) => testgrid?.Draw(g);
    }
}
