using System.Drawing;
using System.Drawing.Drawing2D;
using Race_Element.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.InfoPanel;
using RaceElement.HUD.Overlay.Util;

namespace RaceElement.HUD.Common.Overlays.Driving.CarElectronics;

[Overlay(
    Name = "Car Electronics",
    Description = "Shows the Brake Bias, optionally ABS and TC.",
    SupportedGames = Game.Automobilista2,
    Authors = ["Connor Molz, Reinier Klarenberg"]
)]
public sealed class CarElectronicsOverlay : CommonAbstractOverlay
{
    private readonly CarElectronicsConfig _config = new();

    private sealed class CarElectronicsConfig : OverlayConfiguration
    {
        [ConfigGrouping("Info Panel", "Show or hide additional information in the panel.")]
        public InfoPanelGrouping InfoPanel { get; init; } = new InfoPanelGrouping();

        public sealed class InfoPanelGrouping
        {
            [ToolTip("Toggle ABS in overlay")]
            public bool ShowAbs { get; init; } = true;

            [ToolTip("Toggle TC in overlay")]
            public bool ShowTc { get; init; } = true;
        }

        public CarElectronicsConfig()
        {
            this.GenericConfiguration.AllowRescale = true;
        }
    }

    private DataCollector? _dataCollector;
    private DataModel _model = new();
    private readonly record struct DataModel(float BrakeBias, int AbsLevel, int TcLevel);
    private sealed class DataCollector : AbstractCollectionJob<DataModel>
    {
        public sealed override DataModel Collect => new()
        {
            BrakeBias = SimDataProvider.LocalCar.Electronics.BrakeBias,
            TcLevel = SimDataProvider.LocalCar.Electronics.TractionControlLevel < 0 ? 0 : SimDataProvider.LocalCar.Electronics.TractionControlLevel,
            AbsLevel = SimDataProvider.LocalCar.Electronics.AbsLevel,
        };
    }

    // Window Components
    private Font _font;

    private PanelText _absHeader;
    private PanelText _absValue;
    private PanelText _tcHeader;
    private PanelText _tcValue;
    private PanelText _bbHeader;
    private PanelText _bbValue;

    public CarElectronicsOverlay(Rectangle rectangle) : base(rectangle, "Car Electronics") => RequestsDrawItself = true;

    public sealed override void SetupPreviewData() => _model = new()
    {
        BrakeBias = 48.3f,
        AbsLevel = 3,
        TcLevel = 2
    };

    // Before render function to setup the window and size
    public sealed override void BeforeStart()
    {
        _font = FontUtil.FontSegoeMono(10f * this.Scale);

        int lineHeight = _font.Height;

        int unscaledHeaderWidth = 40;
        int unscaledValueWidth = 58;

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
            using LinearGradientBrush brush = new(panelRect, Color.FromArgb(185, 0, 0, 0),
                Color.FromArgb(255, 10, 10, 10), LinearGradientMode.BackwardDiagonal);
            g.FillPath(brush, path);
            using Pen underlinePen = new(accentColor);
            g.DrawLine(underlinePen, 0 + roundingRadius / 2, lineHeight, headerWidth, lineHeight - 1);
        });

        CachedBitmap valueBackground = new(valueWidth, lineHeight, g =>
        {
            Rectangle panelRect = new(0, 0, valueWidth, lineHeight);
            using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, roundingRadius, 0, 0);
            using LinearGradientBrush brush = new(panelRect, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(185, 0, 0, 0),
                LinearGradientMode.ForwardDiagonal);
            g.FillPath(brush, path);
            using Pen underlinePen = new(accentColor);
            g.DrawLine(underlinePen, 0, lineHeight - 1, valueWidth, lineHeight - 1);
        });

        // Init ABS, TC and BB headers and values
        _bbHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
        _bbHeader.Render("BB");
        _bbValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
        headerRect.Offset(0, lineHeight);
        valueRect.Offset(0, lineHeight);

        if (_config.InfoPanel.ShowAbs)
        {
            _absHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _absHeader.Render("ABS");
            _absValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);
        }

        if (_config.InfoPanel.ShowTc)
        {
            _tcHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _tcHeader.Render("TC");
            _tcValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);
        }

        this.Width = unscaledHeaderWidth + unscaledValueWidth;
        this.Height = (int)(headerRect.Top / this.Scale);

        if (!IsPreviewing)
        {
            _dataCollector = new() { IntervalMillis = (int)(1000f / 30) };
            _dataCollector.OnCollected += OnNewData;
            _dataCollector.Run();
        }
    }

    private long _lastRedraw;
    private void OnNewData(object? sender, DataModel newDataModel)
    {
        // update the data model if it doesn't match the new one.
        if (_model != newDataModel)
        {
            _model = newDataModel;

            if (ShouldRender())
                RequestRedraw();

            _lastRedraw = TimeProvider.System.GetTimestamp();
        }

        // render at least once a second.
        if (TimeProvider.System.GetElapsedTime(_lastRedraw) > TimeSpan.FromSeconds(1))
        {
            if (ShouldRender())
                RequestRedraw();

            _lastRedraw = TimeProvider.System.GetTimestamp();
        }
    }

    public sealed override void Render(Graphics g)
    {
        _bbHeader.Draw(g, this.Scale);
        _bbValue.Draw(g, $"{_model.BrakeBias:F2}", this.Scale);

        if (_config.InfoPanel.ShowAbs)
        {
            _absHeader.Draw(g, this.Scale);
            _absValue.Draw(g, $"{_model.AbsLevel}", this.Scale);
        }

        if (_config.InfoPanel.ShowTc)
        {
            _tcHeader.Draw(g, this.Scale);
            _tcValue.Draw(g, $"{_model.TcLevel}", this.Scale);
        }
    }

    public sealed override void BeforeStop()
    {
        if (!IsPreviewing && _dataCollector != null)
        {
            _dataCollector.OnCollected -= OnNewData;
            _dataCollector.CancelJoin();
        }

        _font?.Dispose();
        _bbHeader?.Dispose();
        _bbValue?.Dispose();
        _absHeader?.Dispose();
        _absValue?.Dispose();
        _tcHeader?.Dispose();
        _tcValue?.Dispose();
    }

}