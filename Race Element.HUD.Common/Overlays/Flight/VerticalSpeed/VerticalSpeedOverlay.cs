using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData.LocalPlane;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Text;

namespace RaceElement.HUD.Common.Overlays.Flight.VerticalSpeed;

[Overlay(
    Name = "Vertical Speed",
    Description = "The Air Planes Vertical speed.",
    SupportedGames = Game.MicrosoftFlightSimulator2020 | Game.MicrosoftFlightSimulator2024,
    Authors = ["Reinier Klarenberg"]
)]
internal sealed class VerticalSpeedOverlay(Rectangle rectangle) : CommonAbstractOverlay(rectangle, "Vertical Speed")
{
    private readonly VerticalSpeedConfiguration _config = new();

    private CachedBitmap? _cachedBackgroundAir;
    private CachedBitmap? _cachedBackgroundEarth;
    private NumberBitmaps? _bitmaps;

    // Precomputed powers of 10 for fast digit extraction (covers up to 7 digits)
    private static readonly int[] _powersOfTen = { 1, 10, 100, 1000, 10000, 100000, 1000000 };

    public override void BeforeStart()
    {
        RefreshRateHz = _config.General.RefreshRate;

        _bitmaps = new NumberBitmaps(_config);
        Width = _config.General.Digits * _bitmaps.BitmapDimension.Width + _config.General.ExtraDigitSpacing * (_config.General.Digits - 1);
        Height = _bitmaps.BitmapDimension.Height;

        _cachedBackgroundAir = CreateBackgroundBitmap(_config.Colors.AirOpacity, _config.Colors.AirColor);
        _cachedBackgroundEarth = CreateBackgroundBitmap(_config.Colors.EarthOpacity, _config.Colors.EarthColor);
    }

    private CachedBitmap CreateBackgroundBitmap(int opacity, Color color) => new(Width, Height, g =>
    {
        RectangleF barArea = new(0, 0, Width - 1, Height - 1);

        using SolidBrush darkBrush = new(Color.FromArgb(opacity, color));
        g.FillRoundedRectangle(darkBrush, Rectangle.Round(barArea), 3);

        using Pen darkPen = new(darkBrush, 1);
        g.DrawRoundedRectangle(darkPen, Rectangle.Round(barArea), 3);
    });

    public override void BeforeStop()
    {
        _cachedBackgroundAir?.Dispose();
        _cachedBackgroundEarth?.Dispose();
        _bitmaps?.Dispose();
    }

    public override void Render(Graphics g)
    {
        if (_bitmaps == null) return;

        var physics = SimDataProvider.LocalPlane.Physics;


        double vertSpeed = physics.VerticalSpeed;
        if (IsPreviewing) vertSpeed = 1723;

        double rawSpeed = _config.General.Units switch
        {
            VerticalSpeedConfiguration.UnitChoice.FeetPerMinute => vertSpeed,
            VerticalSpeedConfiguration.UnitChoice.MetersPerMinute => vertSpeed * 0.3048,
            VerticalSpeedConfiguration.UnitChoice.FeetPerSecond => vertSpeed / 60.0,
            VerticalSpeedConfiguration.UnitChoice.MetersPerSecond => vertSpeed * 0.00508,
            VerticalSpeedConfiguration.UnitChoice.MilesPerHour => vertSpeed * (60.0 / 5280.0),
            VerticalSpeedConfiguration.UnitChoice.KilometersPerHour => vertSpeed * 0.018288,
            _ => vertSpeed
        };

        bool isPositive = rawSpeed >= 0;
        int value = (int)Math.Round(Math.Abs(rawSpeed));


        // Background
        if (isPositive)
        {
            if (_config.Colors.AirOpacity != 0) _cachedBackgroundAir?.Draw(g);
        }
        else
        {
            if (_config.Colors.EarthOpacity != 0) _cachedBackgroundEarth?.Draw(g);
        }

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

            if (leading)
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

        public NumberBitmaps(VerticalSpeedConfiguration config)
        {
            GenerateBitMaps(config);
            BitmapDimension = (_rpmBitmaps[0].Width, _rpmBitmaps[0].Height);
        }

        private void GenerateBitMaps(VerticalSpeedConfiguration config)
        {
            Font font = config.General.Font switch
            {
                VerticalSpeedConfiguration.TextFont.Conthrax => FontUtil.FontConthrax(config.General.FontSize),
                VerticalSpeedConfiguration.TextFont.Obitron => FontUtil.FontOrbitron(config.General.FontSize),
                VerticalSpeedConfiguration.TextFont.Roboto => FontUtil.FontRoboto(config.General.FontSize),
                VerticalSpeedConfiguration.TextFont.Segoe => FontUtil.FontSegoeMono(config.General.FontSize),
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