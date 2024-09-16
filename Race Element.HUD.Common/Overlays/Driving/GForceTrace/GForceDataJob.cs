using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.Common;
using RaceElement.Util.SystemExtensions;
using System.Numerics;

namespace RaceElement.HUD.Common.Overlays.Driving.GForceTrace;

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

        Vector3 acceleration = SimDataProvider.LocalCar.Physics.Acceleration;

        float latG = acceleration[0] *= -1;
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

        float longG = acceleration[2] *= -1;
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

