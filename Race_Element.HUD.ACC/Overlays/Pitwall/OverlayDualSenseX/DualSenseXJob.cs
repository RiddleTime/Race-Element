using RaceElement.Core.Jobs.Loop;
using System;
using static RaceElement.HUD.ACC.Overlays.Pitwall.OverlayDualSenseX.DualSenseXResources;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayDualSenseX
{
    internal sealed class DualSenseXJob(DualSenseXOverlay overlay) : AbstractLoopJob
    {
        public sealed override void RunAction()
        {
            if (!overlay.ShouldRender())
                return;

            if (overlay._client == null)
            {
                try
                {
                    overlay.CreateEndPoint();
                    overlay.SetLighting();
                }
                catch (Exception)
                {
                   // let's not cause an app crash, shall we?
                }
            }

            Packet tcPacket = TriggerHaptics.HandleAcceleration(ref overlay.pagePhysics, overlay._config.ThrottleHaptics);
            if (tcPacket != null)
            {
                overlay.Send(tcPacket);
                //ServerResponse response = Receive();
                //HandleResponse(response);
            }

            Packet absPacket = TriggerHaptics.HandleBraking(ref overlay.pagePhysics, overlay._config.BrakeHaptics);
            if (absPacket != null)
            {
                overlay.Send(absPacket);
                //ServerResponse response = Receive();
                //HandleResponse(response);
            }
        }
        public override void AfterCancel()
        {
            overlay?._client?.Close();
            overlay?._client?.Dispose();
        }
    }
}
