
using RaceElement.Data.ACC.Session;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.ShouldRenderTest;

[Overlay(Name = "Auto Visibility Tester",
    Description = "Shows info about active HUDs",
    OverlayType = OverlayType.Pitwall)]
internal sealed class AutoVisibilityTesterOverlay : AbstractOverlay
{
    private readonly AutoVisibilityTesterConfiguration _config = new();
    private sealed class AutoVisibilityTesterConfiguration : OverlayConfiguration
    {
        public AutoVisibilityTesterConfiguration() => this.GenericConfiguration.AllowRescale = true;
    }

    private InfoPanel _panel;
    public AutoVisibilityTesterOverlay(Rectangle rectangle) : base(rectangle, "Auto Visibility Tester")
    {
        Width = 600;
        Height = 200;
        RefreshRateHz = 10;
    }

    public sealed override void BeforeStart() => _panel = new InfoPanel(11, Width);
    public sealed override void BeforeStop() => _panel?.Dispose();

    public sealed override bool ShouldRender() => true;
    public sealed override void Render(Graphics g)
    {

        _panel.AddLine($"Current Game", $"{GameManager.CurrentGame.ToShortName()}");
        _panel.AddLine($"IsGameRunning", $"{GameManager.IsGameRunning}");
        _panel.AddLine($"DemoMode", $"{HudSettings.Cached.DemoMode}");

        _panel.AddLine($"pageGrahics.Status", $"{pageGraphics.Status}");
        _panel.AddLine($"ignition", $"{pagePhysics.IgnitionOn}");

        _panel.AddLine($"GlobalRed", $"{pageGraphics.GlobalRed}");

        _panel.AddLine($"Broadcast Phase", $"{broadCastRealTime.Phase}");
        _panel.AddLine($"IsFormationLap", $"{RaceSessionState.IsFormationLap(pageGraphics.GlobalRed, broadCastRealTime.Phase)}");

        _panel.AddLine($"LocalCarIndex/Spectating", $"{pageGraphics.PlayerCarID}/{broadCastRealTime.FocusedCarIndex}");

        _panel.AddLine($"ShouldRender?", $"{ShouldRenderClone()}");

        // draw the panel
        _panel.Draw(g);
    }

    public bool ShouldRenderClone()
    {
        if (HudSettings.Cached.DemoMode)
            return true;

        if (!GameManager.IsGameRunning && GameManager.CurrentGame == Game.AssettoCorsaCompetizione)
            return false;

        bool shouldRender = true;

        if (pageGraphics != null)
        {
            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || !pagePhysics.IgnitionOn)
                shouldRender = false;

            if (pageGraphics.GlobalRed)
                shouldRender = false;

            if (RaceSessionState.IsFormationLap(pageGraphics.GlobalRed, broadCastRealTime.Phase))
                shouldRender = true;

            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_REPLAY)
                shouldRender = false;

            if (broadCastRealTime.FocusedCarIndex != pageGraphics.PlayerCarID)
                shouldRender = false;
        }

        return shouldRender;
    }
}