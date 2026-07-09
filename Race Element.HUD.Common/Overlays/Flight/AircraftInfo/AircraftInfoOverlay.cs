using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Flight.AircraftInfo;

[Overlay(
    Name = "Aircraft Info",
    Description = "Displays the Type and Model",
    SupportedGames = Game.MicrosoftFlightSimulator2020 | Game.MicrosoftFlightSimulator2024,
    Authors = ["Reinier Klarenberg"]
)]
internal sealed class AircraftInfoOverlay(Rectangle rectangle) : CommonAbstractOverlay(rectangle, "Aircraft Info")
{
    private readonly AircraftInfoConfiguration _config = new();
    private sealed class AircraftInfoConfiguration : OverlayConfiguration
    {
        public AircraftInfoConfiguration() => GenericConfiguration.AllowRescale = true;
    }

    private InfoPanel _infoPanel;

    public sealed override void BeforeStart()
    {
        _infoPanel = new(12, 300);

        Width = 300;
        Height = _infoPanel.FontHeight * 3 + _infoPanel.ExtraLineSpacing * 2;
    }

    public sealed override void BeforeStop() => _infoPanel?.Dispose();

    public sealed override void Render(Graphics g)
    {
        _infoPanel.AddLine("Type", $"{SimDataProvider.LocalPlane.ATC.Type}");
        _infoPanel.AddLine("Model", $"{SimDataProvider.LocalPlane.ATC.Model}");
        _infoPanel.AddLine("ID", $"{SimDataProvider.LocalPlane.ATC.Identifier}");
        _infoPanel.Draw(g);
    }
}
