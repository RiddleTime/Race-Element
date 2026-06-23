using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace RaceElement.HUD.Common.Overlays.Flight.IndicatedAirSpeed;

[Overlay(
    Name = "Indicated Air Speed",
    Description = "The air speed as indicated by the planes instruments.",
    SupportedGames = Game.MicrosoftFlightSimulator2020 | Game.MicrosoftFlightSimulator2024,
    Authors = ["Reinier Klarenberg"]
)]
internal sealed class IndicatedAirSpeedOverlay(Rectangle rectangle) : CommonAbstractOverlay(rectangle, "Indicated Air Speed")
{
    private readonly IndicatedAirSpeedConfiguration _config = new();

    private CachedBitmap? _cachedBackground;
    private NumberBitmaps? _bitmaps;

    // Precomputed powers of 10 for fast digit extraction (covers up to 7 digits)
    private static readonly int[] _powersOfTen = { 1, 10, 100, 1000, 10000, 100000, 1000000 };

    public override void BeforeStart()
    {
        RefreshRateHz = _config.General.RefreshRate;

        _bitmaps = new NumberBitmaps(_config);
        Width = _config.General.Digits * _bitmaps.BitmapDimension.Width + _config.General.ExtraDigitSpacing * (_config.General.Digits - 1);
        Height = _bitmaps.BitmapDimension.Height;

        _cachedBackground = new(Width, Height, g =>
        {
            RectangleF barArea = new(0, 0, Width - 1, Height - 1);

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using SolidBrush darkBrush = new(Color.FromArgb(_config.Colors.BackgroundOpacity, _config.Colors.BackgroundColor));
            g.FillRoundedRectangle(darkBrush, Rectangle.Round(barArea), 3);
            using Pen darkPen = new(darkBrush, 1);
            g.DrawRoundedRectangle(darkPen, Rectangle.Round(barArea), 3);
        });
    }

    public override void BeforeStop()
    {
        _cachedBackground?.Dispose();
        _bitmaps?.Dispose();
    }

    public override void Render(Graphics g)
    {
        if (_config.Colors.BackgroundOpacity != 0)
            _cachedBackground?.Draw(g);

        if (_bitmaps == null) return;

        var physics = SimDataProvider.LocalPlane.Physics;

        double indicatedAirSpeed = physics.IndicatedAirSpeed;
        if (IsPreviewing) indicatedAirSpeed = 199;
        double rawSpeed = _config.General.Units switch
        {
            IndicatedAirSpeedConfiguration.UnitChoice.Knots => indicatedAirSpeed,
            IndicatedAirSpeedConfiguration.UnitChoice.KilometersPerHour => indicatedAirSpeed * 1.852,
            IndicatedAirSpeedConfiguration.UnitChoice.MilesPerHour => indicatedAirSpeed * 1.1507794480235425,
            IndicatedAirSpeedConfiguration.UnitChoice.MetersPerSecond => indicatedAirSpeed * 0.5144444444444444,
            IndicatedAirSpeedConfiguration.UnitChoice.FeetPerSecond => indicatedAirSpeed * 1.687809911111111,
            _ => indicatedAirSpeed
        };

        int value = (int)Math.Round(rawSpeed);
        if (value < 0) value = 0; // Speed is non-negative

        int digits = _config.General.Digits;
        int x = 0;
        bool leading = true;

        // Get starting power of 10
        int power = digits <= _powersOfTen.Length
            ? _powersOfTen[digits - 1]
            : (int)Math.Pow(10, digits - 1);

        for (int i = 0; i < digits; i++)
        {
            byte digit = (byte)(value / power);

            if (leading && i != digits - 1)
            {
                if (digit == 0)
                {
                    // Skip drawing leading zeros (matches original behavior)
                    power /= 10;
                    x += _bitmaps.BitmapDimension.Width + _config.General.ExtraDigitSpacing;
                    continue;
                }
                leading = false;
            }

            _bitmaps.GetForNumber(digit).Draw(g, new Point(x, 0));

            value %= power;
            power /= 10;
            x += _bitmaps.BitmapDimension.Width + _config.General.ExtraDigitSpacing;
        }
    }

    private sealed class NumberBitmaps : IDisposable
    {
        private readonly CachedBitmap[] _rpmBitmaps = new CachedBitmap[10];
        public readonly (int Width, int Height) BitmapDimension;

        public NumberBitmaps(IndicatedAirSpeedConfiguration config)
        {
            GenerateBitMaps(config);
            BitmapDimension = (_rpmBitmaps[0].Width, _rpmBitmaps[0].Height);
        }

        private void GenerateBitMaps(IndicatedAirSpeedConfiguration config)
        {
            Font font = config.General.Font switch
            {
                IndicatedAirSpeedConfiguration.TextFont.Conthrax => FontUtil.FontConthrax(config.General.FontSize),
                IndicatedAirSpeedConfiguration.TextFont.Obitron => FontUtil.FontOrbitron(config.General.FontSize),
                IndicatedAirSpeedConfiguration.TextFont.Roboto => FontUtil.FontRoboto(config.General.FontSize),
                IndicatedAirSpeedConfiguration.TextFont.Segoe => FontUtil.FontSegoeMono(config.General.FontSize),
                _ => FontUtil.FontConthrax(config.General.FontSize),
            };

            using StringFormat format = StringFormat.GenericDefault;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            int bitmapWidth = (int)(config.General.FontSize + 4);
            int bitmapHeight = bitmapWidth + 4;

            if (bitmapHeight < FontUtil.MeasureHeight(font, "0123456789"))
                bitmapHeight = (int)Math.Round(FontUtil.MeasureHeight(font, "0123456789"));

            for (int i = 0; i <= 9; i++)
                _rpmBitmaps[i] = new(bitmapWidth, bitmapHeight, g =>
                {
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    using SolidBrush textBrush = new(Color.FromArgb(config.Colors.TextOpacity, config.Colors.TextColor));
                    g.DrawStringWithShadow($"{i}", font, textBrush, new RectangleF(0, 2, bitmapWidth, bitmapHeight - 2), format);
                });
        }

        public CachedBitmap GetForNumber(byte number)
        {
#if DEBUG
            ArgumentOutOfRangeException.ThrowIfGreaterThan(number, 9);
#endif
            return _rpmBitmaps[number];
        }

        public void Dispose()
        {
            foreach (CachedBitmap cachedBitmap in _rpmBitmaps)
                cachedBitmap?.Dispose();
        }
    }
}