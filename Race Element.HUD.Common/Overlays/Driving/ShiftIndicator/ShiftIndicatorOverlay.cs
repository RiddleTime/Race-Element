using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Unglide;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftIndicator;

[Overlay(Name = "Shift Indicator",
    Description = "Shift Bar with RPM Text. Adjustable colors and percentages. (BETA)",
    Version = 1.00,
    OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Driving,
Authors = ["Reinier Klarenberg, Dirk Wolf"])]
internal sealed class ShiftIndicatorOverlay : CommonAbstractOverlay
{
    private readonly ShiftIndicatorConfiguration _config = new();

    private ShiftModel _model = new();
    private struct ShiftModel
    {
        public int CurrentRpm;
        public int MaxRpm;
    }

    private int _lastMaxRpm = -1;
    private CachedBitmap _cachedBackground;
    private CachedBitmap _cachedRpmLines;

    private Tweener _pitLimiterTweener;
    private DateTime _pitLimiterStart = DateTime.Now;
    private CachedBitmap _cachedPitLimiterOutline;

    private Font _font;
    private float _halfRpmStringWidth = -1;

    private List<(float, Color)> _colors;
    private List<CachedBitmap> _cachedColorBars;
    private CachedBitmap _cachedFlashBar;

    private Tweener _upShiftTweener;
    private Tween _upShiftTween;
    private DateTime _tweenStart;

    /// <summary>
    /// Used to display the early and upshift RPM in text.
    /// </summary>
    private bool _drawShiftRPM = false;

    public ShiftIndicatorOverlay(Rectangle rectangle) : base(rectangle, "Shift Indicator")
    {
        RefreshRateHz = _config.Bar.RefreshRate;
        Height = _config.Bar.Height + 1;
        Width = _config.Bar.Width + 1;
    }

    public sealed override void SetupPreviewData()
    {
        int maxRpm = SimDataProvider.LocalCar.Engine.MaxRpm;
        if (maxRpm <= 0) maxRpm = 9250; // porsche 911 max rpm..

        _model.MaxRpm = maxRpm;
        _model.CurrentRpm = (int)(maxRpm * 0.9f);
        _drawShiftRPM = true;
    }

    public sealed override void BeforeStart()
    {
        if (_config.Bar.ShowRpmText || _config.Bar.ShowPitLimiter)
        {
            float height = _config.Bar.Height - 14;
            height.Clip(11, 22);
            _font = FontUtil.FontSegoeMono(height);
        }
        int cornerRadius = (int)(10 * Scale);

        _colors =
            [
                (0.7f, Color.FromArgb(_config.Colors.NormalOpacity, _config.Colors.NormalColor)),
                (_config.Upshift.Early / 100f, Color.FromArgb(_config.Colors.EarlyOpacity, _config.Colors.EarlyColor)),
                (_config.Upshift.Upshift / 100f, Color.FromArgb(_config.Colors.UpshiftOpacity, _config.Colors.UpshiftColor))
            ];


        _cachedColorBars = [];
        foreach (var color in _colors.Select(x => x.Item2))
        {
            _cachedColorBars.Add(new CachedBitmap((int)(_config.Bar.Width * Scale + 1), (int)(_config.Bar.Height * Scale + 1), g =>
            {
                Rectangle rect = new(0, 1, (int)(_config.Bar.Width * Scale), (int)(_config.Bar.Height * Scale));
                using HatchBrush hatchBrush = new(HatchStyle.LightUpwardDiagonal, color, Color.FromArgb(color.A - 40, color));
                g.FillRoundedRectangle(hatchBrush, rect, cornerRadius);
            }));
        }

        _cachedFlashBar = new CachedBitmap((int)(_config.Bar.Width * Scale + 1), (int)(_config.Bar.Height * Scale + 1), g =>
        {
            Color color = Color.FromArgb(_config.Colors.FlashOpacity, _config.Colors.FlashColor);
            Rectangle rect = new(0, 1, (int)(_config.Bar.Width * Scale), (int)(_config.Bar.Height * Scale));
            using HatchBrush hatchBrush = new(HatchStyle.LightUpwardDiagonal, color, Color.FromArgb(color.A - 40, color));
            g.FillRoundedRectangle(hatchBrush, rect, cornerRadius);
        }, opacity: 0f);


        _cachedBackground = new CachedBitmap((int)(_config.Bar.Width * Scale + 1), (int)(_config.Bar.Height * Scale + 1), g =>
        {
            int midHeight = (int)(_config.Bar.Height * Scale) / 2;
            using LinearGradientBrush linerBrush = new(new Point(0, midHeight), new Point((int)(_config.Bar.Width * Scale), midHeight), Color.FromArgb(160, 0, 0, 0), Color.FromArgb(230, 0, 0, 0));
            g.FillRoundedRectangle(linerBrush, new Rectangle(0, 0, (int)(_config.Bar.Width * Scale), (int)(_config.Bar.Height * Scale)), cornerRadius);
            using Pen pen = new(Color.Black, 1 * Scale);
            g.DrawRoundedRectangle(pen, new Rectangle(0, 0, (int)(_config.Bar.Width * Scale), (int)(_config.Bar.Height * Scale)), cornerRadius);
        });

        _cachedRpmLines = new CachedBitmap((int)(_config.Bar.Width * Scale + 1), (int)(_config.Bar.Height * Scale + 1), g =>
        {
            int lineCount = (int)Math.Floor((_model.MaxRpm - _config.Bar.HideRpm) / 1000d);

            int leftOver = (_model.MaxRpm - _config.Bar.HideRpm) % 1000;
            if (leftOver < 70)
                lineCount--;

            lineCount.ClipMin(0);
            using SolidBrush brush = new(Color.FromArgb(220, Color.Black));
            using Pen linePen = new(brush, 1.5f * Scale);

            double thousandPercent = 1000d / (_model.MaxRpm - _config.Bar.HideRpm) * lineCount;
            double baseX = _config.Bar.Width * Scale / lineCount * thousandPercent;
            for (int i = 1; i <= lineCount; i++)
            {
                int x = (int)(i * baseX);
                g.DrawLine(linePen, x, 1, x, _config.Bar.Height * Scale - 1);
            }
        });

        if (_config.Bar.ShowPitLimiter)
        {
            _pitLimiterTweener = new Tweener();
            _cachedPitLimiterOutline = new CachedBitmap((int)(Width * Scale), (int)(Height * Scale), g =>
            {
                using Pen pen = new(Color.FromArgb(190, Color.Yellow), 5 * Scale);
                g.DrawRoundedRectangle(pen, new Rectangle(0, 0, (int)(Width * Scale - 1), (int)(Height * Scale - 1)), cornerRadius);
            });
        }

        _upShiftTweener = new Tweener();
        _upShiftTween = new Tween();

        _tweenStart = DateTime.Now;
    }

    public sealed override void BeforeStop()
    {
        _cachedBackground?.Dispose();
        _cachedRpmLines?.Dispose();
        _cachedPitLimiterOutline?.Dispose();
        _cachedFlashBar?.Dispose();

        if (_cachedColorBars != null)
            foreach (CachedBitmap cachedBitmap in _cachedColorBars)
                cachedBitmap?.Dispose();
    }

    public sealed override void Render(Graphics g)
    {
        _cachedBackground?.Draw(g, _config.Bar.Width, _config.Bar.Height);

        if (!IsPreviewing)
        {
            _model.CurrentRpm = SimDataProvider.LocalCar.Engine.Rpm;
            _model.MaxRpm = SimDataProvider.LocalCar.Engine.MaxRpm;
        }

        if (_config.Bar.ShowPitLimiter && SimDataProvider.LocalCar.Engine.IsPitLimiterOn)
            DrawPitLimiterBar(g);

        DrawRpmBar(g);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.TextContrast = 1;

        if (_config.Bar.ShowRpmText)
            DrawRpmText(g);

        // draw calculated early and upshift rpm, this gets activated by the SetupPreviewData() override. (Used in GUI only).
        if (_drawShiftRPM)
        {
            int x = (int)(_halfRpmStringWidth + 8 + _halfRpmStringWidth * 2 + 5);
            int y = (int)(_config.Bar.Height / 2 - _font.Height / 2.05);
            string earlyShiftRpm = $"Early:{_config.Upshift.Early / 100f * _model.MaxRpm:F0}";
            float earlyWidth = g.MeasureString(earlyShiftRpm, _font).Width;
            Rectangle earlyRect = new((int)(x - earlyWidth / 5), y, (int)earlyWidth, _font.Height);
            DrawTextWithOutline(g, Color.White, earlyShiftRpm, x, y, earlyRect);

            x += (int)(earlyWidth + 5);
            string upshiftRpm = $"Up:{_config.Upshift.Upshift / 100f * _model.MaxRpm:F0}";
            float upshiftWidth = g.MeasureString(upshiftRpm, _font).Width;
            Rectangle upshiftRect = new((int)(x - upshiftWidth / 3.5), y, (int)upshiftWidth, _font.Height);
            DrawTextWithOutline(g, Color.White, upshiftRpm, x, y, upshiftRect);
        }
    }

    private void DrawPitLimiterBar(Graphics g)
    {
        if (_cachedPitLimiterOutline.Opacity == 1f)
        {
            if (DateTime.Now.Subtract(_pitLimiterStart).TotalSeconds > 0.71)
            {
                _pitLimiterStart = DateTime.Now;
                _pitLimiterTweener.Tween(_cachedPitLimiterOutline, new { Opacity = 0.25f }, 0.2f);
            }
        }
        else
        {
            if (DateTime.Now.Subtract(_pitLimiterStart).TotalSeconds > 0.21)
            {
                _pitLimiterStart = DateTime.Now;
                _pitLimiterTweener.Tween(_cachedPitLimiterOutline, new { Opacity = 1f }, 0.5f);
            }
        }
        _pitLimiterTweener.Update((float)DateTime.Now.Subtract(_pitLimiterStart).TotalSeconds);

        _cachedPitLimiterOutline?.Draw(g, 0, 0, (int)(Width / Scale), (int)(Height / Scale));
    }

    private void DrawRpmText(Graphics g)
    {
        if (_halfRpmStringWidth < 0) _halfRpmStringWidth = g.MeasureString($"{_model.MaxRpm}", _font).Width / 2;

        int x = (int)(_halfRpmStringWidth + 8);
        int y = (int)(_config.Bar.Height / 2 - _font.Height / 2.05);
        Rectangle backgroundDimension = new((int)(x - _halfRpmStringWidth), y, (int)(_halfRpmStringWidth * 2), _font.Height);
        string currentRpm = $"{_model.CurrentRpm}".FillStart($"{_model.MaxRpm}".Length, ' ');
        DrawTextWithOutline(g, Color.White, currentRpm, x, y, backgroundDimension);
    }

    private void DrawTextWithOutline(Graphics g, Color textColor, string text, int x, int y, Rectangle rect)
    {
        using SolidBrush brush = new(Color.FromArgb(185, 0, 0, 0));
        g.FillRoundedRectangle(brush, rect, 2);
        g.DrawStringWithShadow(text, _font, textColor, new PointF(x - _halfRpmStringWidth, y + _font.Height / 14f), 1.5f * Scale);
    }

    private void DrawRpmBar(Graphics g)
    {
        double percent = 0;

        if (_model.MaxRpm > 0 && _model.CurrentRpm > 0)
            percent = (double)_model.CurrentRpm / _model.MaxRpm;

        if (percent > 0)
        {
            int index = 0;
            foreach ((float, Color) colorRange in _colors)
                if (percent > colorRange.Item1)
                    index = _colors.IndexOf(colorRange);

            // tween the opacity of the color bar when upshift time!
            if (percent > _colors[_colors.Count - 1].Item1)
            {
                if (_upShiftTween.Paused || _upShiftTween.TimeRemaining == 0)
                {
                    _cachedColorBars[_colors.Count - 1].Opacity = 1f;
                    _cachedFlashBar.Opacity = 0;
                    float duration = 1f / _config.Bar.FlashFrequency;
                    _upShiftTween = _upShiftTweener.Tween(_cachedColorBars[_colors.Count - 1], new { Opacity = 0.1f }, duration).Ease(Ease.SineOut);
                    _upShiftTweener.Tween(_cachedFlashBar, new { Opacity = 1f }, duration).Ease(Ease.SineIn);
                    _tweenStart = DateTime.Now;
                }
                else
                {
                    _upShiftTweener.Update((float)DateTime.Now.Subtract(_tweenStart).TotalSeconds);
                }

                _cachedFlashBar.Draw(g, 1, 0, _config.Bar.Width - 1, _config.Bar.Height - 1);
            }

            double adjustedPercent = ((double)_model.CurrentRpm - _config.Bar.HideRpm) / (_model.MaxRpm - _config.Bar.HideRpm);
            var barDrawWidth = (int)(_config.Bar.Width * adjustedPercent);

            g.SetClip(new Rectangle(0, 0, barDrawWidth, _config.Bar.Height));
            _cachedColorBars[index].Draw(g, 1, 0, _config.Bar.Width - 1, _config.Bar.Height - 1);

            g.SetClip(new Rectangle(0, 0, _config.Bar.Width, _config.Bar.Height));
        }

        DrawRpmBar1kLines(g);
    }

    private void DrawRpmBar1kLines(Graphics g)
    {
        if (_lastMaxRpm != _model.MaxRpm)
        {
            _lastMaxRpm = _model.MaxRpm;
            _halfRpmStringWidth = -1; // reseting this so the background for the text is correct
            _cachedRpmLines.Render();
        }

        _cachedRpmLines.Draw(g, _config.Bar.Width, _config.Bar.Height);
    }
}
