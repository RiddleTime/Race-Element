using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.InfoPanel;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.OverlayTrackInfo;

[Overlay(Name = "Track Info", Version = 1.00, OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Track,
    Description = "A panel showing information about the track state: grip, temperatures and wind.\nOptionally showing the global flag, session type and time of day.",
Authors = ["Reinier Klarenberg"])]
internal sealed class TrackInfoOverlay : AbstractOverlay
{
    private readonly TrackInfoConfig _config = new();
    private sealed class TrackInfoConfig : OverlayConfiguration
    {
        [ConfigGrouping("Info Panel", "Show or hide additional information in the panel.")]
        public InfoPanelGrouping InfoPanel { get; init; } = new InfoPanelGrouping();
        public sealed class InfoPanelGrouping
        {
            [ToolTip("Displays the actual time on track.")]
            public bool TimeOfDay { get; init; } = true;

            [ToolTip("Shows the global track flag.")]
            public bool GlobalFlag { get; init; } = true;

            [ToolTip("Shows the type of the session.")]
            public bool SessionType { get; set; } = true;

            [ToolTip("Displays the track temperature")]
            public bool TrackTemperature { get; init; } = true;
        }

        public TrackInfoConfig()
        {
            this.GenericConfiguration.AllowRescale = true;
        }
    }

    private Font _font;

    private PanelText _timeHeader;
    private PanelText _timeValue;
    private PanelText _globalFlagHeader;
    private PanelText _globalFlagValue;
    private PanelText _sessionTypeLabel;
    private PanelText _sessionTypeValue;
    private PanelText _airTempLabel;
    private PanelText _airTempValue;
    private PanelText _trackTempLabel;
    private PanelText _trackTempValue;
    private PanelText _gripLabel;
    private PanelText _gripValue;
    private PanelText _windLabel;
    private PanelText _windValue;

    public TrackInfoOverlay(Rectangle rectangle) : base(rectangle, "Track Info")
    {
        RefreshRateHz = 1;
    }

    public sealed override void BeforeStart()
    {
        _font = FontUtil.FontSegoeMono(10f * this.Scale);

        int lineHeight = _font.Height;

        int unscaledHeaderWidth = 66;
        int unscaledValueWidth = 94;

        int headerWidth = (int)(unscaledHeaderWidth * this.Scale);
        int valueWidth = (int)(unscaledValueWidth * this.Scale);
        int roundingRadius = (int)(6 * this.Scale);

        RectangleF headerRect = new(0, 0, headerWidth, lineHeight);
        RectangleF valueRect = new(headerWidth, 0, valueWidth, lineHeight);
        StringFormat headerFormat = new() { Alignment = StringAlignment.Near };
        StringFormat valueFormat = new() { Alignment = StringAlignment.Far };

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

        if (this._config.InfoPanel.TimeOfDay)
        {
            _timeHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _timeValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);
        }

        if (this._config.InfoPanel.GlobalFlag)
        {
            _globalFlagHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _globalFlagValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);
        }

        if (this._config.InfoPanel.SessionType)
        {
            _sessionTypeLabel = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _sessionTypeValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);
        }

        _gripLabel = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
        _gripValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
        headerRect.Offset(0, lineHeight);
        valueRect.Offset(0, lineHeight);

        _airTempLabel = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
        _airTempValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
        headerRect.Offset(0, lineHeight);
        valueRect.Offset(0, lineHeight);

        if (this._config.InfoPanel.TrackTemperature)
        {
            _trackTempLabel = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _trackTempValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);
        }

        _windLabel = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
        _windValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
        headerRect.Offset(0, lineHeight);
        valueRect.Offset(0, lineHeight);

        this.Width = unscaledHeaderWidth + unscaledValueWidth;
        this.Height = (int)(headerRect.Top / this.Scale);
    }

    public sealed override void BeforeStop()
    {
        _font?.Dispose();

        _timeHeader?.Dispose();
        _timeValue?.Dispose();
        _globalFlagHeader?.Dispose();
        _globalFlagValue?.Dispose();
        _sessionTypeLabel?.Dispose();
        _sessionTypeValue?.Dispose();
        _airTempLabel?.Dispose();
        _airTempValue?.Dispose();
        _trackTempLabel?.Dispose();
        _trackTempValue?.Dispose();
        _gripLabel?.Dispose();
        _gripValue?.Dispose();
        _windLabel?.Dispose();
        _windValue?.Dispose();
    }

    public sealed override void Render(Graphics g)
    {
        if (this._config.InfoPanel.TimeOfDay)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(broadCastRealTime.TimeOfDay.TotalMilliseconds * 1000);
            _timeHeader.Draw(g, "Time", this.Scale);
            _timeValue.Draw(g, $"{time:hh\\:mm\\:ss}", this.Scale);
        }

        if (this._config.InfoPanel.GlobalFlag)
        {
            _globalFlagHeader.Draw(g, "Flag", this.Scale);
            _globalFlagValue.Draw(g, ACCSharedMemory.FlagTypeToString(pageGraphics.Flag), this.Scale);
        }

        if (this._config.InfoPanel.SessionType)
        {
            _sessionTypeLabel.Draw(g, "Session", this.Scale);
            _sessionTypeValue.Draw(g, ACCSharedMemory.SessionTypeToString(pageGraphics.SessionType), this.Scale);
        }

        _gripLabel.Draw(g, "Grip", this.Scale);
        _gripValue.Draw(g, pageGraphics.trackGripStatus.ToString(), this.Scale);

        string airTemp = Math.Round(pagePhysics.AirTemp, 2).ToString("F2");
        _airTempLabel.Draw(g, "Air", this.Scale);
        _airTempValue.Draw(g, $"{airTemp} °C", this.Scale);

        if (this._config.InfoPanel.TrackTemperature)
        {
            _trackTempLabel.Draw(g, "Track", this.Scale);
            string roadTemp = Math.Round(pagePhysics.RoadTemp, 2).ToString("F2");
            _trackTempValue.Draw(g, $"{roadTemp} °C", this.Scale);
        }

        _windLabel.Draw(g, "Wind", this.Scale);
        _windValue.Draw(g, $"{Math.Round(pageGraphics.WindSpeed, 2)} km/h", this.Scale);
    }
}
