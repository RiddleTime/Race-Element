using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Collections.Generic;
using System.Threading;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.HUD.ACC.Overlays.OverlayInputTrace;

internal class InputDataCollector
{
    public int TraceCount = 300;

    public LinkedList<int> Throttle = new();
    public LinkedList<int> Brake = new();
    public LinkedList<int> Steering = new();

    public InputDataCollector(int traceCount)
    {
        TraceCount = traceCount;
        for (int i = 0; i < TraceCount; i++)
        {
            Throttle.AddLast(0);
            Brake.AddLast(0);
            Steering.AddLast(50);
        }
    }

    public void Collect(SPageFilePhysics filePhysics)
    {
        lock (Throttle)
        {
            Throttle.AddFirst((int)(filePhysics.Gas * 100));
            if (Throttle.Count > TraceCount)
                Throttle.RemoveLast();
        }
        lock (Brake)
        {
            Brake.AddFirst((int)(filePhysics.Brake * 100));
            if (Brake.Count > TraceCount)
                Brake.RemoveLast();
        }

        lock (Steering)
        {
            Steering.AddFirst((int)((filePhysics.SteerAngle + 1.0) / 2 * 100));
            if (Steering.Count > TraceCount)
                Steering.RemoveLast();
        }
    }
}
