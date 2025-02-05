using DualSenseAPI;
using RaceElement.Core.Jobs.Loop;

namespace RaceElement.HUD.Common.Overlays.Pitwall.DualSenseInternal;

internal sealed class DsiJob(DsiOverlay overlay) : AbstractLoopJob
{
    private DualSense? _controller;

    public override void BeforeRun()
    {
        DualSense[] available = DualSense.EnumerateControllers().ToArray();
        if (available != null && available.Length > 0)
        {
            _controller = available.First();
            _controller.Acquire();
        }
    }
    public sealed override void RunAction()
    {
        if (_controller == null || !overlay.ShouldRender()) return;


        //_controller.OutputState.RightRumble = 0.5f;
        //_controller.OutputState.L2Effect = new TriggerEffect.Continuous(0, 0.8f);
        //_controller.ReadWriteOnce();

        TriggerHaptics.HandleAcceleration(overlay._config, _controller);


        TriggerHaptics.HandleBraking(overlay._config, _controller);

    }

    public override void AfterCancel()
    {
        if (_controller != null)
        {
            SetDefaultFeedback();
            _controller?.Release();
        }
    }

    private void SetDefaultFeedback()
    {
        _controller.OutputState.RightRumble = 0f;
        _controller.OutputState.LeftRumble = 0f;
        _controller.OutputState.L2Effect = TriggerEffect.Default;
        _controller.OutputState.R2Effect = TriggerEffect.Default;
        _controller.ReadWriteOnce();
    }

}
