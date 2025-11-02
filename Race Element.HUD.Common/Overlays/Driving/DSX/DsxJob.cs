using RaceElement.Core.Jobs.Loop;
using static RaceElement.HUD.Common.Overlays.Driving.DSX.Resources;

namespace RaceElement.HUD.Common.Overlays.Driving.DSX;

internal sealed class DsxJob(DsxOverlay overlay) : AbstractLoopJob
{
    public sealed override void RunAction()
    {
        //if (!overlay.ShouldRender())
        //    return;

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

        DsxPacket tcPacket = TriggerHaptics.HandleAcceleration(overlay._config);
        if (tcPacket != null)
        {
            overlay.Send(tcPacket);
            //ServerResponse response = Receive();
            //HandleResponse(response);
        }

        DsxPacket absPacket = TriggerHaptics.HandleBraking(overlay._config);
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
