using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;

namespace RaceElement.HUD.Common.Overlays.OverlayLapDeltaBar;

[Overlay(Name = "Lap Delta Bar (BETA)", Description = "A customizable Laptime Delta Bar (BETA)", OverlayType = OverlayType.Drive, Version = 1,
    OverlayCategory = OverlayCategory.Lap,
Authors = ["Reinier Klarenberg", "Dirk Wolf"])]
internal sealed class LapDeltaOverlay : CommonAbstractOverlay
{
    private readonly LapTimeDeltaConfiguration _config = new();

    private float _deltaStringWidth = -1;
    private readonly Font _font;

    private CachedBitmap _cachedBackground;
    private CachedBitmap _cachedPositiveDelta;
    private CachedBitmap _cachedNegativeDelta;

    public LapDeltaOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta Bar")
    {
        this.Width = _config.Bar.Width + 1;
        this.Height = _config.Bar.Height + 1;

        _font = FontUtil.FontSegoeMono(_config.Delta.FontSize);
        this.Height += _font.Height * 1;

        this.RefreshRateHz = 5;
    }

    public sealed override void SetupPreviewData()
    {
        SessionData.Instance.LapDeltaToSessionBestLapMs = -0137;
    }

    public sealed override void BeforeStart()
    {
        try
        {
            int cornerRadius = (int)(_config.Bar.Roundness * this.Scale);

            _cachedBackground = new CachedBitmap((int)(_config.Bar.Width * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
            {
                Color bgColor = Color.FromArgb(185, 0, 0, 0);
                HatchBrush hatchBrush = new(HatchStyle.LightUpwardDiagonal, bgColor, Color.FromArgb(bgColor.A - 50, bgColor));
                g.FillRoundedRectangle(hatchBrush, new Rectangle(0, 0, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), cornerRadius);
                g.DrawRoundedRectangle(new Pen(Color.Black, 1 * this.Scale), new Rectangle(0, 0, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), cornerRadius);
            });

            _cachedPositiveDelta = new CachedBitmap((int)(_config.Bar.Width / 2 * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
            {
                Rectangle rect = new(0, 0, (int)(_config.Bar.Width / 2 * this.Scale), (int)(_config.Bar.Height * this.Scale));
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(rect, cornerRadius, 0, 0, cornerRadius);
                g.FillPath(new SolidBrush(Color.FromArgb(_config.Colors.SlowerOpacity, _config.Colors.SlowerColor)), path);
            });

            _cachedNegativeDelta = new CachedBitmap((int)(_config.Bar.Width / 2 * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
            {
                Rectangle rect = new(0, 0, (int)(_config.Bar.Width / 2 * this.Scale), (int)(_config.Bar.Height * this.Scale));
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(rect, 0, cornerRadius, cornerRadius, 0);
                g.FillPath(new SolidBrush(Color.FromArgb(_config.Colors.FasterOpacity, _config.Colors.FasterColor)), path);
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public sealed override void BeforeStop()
    {
        _cachedBackground?.Dispose();
        _cachedPositiveDelta?.Dispose();
        _cachedNegativeDelta?.Dispose();

        _font?.Dispose();
    }

    public sealed override bool ShouldRender()
    {        
        if (_config.Delta.HideForRace && !this.IsRepositioning && SessionData.Instance.SessionType == RaceSessionType.Race)
            return false;

        /* TODO
        if (_config.Delta.Spectator && RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
            return true; */

        return base.ShouldRender();
    }

    public sealed override void Render(Graphics g)
    {
        _cachedBackground?.Draw(g, 0, 0, _config.Bar.Width, _config.Bar.Height);

        float delta = GetDelta();
        DrawDeltaBar(g, delta);
        DrawDeltaText(g, delta);
    }

    private float GetDelta()
    {
        float delta = (float)SessionData.Instance.LapDeltaToSessionBestLapMs;
        if (_config.Delta.Spectator)
        {
            int focusedIndex = SessionData.Instance.FocusedCarIndex;
            if (SimDataProvider.Instance.IsSpectating(SessionData.Instance.PlayerCarIndex, focusedIndex))
                lock (SessionData.Instance.Cars)
                {
                    if (SessionData.Instance.Cars.Any())
                    {
                        var car = SessionData.Instance.Cars.First(car => car.Key == focusedIndex);
                        delta = car.Value.LapDeltaToSessionBestLap;
                    }
                }
        }

        delta.Clip(-_config.Delta.MaxDelta, _config.Delta.MaxDelta);

        return delta;
    }

    private void DrawDeltaBar(Graphics g, float delta)
    {
        float halfBarWidth = _config.Bar.Width / 2f;

        if (delta > 0)
        {
            float fillPercent = delta / _config.Delta.MaxDelta;
            float drawWidth = halfBarWidth * fillPercent;
            drawWidth.ClipMin(1);

            g.SetClip(new Rectangle((int)(halfBarWidth - drawWidth), 0, (int)drawWidth, _config.Bar.Height));
            _cachedPositiveDelta?.Draw(g, 0, 0, (int)halfBarWidth, _config.Bar.Height);
            g.ResetClip();
        }
        else if (delta < 0)
        {
            float fillPercent = delta / -_config.Delta.MaxDelta;
            float drawWidth = halfBarWidth * fillPercent;
            drawWidth.ClipMin(1);

            g.SetClip(new Rectangle((int)(halfBarWidth), 0, (int)drawWidth, _config.Bar.Height));
            _cachedNegativeDelta?.Draw(g, (int)halfBarWidth, 0, (int)halfBarWidth, _config.Bar.Height);
            g.ResetClip();
        }
    }

    private void DrawDeltaText(Graphics g, float delta)
    {
        string currentDelta = $"{delta.ToString($"F{_config.Delta.Decimals}")}";
        if (delta >= 0) currentDelta = "+" + currentDelta;

        currentDelta.FillStart(_config.Delta.Decimals + 3, ' '); // (+3) = ('-' or '+') plus "0."

        if (_deltaStringWidth < 0)
            _deltaStringWidth = g.MeasureString(currentDelta, _font).Width;

        int x = _config.Bar.Width / 2;
        int y = _config.Bar.Height + 2;
        DrawTextWithOutline(g, !SessionData.Instance.Cars[SessionData.Instance.PlayerCarIndex].Value.CurrentLap.IsInvalid || 
            SimDataProvider.Instance.IsSpectating(SessionData.Instance.PlayerCarIndex, SessionData.Instance.FocusedCarIndex) ? Color.White : Color.Red, currentDelta, x, y);
    }

    private void DrawTextWithOutline(Graphics g, Color textColor, string text, int x, int y)
    {
        Rectangle backgroundDimension = new((int)(x - _deltaStringWidth / 2), y, (int)(_deltaStringWidth), (int)(_font.Height * 0.9));

        g.SmoothingMode = SmoothingMode.AntiAlias;
        using SolidBrush backgroundBrush = new(Color.FromArgb(185, 0, 0, 0));
        g.FillRoundedRectangle(backgroundBrush, backgroundDimension, (int)(2 * this.Scale));

        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.TextContrast = 1;

        g.DrawStringWithShadow(text, _font, textColor, new PointF(x - _deltaStringWidth / 2, y), 1.3f);
    }
}
