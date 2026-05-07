using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace RaceElement.HUD.Common.Overlays.Flight.Altitude;

[Overlay(
    Name = "Altitudede",
    Description = "The Air Planes Altitude.",
    SupportedGames = Game.MicrosoftFlightSimulator2020,
    Authors = ["Reinier Klarenberg"]
)]
internal sealed class AltitudeOverlay(Rectangle rectangle) : CommonAbstractOverlay(rectangle, "Altitude")
{
    private readonly AltitudeConfiguration _config = new();

    private CachedBitmap? _cachedBackground;
    private NumberBitmaps? _bitmaps;

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
        if (_config.Colors.BackgroundOpacity != 0) _cachedBackground?.Draw(g);

        if (_bitmaps == null) return;

        int x = 0;

        var physics = SimDataProvider.LocalPlane.Physics;
        double altitude = _config.General.Units switch
        {
            AltitudeConfiguration.UnitChoice.Feet => physics.AltitudeSea,
            AltitudeConfiguration.UnitChoice.Meters => physics.AltitudeSea * 0.3048,
            AltitudeConfiguration.UnitChoice.Miles => physics.AltitudeSea / 5280.0,
            AltitudeConfiguration.UnitChoice.Kilometer => physics.AltitudeSea * 0.0003048,
            _ => physics.VerticalSpeed
        };

        string s = $"{altitude:f0}".FillStart(_config.General.Digits, ' ');

        for (int i = 0; i < _config.General.Digits; i++)
        {
            if (byte.TryParse(s.AsSpan(i, 1), out byte number))
                if (i != 0 || number != 0) // do not draw the first "0"
                    _bitmaps.GetForNumber(number).Draw(g, new(x, 0));

            x += _bitmaps.BitmapDimension.Width + _config.General.ExtraDigitSpacing;
        }
    }

    private sealed class NumberBitmaps : IDisposable
    {
        private readonly CachedBitmap[] _rpmBitmaps = new CachedBitmap[10];
        public readonly (int Width, int Height) BitmapDimension;
        public NumberBitmaps(AltitudeConfiguration config)
        {
            GenerateBitMaps(config);
            BitmapDimension = (_rpmBitmaps[0].Width, _rpmBitmaps[0].Height);
        }

        private void GenerateBitMaps(AltitudeConfiguration config)
        {
            Font font = config.General.Font switch
            {
                AltitudeConfiguration.TextFont.Conthrax => FontUtil.FontConthrax(config.General.FontSize),
                AltitudeConfiguration.TextFont.Obitron => FontUtil.FontOrbitron(config.General.FontSize),
                AltitudeConfiguration.TextFont.Roboto => FontUtil.FontRoboto(config.General.FontSize),
                AltitudeConfiguration.TextFont.Segoe => FontUtil.FontSegoeMono(config.General.FontSize),
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
