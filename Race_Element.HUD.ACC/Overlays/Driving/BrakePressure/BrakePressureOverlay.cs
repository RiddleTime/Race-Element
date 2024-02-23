﻿using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.OverlayUtil.ProgressBars;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.BrakePressure;

[Overlay(Name = "Brake Pressure",
Description = "Shows the applied brake pressure percentage for all 4 brakes.",
OverlayType = OverlayType.Drive,
OverlayCategory = OverlayCategory.Inputs,
Authors = ["Reinier Klarenberg"]
)]
internal sealed class BrakePressureOverlay : AbstractOverlay
{
    private readonly BrakePressureConfiguration _config = new();

    private readonly HorizontalProgressBar[] _brakePressureBars = new HorizontalProgressBar[4];
    private readonly Point[] _barLocations = new Point[4];
    private readonly DrawableTextCell[] _brakePressureTexts = new DrawableTextCell[4];
    private Font _font;

    private SolidBrush _textBrush;
    private SolidBrush _fillBrush;
    private SolidBrush _outlineBrush;
    private SolidBrush _backgroundBrush;

    public BrakePressureOverlay(Rectangle rectangle) : base(rectangle, "Brake Pressure")
    {
        RefreshRateHz = 20;
    }

    public sealed override void SetupPreviewData()
    {
        pagePhysics.brakePressure[0] = 0.647f;
        pagePhysics.brakePressure[1] = 0.646f;
        pagePhysics.brakePressure[2] = 0.323f;
        pagePhysics.brakePressure[3] = 0.321f;
    }

    public sealed override void BeforeStart()
    {
        int barWidth = 80;
        int barHeight = 20;

        _textBrush = new SolidBrush(Color.FromArgb(_config.Colors.TextOpacity, _config.Colors.TextColor));
        _fillBrush = new SolidBrush(Color.FromArgb(_config.Colors.FillOpacity, _config.Colors.FillColor));
        _outlineBrush = new SolidBrush(Color.FromArgb(_config.Colors.OutlineOpacity, _config.Colors.OutlineColor));
        _backgroundBrush = new SolidBrush(Color.FromArgb(_config.Colors.BackgroundOpacity, _config.Colors.BackgroundColor));

        for (int i = 0; i < 4; i++)
            _brakePressureBars[i] = new HorizontalProgressBar(barWidth, barHeight)
            {
                OutlineBrush = _outlineBrush,
                BackgroundBrush = _backgroundBrush,
                FillBrush = _fillBrush,
                LeftToRight = (i % 2 == 0),
                DrawBackground = true,
            };

        _barLocations[0] = new Point(0, 0);
        _barLocations[1] = new Point(barWidth, 0);
        _barLocations[2] = new Point(0, barHeight);
        _barLocations[3] = new Point(barWidth, barHeight);

        _font = FontUtil.FontSegoeMono(12);
        Rectangle[] textRects =
        [
            new Rectangle(0, 0, barWidth, barHeight),
            new Rectangle(barWidth, 0, barWidth, barHeight),
            new Rectangle(0, barHeight, barWidth, barHeight),
            new Rectangle(barWidth, barHeight, barWidth, barHeight),
        ];
        for (int i = 0; i < 4; i++)
            _brakePressureTexts[i] = new DrawableTextCell(textRects[i], _font)
            {
                StringFormat = new StringFormat()
                {
                    Alignment = (i % 2 == 0 ? StringAlignment.Near : StringAlignment.Far)
                },
                TextBrush = _textBrush
            };


        Width = barWidth * 2;
        Height = barHeight * 2;
    }

    public sealed override void BeforeStop()
    {
        for (int i = 0; i < 4; i++)
        {
            _brakePressureTexts[i]?.Dispose();
            _brakePressureBars[i]?.DisposeBitmaps();
        }

        _textBrush?.Dispose();
        _fillBrush?.Dispose();
        _outlineBrush?.Dispose();
        _backgroundBrush?.Dispose();
        _font?.Dispose();
    }

    public sealed override void Render(Graphics g)
    {
        for (int i = 0; i < _brakePressureBars.Length; i++)
        {
            _brakePressureBars[i].Value = pagePhysics.brakePressure[i];
            _brakePressureBars[i].Draw(g, _barLocations[i].X, _barLocations[i].Y);
            _brakePressureTexts[i]?.UpdateText($"{pagePhysics.brakePressure[i] * 100f:F1}");
            _brakePressureTexts[i]?.Draw(g);
        }
    }
}