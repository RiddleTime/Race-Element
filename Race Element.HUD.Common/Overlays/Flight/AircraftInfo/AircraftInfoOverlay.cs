using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Flight.AircraftInfo;

[Overlay(
    Name = "Aircraft Info",
    Description = "Displays the Type and Model"
)]
internal class AircraftInfoOverlay(Rectangle rectangle) : CommonAbstractOverlay(rectangle, "Aircraft Info")
{
    private readonly AircraftInfoConfiguration _config = new();
    private sealed class AircraftInfoConfiguration : OverlayConfiguration
    {
        public AircraftInfoConfiguration() => GenericConfiguration.AllowRescale = true;
    }

    private InfoPanel _infoPanel;

    public override void BeforeStart()
    {
        _infoPanel = new(12, 300);

        Width = 300;
        Height = _infoPanel.FontHeight * 2 + _infoPanel.ExtraLineSpacing * 3;
    }

    public override void BeforeStop()
    {
        _infoPanel?.Dispose();
    }

    public override void Render(Graphics g)
    {
        _infoPanel.AddLine("Type", $"{SimDataProvider.LocalPlane.ATC.Type}");
        _infoPanel.AddLine("Model", $"{SimDataProvider.LocalPlane.ATC.Model}");
        _infoPanel.Draw(g);
    }
}
