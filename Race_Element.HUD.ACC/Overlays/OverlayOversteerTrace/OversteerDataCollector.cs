using RaceElement.Util.SystemExtensions;
using RaceElement.HUD.Overlay.Internal;
using System.Collections.Generic;
using System.Threading;
using static RaceElement.ACCSharedMemory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.HUD.ACC.Overlays.OverlaySlipAngle
{
    internal class OversteerDataCollector
    {
        public int TraceCount = 300;
        public int Herz = 30;
        public float MaxSlipAngle { get; set; }

        public LinkedList<float> OversteerData = new LinkedList<float>();
        public LinkedList<float> UndersteerData = new LinkedList<float>();


        public OversteerDataCollector(int traceCount)
        {
            TraceCount = traceCount;
            for (int i = 0; i < TraceCount; i++)
            {
                OversteerData.AddLast(0);
                UndersteerData.AddLast(0);
            }
        }

        public void Collect(SPageFilePhysics pagePhysics)
        {
            float slipRatioFront = (pagePhysics.WheelSlip[(int)Wheel.FrontLeft] + pagePhysics.WheelSlip[(int)Wheel.FrontRight]) / 2;
            float slipRatioRear = (pagePhysics.WheelSlip[(int)Wheel.RearLeft] + pagePhysics.WheelSlip[(int)Wheel.RearRight]) / 2;

            // understeer
            if (slipRatioFront > slipRatioRear)
            {
                float diff = slipRatioFront - slipRatioRear;
                diff.ClipMax(MaxSlipAngle);
                lock (UndersteerData)
                    UndersteerData.AddFirst(diff);
                lock (OversteerData)
                    OversteerData.AddFirst(0);
            }

            // oversteer
            if (slipRatioRear > slipRatioFront)
            {
                float diff = slipRatioRear - slipRatioFront;
                diff.ClipMax(MaxSlipAngle);
                lock (OversteerData)
                    OversteerData.AddFirst(diff);
                lock (UndersteerData)
                    UndersteerData.AddFirst(0);
            }

            lock (OversteerData)
                if (OversteerData.Count > TraceCount)
                    OversteerData.RemoveLast();

            lock (UndersteerData)
                if (UndersteerData.Count > TraceCount)
                    UndersteerData.RemoveLast();
        }
    }
}
