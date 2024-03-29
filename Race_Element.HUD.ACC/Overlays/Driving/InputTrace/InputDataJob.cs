﻿using RaceElement.Core.Jobs.LoopJob;
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
    public readonly LinkedList<int> Steering = [];

    /// <summary>
    /// Stores accelerator data
    /// </summary>
    public readonly LinkedList<int> Throttle = [];

    /// <summary>
    /// Stores braking data
    /// </summary>
    public readonly LinkedList<int> Brake = [];

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
            Steering.AddFirst(presetSteering);
            Throttle.AddFirst(presetThrottle);
            Brake.AddFirst(presetBrake);
        }
    }

    public sealed override void RunAction()
    {
        if (!Overlay.ShouldRender())
            return;

        SPageFilePhysics filePhysics = Overlay.pagePhysics;
        lock (Throttle)
        {
            Throttle.AddFirst((int)(filePhysics.Gas * 100));
            if (Throttle.Count > DataCount)
                Throttle.RemoveLast();
        }
        lock (Brake)
        {
            Brake.AddFirst((int)(filePhysics.Brake * 100));
            if (Brake.Count > DataCount)
                Brake.RemoveLast();
        }

        lock (Steering)
        {
            Steering.AddFirst((int)((filePhysics.SteerAngle + 1.0) / 2 * 100));
            if (Steering.Count > DataCount)
                Steering.RemoveLast();
        }
    }
}
