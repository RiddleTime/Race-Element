using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Games;
using System.Diagnostics;
using static RaceElement.HUD.Common.Overlays.Driving.DSX.Resources;

namespace RaceElement.HUD.Common.Overlays.Driving.DSX;

internal sealed class DsxJob(DsxOverlay? overlay) : AbstractLoopJob
{
    public sealed override void RunAction()
    {
        if (overlay == null)
            return;

        if (!GameManager.IsGameRunning && !overlay._config.Behavior.IgnoreGameRequirement)
        {
            if (overlay._hasSetLighting)
            {
                overlay?.StopClient();
            }

            return;
        }

        if (overlay?._client == null)
        {
            try
            {
                overlay?.CreateEndPoint();
                overlay?.SetLighting();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            Debug.WriteLine("Created enpoint and set lighting!");
            return;
        }

        DsxPacket tcPacket = TriggerHaptics.HandleAcceleration(overlay?._config);
        if (tcPacket != null)
        {
            overlay?.Send(tcPacket);
            //ServerResponse response = Receive();
            //HandleResponse(response);
        }

        DsxPacket absPacket = TriggerHaptics.HandleBraking(overlay?._config);
        if (absPacket != null)
        {
            overlay?.Send(absPacket);
            //ServerResponse response = Receive();
            //HandleResponse(response);
        }
    }

    public override void AfterCancel()
    {
        overlay?.StopClient();
    }
}
