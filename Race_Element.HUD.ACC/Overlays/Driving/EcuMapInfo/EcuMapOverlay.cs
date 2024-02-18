using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayEcuMapInfo;

[Overlay(Name = "ECU Maps", Version = 1.00, OverlayType = OverlayType.Drive,
    Description = "A panel showing information about the current ECU Map.",
Authors = ["Reinier Klarenberg"])]
internal sealed class EcuMapOverlay : AbstractOverlay
{
    private const int PanelWidth = 270;
    private readonly InfoPanel _panel = new(10, PanelWidth);

    private readonly EcuMapConfiguration _config = new();
    private sealed class EcuMapConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Info Panel", "Show or hide additional information in the panel.")]
        public InfoPanelGrouping InfoPanel { get; init; } = new InfoPanelGrouping();
        public class InfoPanelGrouping
        {
            [ToolTip("Displays the number of the ecu map.")]
            public bool MapNumber { get; init; } = true;
        }

        public EcuMapConfiguration() => this.GenericConfiguration.AllowRescale = true;
    }

    public EcuMapOverlay(Rectangle rectangle) : base(rectangle, "ECU Maps")
    {
        this.RefreshRateHz = 3;

        this.Width = PanelWidth + 1;
        this.Height = _panel.FontHeight * 5 + 1;
    }

    public sealed override void BeforeStart()
    {
        if (!this._config.InfoPanel.MapNumber)
            this.Height -= _panel.FontHeight;
    }

    public sealed override void Render(Graphics g)
    {
        EcuMap current = EcuMaps.GetMap(pageStatic.CarModel, pageGraphics.EngineMap);

        if (current != null)
        {
            if (this._config.InfoPanel.MapNumber)
                _panel.AddLine("Map", $"{current.Index}");
            _panel.AddLine("Power", $"{current.Power}");
            _panel.AddLine("Condition", $"{current.Conditon}");
            _panel.AddLine("Fuel", $"{current.FuelConsumption}");
            _panel.AddLine("Throttle", $"{current.ThrottleMap}");
        }
        else
        {
            _panel.AddLine("Car", "Not supported");
            _panel.AddLine("Model", pageStatic.CarModel);
            _panel.AddLine("Map", $"{pageGraphics.EngineMap + 1}");
        }

        _panel.Draw(g);
    }
}
