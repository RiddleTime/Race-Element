using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Games;
using static RaceElement.HUD.Common.Overlays.Driving.DSX.Resources;

namespace RaceElement.HUD.Common.Overlays.Driving.DSX;

internal sealed class DsxJob(DsxOverlay overlay) : AbstractLoopJob
{
    private bool _hasSetLighting = false;

    public sealed override void RunAction()
    {
        //if (!overlay.ShouldRender())
        //    return;

        if (!GameManager.IsGameRunning)
        {
            overlay?._client?.Close();
            overlay?._client?.Dispose();


            if (overlay._hasSetLighting)
            {
                DsxPacket resetPacket = new();
                resetPacket.AddResetToPacket(0);
                overlay.Send(resetPacket);
                overlay._hasSetLighting = false;
            }
            return;
        }

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
