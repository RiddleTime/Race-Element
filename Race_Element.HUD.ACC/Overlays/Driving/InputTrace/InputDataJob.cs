using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.Util.SystemExtensions;
using System.Collections.Generic;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.HUD.ACC.Overlays.Driving.InputTrace;

internal sealed class InputDataJob : AbstractLoopJob
{
    public InputTraceOverlay Overlay { get; private init; }

    private InputTraceConfiguration Config { get; set; }

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

    /// <summary>
    /// Stores ABS Activation data
    /// </summary>
    public readonly List<bool> ABS = [];

    /// <summary>
    /// Stores Traction Control activation data
    /// </summary>
    public readonly List<bool> TC = [];

    /// <summary>
    /// Stores the Car's Speed data
    /// </summary>
    public readonly List<int> Speed = [];

    public InputDataJob(InputTraceOverlay overlay, InputTraceConfiguration config, int dataCount)
    {
        Overlay = overlay;
        Config = config;
        DataCount = dataCount;

        int presetSteering = 50, presetThrottle = 0, presetBrake = 0, presetSpeed = 0;
        if (Overlay.IsPreviewing)
        {
            presetSteering = 45;
            presetThrottle = 55;
            presetSpeed = 25;
            presetBrake = 5;
        }
        for (int i = 0; i < DataCount; i++)
        {
            Steering.Insert(0, presetSteering);
            Throttle.Insert(0, presetThrottle);
            Brake.Insert(0, presetBrake);
            ABS.Insert(0, false);
            TC.Insert(0, false);
            Speed.Insert(0, presetSpeed);
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
        lock (ABS)
        {
            ABS.Insert(0, filePhysics.Abs > 0);
            if (ABS.Count > DataCount)
                ABS.RemoveAt(ABS.Count - 1);
        }
        lock (TC)
        {
            TC.Insert(0, filePhysics.TC > 0);
            if (TC.Count > DataCount)
                TC.RemoveAt(TC.Count - 1);
        }
        lock (Speed)
        {
            int speed = (int)filePhysics.SpeedKmh;
            speed.Clip(0, Config.Data.MaxSpeed);
            speed = (int)(100f * speed / Config.Data.MaxSpeed);
            Speed.Insert(0, speed);
            if (Speed.Count > DataCount)
                Speed.RemoveAt(Speed.Count - 1);
        }
    }
}
