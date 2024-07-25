using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Util.SystemExtensions;
using System.Collections.Generic;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.HUD.ACC.Overlays.Driving.GForceTrace;

internal sealed class GForceDataJob : AbstractLoopJob
{
    public GForceTraceOverlay Overlay { get; private init; }
    public int DataCount { get; private init; }

    public readonly List<int> Lateral = [];
    public readonly List<int> Longitudinal = [];

    public GForceDataJob(GForceTraceOverlay overlay, int dataCount)
    {
        Overlay = overlay;
        DataCount = dataCount;

        int presetLat = 50, presetLong = 50;
        if (Overlay.IsPreviewing)
        {
            presetLat = 45;
            presetLong = 55;
        }
        for (int i = 0; i < DataCount; i++)
        {
            Lateral.Insert(0, presetLat);
            Longitudinal.Insert(0, presetLat);
        }
    }

    public sealed override void RunAction()
    {
        if (!Overlay.ShouldRender())
            return;

        SPageFilePhysics filePhysics = Overlay.pagePhysics;

        float latG = filePhysics.AccG[0] *= -1;
        float maxLatG = Overlay._config.Data.MaxLatG;
        latG.Clip(-maxLatG, maxLatG);
        latG += maxLatG;

        int latData = (int)(latG * 100 / (maxLatG * 2));
        lock (Lateral)
        {
            Lateral.Insert(0, latData);

            if (Lateral.Count > DataCount)
                Lateral.RemoveAt(Lateral.Count - 1);
        }

        float longG = filePhysics.AccG[2] *= -1;
        float maxLongG = Overlay._config.Data.MaxLongG;
        longG.Clip(-maxLongG, maxLongG);
        longG += maxLongG;

        int longData = (int)(longG * 100 / (maxLongG * 2));
        lock (Longitudinal)
        {
            Longitudinal.Insert(0, longData);

            if (Longitudinal.Count > DataCount)
                Longitudinal.RemoveAt(Longitudinal.Count - 1);
        }
    }
}

