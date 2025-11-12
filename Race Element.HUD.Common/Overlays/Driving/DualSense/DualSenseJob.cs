using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.Data.Games;

using static RaceElement.HUD.Common.Overlays.Driving.DualSense.DS5W;
using static RaceElement.HUD.Common.Overlays.Driving.DualSense.Util;

namespace RaceElement.HUD.Common.Overlays.Driving.DualSense;

internal sealed class DualSenseJob(DualSenseOverlay overlay) : AbstractLoopJob
{
    private TriggerHaptics _triggerHaptics = new();
    public sealed override void RunAction()
    {
        //if (!overlay.ShouldRender())
        //    return;
        ds5w_batch_begin();
        _triggerHaptics.HandleAcceleration(overlay._config.ThrottleSlip);
        _triggerHaptics.HandleBraking(overlay._config.BrakeSlip);
        if (!GameManager.CurrentGame.HasFlag(Game.RaceRoom) &&
            !GameManager.CurrentGame.HasFlag(Game.AssettoCorsa1))
            _triggerHaptics.HandleRumble(overlay._config.Rumble);
        ds5w_batch_end();
    }
    public override void AfterCancel()
    {
    }
}
