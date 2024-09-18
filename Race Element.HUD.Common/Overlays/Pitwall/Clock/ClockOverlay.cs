using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.InfoPanel;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.Common.Overlays.Pitwall.Clock;

[Overlay(
    Name = "Clock",
    Description = "Displays the system time of your operation system.",
    OverlayType = OverlayType.Pitwall,
    OverlayCategory = OverlayCategory.All,
    Authors = ["Reinier Klarenberg"]
)]
internal sealed class ClockOverlay(Rectangle rectangle) : CommonAbstractOverlay(rectangle, "Clock")
{
    private readonly SystemTimeConfig _config = new();
    private sealed class SystemTimeConfig : OverlayConfiguration
    {
        public SystemTimeConfig() => GenericConfiguration.AllowRescale = true;

        public enum TimeFormat
        {
            Full,
            AmPm
        }

        [ConfigGrouping("Clock", "Change settings that alter the presentation of the time.")]
        public InfoPanelGrouping InfoPanel { get; init; } = new();
        public sealed class InfoPanelGrouping
        {
            [ToolTip("Change between Full(24h) and AM/PM notation of time.")]
            public TimeFormat Format { get; init; } = TimeFormat.Full;

            [ToolTip("Renders this HUD regardless of the whether the engine is running or not.")]
            public bool AlwaysShow { get; init; } = true;
        }
    }

    private Font _font;

    private PanelText _timeHeader;
    private PanelText _timeValue;

    private string _timeFormat = FullTimeFormat;
    private const string FullTimeFormat = "HH\\:mm\\:ss";
    private const string AmpPmTimeFormat = "hh\\:mm\\:ss tt";

    public override void BeforeStart()
    {
        _timeFormat = _config.InfoPanel.Format switch
        {
            SystemTimeConfig.TimeFormat.Full => FullTimeFormat,
            SystemTimeConfig.TimeFormat.AmPm => AmpPmTimeFormat,
            _ => FullTimeFormat,
        };


        _font = FontUtil.FontSegoeMono(10f * Scale);

        int lineHeight = _font.Height;

        int unscaledHeaderWidth = 46;
        int unscaledValueWidth = 80;
        if (_config.InfoPanel.Format == SystemTimeConfig.TimeFormat.AmPm)
            unscaledValueWidth += 24;

        int headerWidth = (int)(unscaledHeaderWidth * Scale);
        int valueWidth = (int)(unscaledValueWidth * Scale);
        int roundingRadius = (int)(6 * Scale);

        RectangleF headerRect = new(0, 0, headerWidth, lineHeight);
        RectangleF valueRect = new(headerWidth, 0, valueWidth, lineHeight);
        StringFormat headerFormat = new() { Alignment = StringAlignment.Near };
        StringFormat valueFormat = new() { Alignment = StringAlignment.Center, };
        Color accentColor = Color.FromArgb(25, 255, 0, 0);
        CachedBitmap headerBackground = new(headerWidth, lineHeight, g =>
        {
            Rectangle panelRect = new(0, 0, headerWidth, lineHeight);
            using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, 0, 0, roundingRadius);
            using LinearGradientBrush brush = new(panelRect, Color.FromArgb(185, 0, 0, 0), Color.FromArgb(255, 10, 10, 10), LinearGradientMode.BackwardDiagonal);
            g.FillPath(brush, path);
            g.DrawLine(new Pen(accentColor), 0 + roundingRadius / 2, lineHeight, headerWidth, lineHeight - 1);
        });
        CachedBitmap valueBackground = new(valueWidth, lineHeight, g =>
        {
            Rectangle panelRect = new(0, 0, valueWidth, lineHeight);
            using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, roundingRadius, 0, 0);
            using LinearGradientBrush brush = new(panelRect, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(185, 0, 0, 0), LinearGradientMode.ForwardDiagonal);
            g.FillPath(brush, path);
            g.DrawLine(new Pen(accentColor), 0, lineHeight - 1, valueWidth, lineHeight - 1);
        });

        _timeHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
        _timeValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
        headerRect.Offset(0, lineHeight);
        valueRect.Offset(0, lineHeight);

        Width = unscaledHeaderWidth + unscaledValueWidth;
        Height = (int)(headerRect.Top / Scale);
        RefreshRateHz = 1;
    }

    public override void BeforeStop()
    {
        _font?.Dispose();
        _timeHeader?.Dispose();
        _timeValue?.Dispose();
    }

    public override bool ShouldRender()
    {
        if (_config.InfoPanel.AlwaysShow)
            return true;

        return base.ShouldRender();
    }

    public override void Render(Graphics g)
    {
        _timeHeader.Draw(g, "Clock", Scale);
        _timeValue.Draw(g, $"{DateTime.Now.ToString(_timeFormat)}", Scale);
    }
}
