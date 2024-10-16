using RaceElement.Core.Jobs.Loop;
using System.Collections.Generic;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.HUD.ACC.Overlays.Driving.InputTrace;

internal sealed class InputDataJob : AbstractLoopJob
{
    public InputTraceOverlay Overlay { get; private init; }
    public int DataCount { get; private init; }

    /// <summary>
    /// Stores Steering, 50 % is center 
    /// </summary>
    public readonly List<int> Steering = [];

    /// <summary>
    /// Stores accelerator data
    /// </summary>
    public readonly List<int> Throttle = [];

    /// <summary>
    /// Stores braking data
    /// </summary>
    public readonly List<int> Brake = [];

    public InputDataJob(InputTraceOverlay overlay, int dataCount)
    {
        Overlay = overlay;
        DataCount = dataCount;

        int presetSteering = 50, presetThrottle = 0, presetBrake = 0;
        if (Overlay.IsPreviewing)
        {
            presetSteering = 45;
            presetThrottle = 55;
            presetBrake = 5;
        }
        for (int i = 0; i < DataCount; i++)
        {
            Steering.Insert(0, presetSteering);
            Throttle.Insert(0, presetThrottle);
            Brake.Insert(0, presetBrake);
        }
    }

    public sealed override void RunAction()
    {
        if (!Overlay.ShouldRender())
            return;

        SPageFilePhysics filePhysics = Overlay.pagePhysics;
        lock (Throttle)
        {
            Throttle.Insert(0, (int)(filePhysics.Gas * 100));
            if (Throttle.Count > DataCount)
                Throttle.RemoveAt(Throttle.Count - 1);
        }
        lock (Brake)
        {
            Brake.Insert(0, (int)(filePhysics.Brake * 100));
            if (Brake.Count > DataCount)
                Brake.RemoveAt(Brake.Count - 1);
        }

        lock (Steering)
        {
            Steering.Insert(0, (int)((filePhysics.SteerAngle + 1.0) / 2 * 100));
            if (Steering.Count > DataCount)
                Steering.RemoveAt(Steering.Count - 1);
        }
    }
}
