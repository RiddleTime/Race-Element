using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.InfoPanel;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayClock
{
    [Overlay(Name = "Clock",
        Description = "Displays the system time of your operation system.",
        OverlayType = OverlayType.Pitwall,
        OverlayCategory = OverlayCategory.All)]
    internal sealed class ClockOverlay : AbstractOverlay
    {
        private readonly SystemTimeConfig _config = new SystemTimeConfig();
        private sealed class SystemTimeConfig : OverlayConfiguration
        {
            public enum TimeFormat
            {
                Full,
                AmPm
            }

            [ConfigGrouping("Clock", "Change settings that alter the presentation of the time.")]
            public InfoPanelGrouping InfoPanel { get; set; } = new InfoPanelGrouping();
            public class InfoPanelGrouping
            {
                [ToolTip("Change between Full(24h) and AM/PM notation of time.")]
                public TimeFormat Format { get; set; } = TimeFormat.Full;

                [ToolTip("Renders this HUD regardless of the whether the engine is running or not.")]
                public bool AlwaysShow { get; set; } = true;
            }

            public SystemTimeConfig() => this.AllowRescale = true;
        }

        private Font _font;

        private PanelText _timeHeader;
        private PanelText _timeValue;

        public ClockOverlay(Rectangle rectangle) : base(rectangle, "Clock")
        {
            RefreshRateHz = 1;
        }

        public override void BeforeStart()
        {
            _font = FontUtil.FontSegoeMono(10f * this.Scale);

            int lineHeight = _font.Height;

            int unscaledHeaderWidth = 46;
            int unscaledValueWidth = 80;
            if (_config.InfoPanel.Format == SystemTimeConfig.TimeFormat.AmPm)
                unscaledValueWidth += 24;

            int headerWidth = (int)(unscaledHeaderWidth * this.Scale);
            int valueWidth = (int)(unscaledValueWidth * this.Scale);
            int roundingRadius = (int)(6 * this.Scale);

            RectangleF headerRect = new RectangleF(0, 0, headerWidth, lineHeight);
            RectangleF valueRect = new RectangleF(headerWidth, 0, valueWidth, lineHeight);
            StringFormat headerFormat = new StringFormat() { Alignment = StringAlignment.Near };
            StringFormat valueFormat = new StringFormat() { Alignment = StringAlignment.Center, };
            Color accentColor = Color.FromArgb(25, 255, 0, 0);
            CachedBitmap headerBackground = new CachedBitmap(headerWidth, lineHeight, g =>
            {
                Rectangle panelRect = new Rectangle(0, 0, headerWidth, lineHeight);
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, 0, 0, roundingRadius);
                using LinearGradientBrush brush = new LinearGradientBrush(panelRect, Color.FromArgb(185, 0, 0, 0), Color.FromArgb(255, 10, 10, 10), LinearGradientMode.BackwardDiagonal);
                g.FillPath(brush, path);
                g.DrawLine(new Pen(accentColor), 0 + roundingRadius / 2, lineHeight, headerWidth, lineHeight - 1);
            });
            CachedBitmap valueBackground = new CachedBitmap(valueWidth, lineHeight, g =>
            {
                Rectangle panelRect = new Rectangle(0, 0, valueWidth, lineHeight);
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, roundingRadius, 0, 0);
                using LinearGradientBrush brush = new LinearGradientBrush(panelRect, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(185, 0, 0, 0), LinearGradientMode.ForwardDiagonal);
                g.FillPath(brush, path);
                g.DrawLine(new Pen(accentColor), 0, lineHeight - 1, valueWidth, lineHeight - 1);
            });

            _timeHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _timeValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);

            this.Width = unscaledHeaderWidth + unscaledValueWidth;
            this.Height = (int)(headerRect.Top / this.Scale);
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
            _timeHeader.Draw(g, "Clock", this.Scale);
            string format = string.Empty;
            switch (_config.InfoPanel.Format)
            {
                case SystemTimeConfig.TimeFormat.Full:
                    {
                        format = "HH\\:mm\\:ss";
                        break;
                    }
                case SystemTimeConfig.TimeFormat.AmPm:
                    {
                        format = "hh\\:mm\\:ss tt";
                        break;
                    }
            }
            _timeValue.Draw(g, $"{DateTime.Now.ToString(format)}", this.Scale);

        }
    }
}
