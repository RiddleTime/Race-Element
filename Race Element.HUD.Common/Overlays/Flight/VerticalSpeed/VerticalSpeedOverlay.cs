using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData.LocalPlane;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
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

        int x = 0;

        PhysicsData physics = SimDataProvider.LocalPlane.Physics;
        double verticalSpeed = _config.General.Units switch
        {
            VerticalSpeedConfiguration.UnitChoice.FeetPerMinute => physics.VerticalSpeed,
            VerticalSpeedConfiguration.UnitChoice.MetersPerMinute => physics.VerticalSpeed * 0.3048,
            VerticalSpeedConfiguration.UnitChoice.FeetPerSecond => physics.VerticalSpeed / 60.0,
            VerticalSpeedConfiguration.UnitChoice.MetersPerSecond => physics.VerticalSpeed * 0.00508,
            VerticalSpeedConfiguration.UnitChoice.MilesPerHour => physics.VerticalSpeed * (60.0 / 5280.0),
            VerticalSpeedConfiguration.UnitChoice.KilometersPerHour => physics.VerticalSpeed * 0.018288,
            _ => physics.VerticalSpeed
        };

        if (verticalSpeed >= 0)
        {
            if (_config.Colors.AirOpacity != 0) _cachedBackgroundAir?.Draw(g);
        }
        else
        {
            if (_config.Colors.EarthOpacity != 0) _cachedBackgroundEarth?.Draw(g);
        }


        string s = $"{verticalSpeed:f0}";
        if (verticalSpeed < 0) s = s.Replace("-", "");
        s = s.FillStart(_config.General.Digits, ' ');

        for (int i = 0; i < _config.General.Digits; i++)
        {
            if (byte.TryParse(s.AsSpan(i, 1), out byte number))
            {
                if (i == 0 && number == 0) // do not draw the first "0"
                    goto increaseX;

                _bitmaps.GetForNumber(number).Draw(g, new(x, 0));
            }
        increaseX:
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

        public CachedBitmap GetForNumber(byte number) => _rpmBitmaps.AsSpan()[number];

        public void Dispose()
        {
            foreach (CachedBitmap cachedBitmap in _rpmBitmaps)
                cachedBitmap?.Dispose();
        }
    }

}
