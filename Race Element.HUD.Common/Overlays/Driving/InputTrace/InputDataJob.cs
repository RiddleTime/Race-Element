using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.Common;
using System.Diagnostics;

namespace RaceElement.HUD.Common.Overlays.Driving.InputTrace;

internal sealed class InputDataJob : AbstractLoopJob
{
    public InputTraceOverlay Overlay { get; private init; }
    public int DataCount { get; private init; }

    internal readonly object _lock = new();

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
        if (!Overlay.ShouldRender()) return;

        lock (_lock)
        {
            Throttle.Insert(0, (int)(SimDataProvider.LocalCar.Inputs.Throttle * 100));
            if (Throttle.Count > DataCount)
                Throttle.RemoveAt(Throttle.Count - 1);

            Brake.Insert(0, (int)(SimDataProvider.LocalCar.Inputs.Brake * 100));
            if (Brake.Count > DataCount)
                Brake.RemoveAt(Brake.Count - 1);

            Steering.Insert(0, (int)((SimDataProvider.LocalCar.Inputs.Steering + 1.0) / 2 * 100));
            if (Steering.Count > DataCount)
                Steering.RemoveAt(Steering.Count - 1);
        }
    }
}
