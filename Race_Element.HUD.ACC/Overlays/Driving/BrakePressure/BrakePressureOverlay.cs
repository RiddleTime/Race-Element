using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.OverlayUtil.ProgressBars;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.Driving.BrakePressure;

[Overlay(Name = "Brake Pressure",
Description = "Shows the applied brake pressure, front is left and rear is right.",
OverlayType = OverlayType.Drive,
OverlayCategory = OverlayCategory.Inputs,
Authors = ["Reinier Klarenberg"]
)]
internal sealed class BrakePressureOverlay(Rectangle rectangle) : ACCOverlay(rectangle, "Brake Pressure")
{
    private readonly BrakePressureConfiguration _config = new();

    private readonly HorizontalProgressBar[] _brakePressureBars = new HorizontalProgressBar[2];
    private readonly Point[] _barLocations = new Point[2];
    private readonly DrawableTextCell[] _brakePressureTexts = new DrawableTextCell[2];
    private Font _font;

    private SolidBrush _textBrush;
    private SolidBrush _frontBrush;
    private SolidBrush _rearBrush;
    private SolidBrush _outlineBrush;
    private SolidBrush _backgroundBrush;
    public sealed override void SetupPreviewData()
    {
        pagePhysics.brakePressure[0] = 0.546f;
        pagePhysics.brakePressure[1] = 0.546f;
        pagePhysics.brakePressure[2] = 0.283f;
        pagePhysics.brakePressure[3] = 0.283f;
    }

    public sealed override void BeforeStart()
    {
        int barWidth = 200;
        int barHeight = 30;

        _textBrush = new SolidBrush(Color.FromArgb(_config.Colors.TextOpacity, _config.Colors.TextColor));
        _frontBrush = new SolidBrush(Color.FromArgb(_config.Colors.FrontFillOpacity, _config.Colors.FrontFillColor));
        _rearBrush = new SolidBrush(Color.FromArgb(_config.Colors.RearFillOpacity, _config.Colors.RearFillColor));
        _outlineBrush = new SolidBrush(Color.FromArgb(_config.Colors.OutlineOpacity, _config.Colors.OutlineColor));
        _backgroundBrush = new SolidBrush(Color.FromArgb(_config.Colors.BackgroundOpacity, _config.Colors.BackgroundColor));

        for (int i = 0; i < 2; i++)
            _brakePressureBars[i] = new HorizontalProgressBar(barWidth, barHeight)
            {
                OutlineBrush = _outlineBrush,
                BackgroundBrush = _backgroundBrush,
                FillBrush = !(i % 2 != 0) ? _frontBrush : _rearBrush,
                LeftToRight = i % 2 == 0,
                DrawBackground = !(i % 2 != 0),
                DrawOutline = (i % 2 != 0)
            };

        _barLocations[0] = new Point(0, 0);
        _barLocations[1] = new Point(0, 0);

        _font = FontUtil.FontSegoeMono(18);
        for (int i = 0; i < 2; i++)
            _brakePressureTexts[i] = new DrawableTextCell(new Rectangle(0, 0, barWidth, barHeight), _font)
            {
                StringFormat = new StringFormat()
                {
                    Alignment = i % 2 == 0 ? StringAlignment.Near : StringAlignment.Far,
                    LineAlignment = StringAlignment.Center,
                },
                TextBrush = _textBrush
            };


        Width = barWidth;
        Height = barHeight;
        RefreshRateHz = 20;
    }

    public sealed override void BeforeStop()
    {
        for (int i = 0; i < 2; i++)
        {
            _brakePressureTexts[i]?.Dispose();
            _brakePressureBars[i]?.DisposeBitmaps();
        }

        _textBrush?.Dispose();
        _frontBrush?.Dispose();
        _rearBrush?.Dispose();
        _outlineBrush?.Dispose();
        _backgroundBrush?.Dispose();

        _font?.Dispose();
    }

    public sealed override void Render(Graphics g)
    {
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        for (int i = 0; i < 2; i++)
        {
            _brakePressureBars[i].Value = pagePhysics.brakePressure[i * 2];
            _brakePressureBars[i].Draw(g, _barLocations[i].X, _barLocations[i].Y);
            _brakePressureTexts[i]?.UpdateText($"{pagePhysics.brakePressure[i * 2] * 100f:F1}");
            _brakePressureTexts[i]?.Draw(g);
        }
    }
}
