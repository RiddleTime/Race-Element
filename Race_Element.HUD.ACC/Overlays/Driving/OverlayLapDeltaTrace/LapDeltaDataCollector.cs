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
            for(int i = 0; i < TraceCount; i++)
            {
                PositiveDeltaData.AddLast(0);
                NegativeDeltaData.AddLast(0);
            }
        }

        public void SetupPreviewData()
        {
            PositiveDeltaData.Clear();
            NegativeDeltaData.Clear();
            for (int i = 0; i < TraceCount / 3; i++)
            {
                PositiveDeltaData.AddLast(0);
                NegativeDeltaData.AddLast((float)(1 + Math.Sin(i / 30d)));
            }
            for (int i = 0; i < TraceCount / 3 * 1; i++)
            {
                PositiveDeltaData.AddLast(0);
                NegativeDeltaData.AddLast(0);
            }

            for (int i = TraceCount / 3 * 2; i < TraceCount; i++)
            {
                PositiveDeltaData.AddLast((float)(1 + -Math.Cos(i / 30d)));
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

            lock (PositiveDeltaData)
                if (PositiveDeltaData.Count > TraceCount)
                    PositiveDeltaData.RemoveLast();

            lock (NegativeDeltaData)
                if (NegativeDeltaData.Count > TraceCount)
                    NegativeDeltaData.RemoveLast();
        }
    }
}
