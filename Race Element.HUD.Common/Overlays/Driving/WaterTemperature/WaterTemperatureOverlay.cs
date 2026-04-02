using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace RaceElement.HUD.Common.Overlays.Driving.WaterTemperature;

[Overlay(
    Name = "Water Temperature",
    Description = "Shows engine water (coolant) temperature with color-coded warnings",
    Authors = ["Aaron"],
    SupportedGames = Data.Games.Game.RichardBurnsRally
)]
internal sealed class WaterTemperatureOverlay(Rectangle rectangle) : CommonAbstractOverlay(rectangle, "Water Temperature")
{
    private readonly WaterTemperatureConfiguration _config = new();

    private CachedBitmap? _cachedBackground;
    private TemperatureBitmaps? _bitmaps;
    private Font? _unitFont;
    private Font? _labelFont;

    public override void BeforeStart()
    {
        RefreshRateHz = _config.General.RefreshRate;

        _bitmaps = new TemperatureBitmaps(_config);
        
        // Calculate width: digits + spacing + unit + label
        int digitsWidth = _config.General.Digits * _bitmaps.BitmapDimension.Width + 
                         _config.General.ExtraDigitSpacing * (_config.General.Digits - 1);
        
        int unitWidth = 0;
        if (_config.General.ShowUnit)
        {
            _unitFont = _config.General.Font switch
            {
                WaterTemperatureConfiguration.TemperatureTextFont.Conthrax => FontUtil.FontConthrax(_config.General.FontSize * 0.6f),
                WaterTemperatureConfiguration.TemperatureTextFont.Obitron => FontUtil.FontOrbitron(_config.General.FontSize * 0.6f),
                WaterTemperatureConfiguration.TemperatureTextFont.Roboto => FontUtil.FontRoboto(_config.General.FontSize * 0.6f),
                WaterTemperatureConfiguration.TemperatureTextFont.Segoe => FontUtil.FontSegoeMono(_config.General.FontSize * 0.6f),
                _ => FontUtil.FontRoboto(_config.General.FontSize * 0.6f),
            };
            string unitText = _config.General.Units == WaterTemperatureConfiguration.UnitChoice.Celsius ? "°C" : "°F";
            unitWidth = (int)FontUtil.MeasureWidth(_unitFont, unitText) + 5;
        }

        int labelWidth = 0;
        if (_config.General.ShowLabel)
        {
            _labelFont = _config.General.Font switch
            {
                WaterTemperatureConfiguration.TemperatureTextFont.Conthrax => FontUtil.FontConthrax(_config.General.FontSize * 0.8f),
                WaterTemperatureConfiguration.TemperatureTextFont.Obitron => FontUtil.FontOrbitron(_config.General.FontSize * 0.8f),
                WaterTemperatureConfiguration.TemperatureTextFont.Roboto => FontUtil.FontRoboto(_config.General.FontSize * 0.8f),
                WaterTemperatureConfiguration.TemperatureTextFont.Segoe => FontUtil.FontSegoeMono(_config.General.FontSize * 0.8f),
                _ => FontUtil.FontRoboto(_config.General.FontSize * 0.8f),
            };
            labelWidth = (int)FontUtil.MeasureWidth(_labelFont, _config.General.LabelText) + 10;
        }

        Width = digitsWidth + unitWidth + labelWidth + 10;
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
        _unitFont?.Dispose();
        _labelFont?.Dispose();
    }

    public override void Render(Graphics g)
    {
        if (_config.Colors.BackgroundOpacity != 0) _cachedBackground?.Draw(g);

        if (_bitmaps == null) return;

        int x = 5;

        // Draw label if enabled
        if (_config.General.ShowLabel && _labelFont != null)
        {
            using SolidBrush labelBrush = new(Color.FromArgb(_config.Colors.TextOpacity, _config.Colors.TextColor));
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.DrawString(_config.General.LabelText, _labelFont, labelBrush, x, Height / 2 - _labelFont.Height / 2);
            x += (int)FontUtil.MeasureWidth(_labelFont, _config.General.LabelText) + 10;
        }

        // Get water temperature
        float waterTempCelsius = SimDataProvider.LocalCar.Engine.WaterTemperature;
        float displayTemp = waterTempCelsius;
        
        if (_config.General.Units == WaterTemperatureConfiguration.UnitChoice.Fahrenheit)
            displayTemp = waterTempCelsius * 9f / 5f + 32f;

        // Determine color based on temperature
        Color tempColor = GetTemperatureColor(waterTempCelsius);

        // Format temperature
        string tempStr = $"{displayTemp:f0}".FillStart(_config.General.Digits, ' ');

        // Draw digits
        for (int i = 0; i < _config.General.Digits; i++)
        {
            if (byte.TryParse(tempStr.AsSpan(i, 1), out byte number))
            {
                if (i != 0 || number != 0) // do not draw leading zeros
                {
                    _bitmaps.GetForNumber(number, tempColor).Draw(g, new(x, 0));
                }
            }

            x += _bitmaps.BitmapDimension.Width + _config.General.ExtraDigitSpacing;
        }

        // Draw unit if enabled
        if (_config.General.ShowUnit && _unitFont != null)
        {
            using SolidBrush unitBrush = new(Color.FromArgb(_config.Colors.TextOpacity, tempColor));
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            string unitText = _config.General.Units == WaterTemperatureConfiguration.UnitChoice.Celsius ? "°C" : "°F";
            g.DrawString(unitText, _unitFont, unitBrush, x + 5, Height / 2 - _unitFont.Height / 2);
        }
    }

    private Color GetTemperatureColor(float tempCelsius)
    {
        if (tempCelsius >= _config.Thresholds.DangerTemperature)
            return _config.Colors.DangerColor;
        else if (tempCelsius >= _config.Thresholds.WarningTemperature)
            return _config.Colors.WarningColor;
        else
            return _config.Colors.NormalColor;
    }

    private sealed class TemperatureBitmaps : IDisposable
    {
        private readonly Dictionary<(byte, Color), CachedBitmap> _bitmapCache = new();
        public readonly (int Width, int Height) BitmapDimension;
        private readonly WaterTemperatureConfiguration _config;
        private readonly Font _font;

        public TemperatureBitmaps(WaterTemperatureConfiguration config)
        {
            _config = config;
            _font = config.General.Font switch
            {
                WaterTemperatureConfiguration.TemperatureTextFont.Conthrax => FontUtil.FontConthrax(config.General.FontSize),
                WaterTemperatureConfiguration.TemperatureTextFont.Obitron => FontUtil.FontOrbitron(config.General.FontSize),
                WaterTemperatureConfiguration.TemperatureTextFont.Roboto => FontUtil.FontRoboto(config.General.FontSize),
                WaterTemperatureConfiguration.TemperatureTextFont.Segoe => FontUtil.FontSegoeMono(config.General.FontSize),
                _ => FontUtil.FontConthrax(config.General.FontSize),
            };

            int bitmapWidth = (int)(config.General.FontSize + 4);
            int bitmapHeight = bitmapWidth + 4;

            if (bitmapHeight < FontUtil.MeasureHeight(_font, "0123456789"))
                bitmapHeight = (int)Math.Round(FontUtil.MeasureHeight(_font, "0123456789"));

            BitmapDimension = (bitmapWidth, bitmapHeight);
        }

        public CachedBitmap GetForNumber(byte number, Color color)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(number, 9);

            var key = (number, color);
            if (!_bitmapCache.TryGetValue(key, out CachedBitmap? bitmap))
            {
                bitmap = CreateBitmap(number, color);
                _bitmapCache[key] = bitmap;
            }

            return bitmap;
        }

        private CachedBitmap CreateBitmap(byte number, Color color)
        {
            using StringFormat format = StringFormat.GenericDefault;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            return new(BitmapDimension.Width, BitmapDimension.Height, g =>
            {
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                using SolidBrush textBrush = new(Color.FromArgb(_config.Colors.TextOpacity, color));
                g.DrawStringWithShadow($"{number}", _font, textBrush, 
                    new RectangleF(0, 2, BitmapDimension.Width, BitmapDimension.Height - 2), format);
            });
        }

        public void Dispose()
        {
            foreach (var bitmap in _bitmapCache.Values)
                bitmap?.Dispose();
            _bitmapCache.Clear();
            _font?.Dispose();
        }
    }
}
