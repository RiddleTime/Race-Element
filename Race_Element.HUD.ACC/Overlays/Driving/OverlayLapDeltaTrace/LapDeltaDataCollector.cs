using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayLapDeltaGraph
{
    internal class LapDeltaDataCollector
    {
        private readonly int TraceCount = 300;
        public float MaxDelta { get; set; }

        public LinkedList<float> PositiveDeltaData = new LinkedList<float>();
        public LinkedList<float> NegativeDeltaData = new LinkedList<float>();

        public LapDeltaDataCollector(int traceCount)
        {
            TraceCount = traceCount;
            for (int i = 0; i < TraceCount; i++)
            {
                PositiveDeltaData.AddLast(0);
                NegativeDeltaData.AddLast(0);
            }
        }

        public void Collect(float delta)
        {
            if (delta < 0)
            {
                NegativeDeltaData.AddFirst(-delta);
                PositiveDeltaData.AddFirst(0);
            }
            else
            {
                PositiveDeltaData.AddFirst(delta);
                NegativeDeltaData.AddFirst(0);
            }

            if (PositiveDeltaData.Count > TraceCount)
                PositiveDeltaData.RemoveLast();

            if (NegativeDeltaData.Count > TraceCount)
                NegativeDeltaData.RemoveLast();
        }
    }
}
