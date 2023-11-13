using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using System;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayGridTest
{
    [Overlay(Name = "Grid Test", Description = "Testing of the grid", OverlayType = OverlayType.Release)]
    internal sealed class GridTestOverlay : AbstractOverlay
    {
        private readonly GridTestConfiguration _config = new GridTestConfiguration();
        private sealed class GridTestConfiguration : OverlayConfiguration
        {
            public GridTestConfiguration() => AllowRescale = false;

            [ConfigGrouping("Grid", "Customize rows and columns")]
            public GridGrouping Grid { get; set; } = new GridGrouping();
            public sealed class GridGrouping
            {
                [IntRange(4, 50, 2)]
                public int Rows { get; set; } = 20;

                [IntRange(4, 50, 2)]
                public int Columns { get; set; } = 20;
            }

        }

        private readonly GraphicsGrid testgrid;

        public GridTestOverlay(Rectangle rectangle) : base(rectangle, "Grid Test")
        {
            testgrid = new GraphicsGrid(_config.Grid.Rows, _config.Grid.Columns);
            RefreshRateHz = 1;
        }

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
                        g.FillRectangle(new SolidBrush(Color.FromArgb(230 + rand.Next(25), rand.Next(185), rand.Next(75))), new Rectangle(0, 0, (int)cell.Rectangle.Width, (int)cell.Rectangle.Height));
                    });
                    cell.CachedBackground.Opacity = (float)(rand.NextDouble() / 2 + 0.5d);

                    testgrid.Grid[row][column] = cell;
                }
            Width = (int)(testgrid.Rows * columnWidth * Scale);
            Height = (int)(testgrid.Columns * columnHeight * Scale);
        }

        public override void BeforeStop() => testgrid?.Dispose();

        public override bool ShouldRender() => true;

        public override void Render(Graphics g)
        {
            Random rand = new Random();
            double opacityRange = 0.85;
            for (int row = 0; row < testgrid.Rows; row++)
                for (int column = 0; column < testgrid.Columns; column++)
                {
                    testgrid.Grid[row][column].CachedBackground.Opacity = (float)(rand.NextDouble() * opacityRange + 1 - opacityRange);
                }

            testgrid.Draw(g, (float)(Scale));
        }
    }
}
