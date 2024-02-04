using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayGridTest;

[Overlay(Name = "Grid Test", Description = "Testing of the grid", OverlayType = OverlayType.Pitwall)]
internal sealed class GridTestOverlay : AbstractOverlay
{
    private readonly GridTestConfiguration _config = new();
    private sealed class GridTestConfiguration : OverlayConfiguration
    {
        public GridTestConfiguration() => GenericConfiguration.AllowRescale = true;

        [ConfigGrouping("Grid", "Customize rows and columns")]
        public GridGrouping Grid { get; set; } = new GridGrouping();
        public sealed class GridGrouping
        {
            [IntRange(1, 50, 1)]
            public int Rows { get; set; } = 20;

            [IntRange(1, 50, 1)]
            public int Columns { get; set; } = 20;

            [IntRange(1, 50, 1)]
            public int RefreshRate { get; set; } = 30;
        }
    }

    private readonly GraphicsGrid testgrid;
    private Font Font;

    public GridTestOverlay(Rectangle rectangle) : base(rectangle, "Grid Test")
    {
        testgrid = new GraphicsGrid(_config.Grid.Rows, _config.Grid.Columns);
        RefreshRateHz = _config.Grid.RefreshRate;
    }

    public override void BeforeStart()
    {
        Font = FontUtil.FontSegoeMono(12 * Scale);
        float fontHeight = Font.GetHeight(120);
        int columnWidth = (int)fontHeight;
        int columnHeight = (int)fontHeight;
        Random rand = new();

        for (int row = 0; row < testgrid.Rows; row++)
            for (int column = 0; column < testgrid.Columns; column++)
            {
                DrawableTextCell cell = new(new RectangleF(column * columnWidth, row * columnHeight, columnWidth, columnHeight), Font);

                cell.CachedBackground = new CachedBitmap((int)cell.Rectangle.Width, (int)cell.Rectangle.Height, g =>
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(230 + rand.Next(25), rand.Next(185), rand.Next(75))), new Rectangle(0, 0, (int)cell.Rectangle.Width, (int)cell.Rectangle.Height));
                });
                cell.CachedBackground.Opacity = (float)(rand.NextDouble() / 2 + 0.5d);
                cell.UpdateText($"{rand.Next(10)}");

                testgrid.Grid[row][column] = cell;
            }
        Width = (int)(testgrid.Columns * columnWidth * Scale);
        Height = (int)(testgrid.Rows * columnHeight * Scale);
    }

    public override void BeforeStop()
    {
        testgrid?.Dispose();
    }

    public override bool ShouldRender() => true;
    public override void Render(Graphics g)
    {
        Random rand = new();
        for (int row = 0; row < testgrid.Rows; row++)
            for (int column = 0; column < testgrid.Columns; column++)
            {
                DrawableTextCell cell = (DrawableTextCell)testgrid.Grid[row][column];
                if (column == 0)
                    cell.CachedBackground.SetRenderer(g => g.FillRectangle(new SolidBrush(Color.FromArgb(230 + rand.Next(25), rand.Next(185), rand.Next(75))), new Rectangle(0, 0, (int)cell.Rectangle.Width, (int)cell.Rectangle.Height)));
                else
                    cell.UpdateText($"{(rand.Next(50) == 1 ? 1 : 0)}");
            }

        testgrid.Draw(g, Scale);
    }
}
