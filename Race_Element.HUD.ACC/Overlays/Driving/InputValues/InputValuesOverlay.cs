using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayInputValues;

[Overlay(Name = "Input Values",
    Description = "Shows raw Throttle and Brake data.",
    OverlayCategory = OverlayCategory.Inputs,
    OverlayType = OverlayType.Drive,
Authors = ["Reinier Klarenberg"]
    )]
internal class InputValuesOverlay : AbstractOverlay
{
    private readonly InputValuesConfiguration _config = new();
    private sealed class InputValuesConfiguration : OverlayConfiguration
    {
        public InputValuesConfiguration() => GenericConfiguration.AllowRescale = true;

        [ConfigGrouping("Format", "Adjust the text format")]
        public FormatGrouping Format { get; init; } = new FormatGrouping();
        public class FormatGrouping
        {
            [ToolTip("Change the amount of decimals shown.")]
            [IntRange(0, 3, 1)]
            public int Decimals { get; init; } = 2;
        }
    }

    private GraphicsGrid _graphicsGrid;
    private DrawableTextCell _throttleCell;
    private DrawableTextCell _brakeCell;

    public InputValuesOverlay(Rectangle rectangle) : base(rectangle, "Input Values")
    {
        RefreshRateHz = 20;
    }

    public override void SetupPreviewData()
    {
        pagePhysics.Gas = 0.9986f;
        pagePhysics.Brake = 0.0127f;
    }

    public override void BeforeStart()
    {
        _graphicsGrid = new GraphicsGrid(2, 1);

        Font font = FontUtil.FontSegoeMono(14f * Scale);
        string sample = 100f.ToString($"F{_config.Format.Decimals}");
        float fontWidth = FontUtil.MeasureWidth(font, sample);

        float baseWidth = fontWidth;
        float baseHeight = font.GetHeight(96);

        Color colorDefault = Color.FromArgb(190, Color.Black);
        Color colorRed = Color.FromArgb(150, Color.Red);
        Color colorGreen = Color.FromArgb(150, Color.LimeGreen);
        using HatchBrush columnBrushDefault = new(HatchStyle.LightUpwardDiagonal, colorDefault, Color.FromArgb(colorDefault.A - 25, colorDefault));
        using HatchBrush columnBrushRed = new(HatchStyle.LightUpwardDiagonal, colorDefault, Color.FromArgb(colorRed.A - 75, colorRed));
        using HatchBrush columnBrushGreen = new(HatchStyle.LightUpwardDiagonal, colorDefault, Color.FromArgb(colorGreen.A - 25, colorGreen));

        _throttleCell = new DrawableTextCell(new RectangleF(0, 0, baseWidth, baseHeight), font);
        _throttleCell.CachedBackground = new CachedBitmap((int)baseWidth, (int)baseHeight, g =>
        {
            Rectangle rect = new(0, 0, (int)baseWidth - 1, (int)baseHeight - 1);
            using LinearGradientBrush brush = new(new PointF(baseWidth, baseHeight), new PointF(0, 0), Color.FromArgb(90, 0, 0, 0), Color.FromArgb(colorGreen.A, 10, 10, 10));
            g.FillRoundedRectangle(brush, rect, (int)(2f * Scale));
            g.FillRoundedRectangle(columnBrushGreen, rect, (int)(2f * Scale));
            using Pen pen = new(colorGreen, 1f * Scale);
            g.DrawRoundedRectangle(pen, rect, (int)(2f * Scale));
        });
        _graphicsGrid.Grid[0][0] = _throttleCell;

        _brakeCell = new DrawableTextCell(new RectangleF(0, baseHeight, baseWidth, baseHeight), font);
        _brakeCell.CachedBackground = new CachedBitmap((int)baseWidth, (int)baseHeight, g =>
        {
            Rectangle rect = new(0, 0, (int)baseWidth - 1, (int)baseHeight - 1);
            using LinearGradientBrush brush = new(new PointF(baseWidth, baseHeight), new PointF(0, 0), Color.FromArgb(90, 0, 0, 0), Color.FromArgb(colorRed.A, 10, 10, 10));
            g.FillRoundedRectangle(brush, rect, (int)(2f * Scale));
            g.FillRoundedRectangle(columnBrushRed, rect, (int)(2f * Scale));
            using Pen pen = new(colorRed, 1f * Scale);
            g.DrawRoundedRectangle(pen, rect, (int)(2f * Scale));
        });
        _graphicsGrid.Grid[1][0] = _brakeCell;

        Width = (int)baseWidth;
        Height = (int)(baseHeight * 2f) + 1;
    }

    public override void BeforeStop()
    {
        _graphicsGrid?.Dispose();
        _throttleCell?.Dispose();
        _brakeCell?.Dispose();
    }

    public override void Render(Graphics g)
    {
        _throttleCell.CachedBackground.Opacity = 0.7f + 0.3f * pagePhysics.Gas;
        _throttleCell.UpdateText($"{(pagePhysics.Gas * 100f).ToString($"F{_config.Format.Decimals}")}");

        _brakeCell.CachedBackground.Opacity = 0.7f + 0.3f * pagePhysics.Brake;
        _brakeCell.UpdateText($"{(pagePhysics.Brake * 100f).ToString($"F{_config.Format.Decimals}")}");
        _graphicsGrid.Draw(g);
    }
}
