using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace RaceElement.HUD.Common.Overlays.Driving.Speedometer;

[Overlay(
    Name = "Speedometer",
    Description = "The current speed as text",
    UnsupportedGames = Game.MicrosoftFlightSimulator2020 | Game.MicrosoftFlightSimulator2024,
    Authors = ["Reinier Klarenberg"]
)]
internal sealed class SpeedometerOverlay(Rectangle rectangle) : CommonAbstractOverlay(rectangle, "Speedometer")
{
    private readonly SpeedometerConfiguration _config = new();

    private CachedBitmap? _cachedBackground;
    private RpmBitmaps? _bitmaps;

    public override void BeforeStart()
    {
        RefreshRateHz = _config.General.RefreshRate;

        _bitmaps = new RpmBitmaps(_config);
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
        if (_config.Colors.BackgroundOpacity != 0) _cachedBackground?.Draw(g);

        if (_bitmaps == null) return;

        int x = 0;

        float speedKmh = SimDataProvider.LocalCar.Physics.Velocity;
        if (_config.General.Units == SpeedometerConfiguration.UnitChoice.Mph)
            speedKmh *= 0.621371f;

        string s = $"{speedKmh:f0}".FillStart(_config.General.Digits, ' ');

        for (int i = 0; i < _config.General.Digits; i++)
        {
            if (byte.TryParse(s.AsSpan(i, 1), out byte number))
                if (i != 0 || number != 0) // do not draw the first "0"
                    _bitmaps.GetForNumber(number).Draw(g, new(x, 0));

            x += _bitmaps.BitmapDimension.Width + _config.General.ExtraDigitSpacing;
        }
    }

    private sealed class RpmBitmaps : IDisposable
    {
        private readonly CachedBitmap[] _rpmBitmaps = new CachedBitmap[10];
        public readonly (int Width, int Height) BitmapDimension;
        public RpmBitmaps(SpeedometerConfiguration config)
        {
            GenerateBitMaps(config);
            BitmapDimension = (_rpmBitmaps[0].Width, _rpmBitmaps[0].Height);
        }

        private void GenerateBitMaps(SpeedometerConfiguration config)
        {
            Font font = config.General.Font switch
            {
                SpeedometerConfiguration.RpmTextFont.Conthrax => FontUtil.FontConthrax(config.General.FontSize),
                SpeedometerConfiguration.RpmTextFont.Obitron => FontUtil.FontOrbitron(config.General.FontSize),
                SpeedometerConfiguration.RpmTextFont.Roboto => FontUtil.FontRoboto(config.General.FontSize),
                SpeedometerConfiguration.RpmTextFont.Segoe => FontUtil.FontSegoeMono(config.General.FontSize),
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
            ArgumentOutOfRangeException.ThrowIfGreaterThan(number, 9);
            return _rpmBitmaps.AsSpan()[number];
        }

        public void Dispose()
        {
            foreach (CachedBitmap cachedBitmap in _rpmBitmaps)
                cachedBitmap?.Dispose();
        }

    }

}
