using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Util.SystemExtensions;
using System.Collections.Generic;
using static RaceElement.ACCSharedMemory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.HUD.ACC.Overlays.Driving.OversteerTrace
{
    internal sealed class OversteerDataJob : AbstractLoopJob
    {
        public OversteerTraceOverlay Overlay { get; private init; }
        public int DataCount { get; private init; }
        public float MaxSlipAngle { get; private init; }

        /// <summary>
        /// Stores oversteer (in slip angle) 
        /// </summary>
        public readonly LinkedList<float> Oversteer = [];

        /// <summary>
        /// Stores understeer (in slip angle) 
        /// </summary>
        public readonly LinkedList<float> Understeer = [];

        public OversteerDataJob(OversteerTraceOverlay overlay, int dataCount, float maxSlipAngle)
        {
            Overlay = overlay;
            DataCount = dataCount;
            MaxSlipAngle = maxSlipAngle;

            float presetOversteer = 0, presetUndersteer = 0;
            if (Overlay.IsPreviewing)
                presetUndersteer = 0.1f;

            for (int i = 0; i < DataCount; i++)
            {
                Oversteer.AddFirst(presetOversteer);
                Understeer.AddFirst(presetUndersteer);
            }
        }

        public sealed override void RunAction()
        {
            if (!Overlay.ShouldRender())
                return;

            SPageFilePhysics pagePhysics = Overlay.pagePhysics;

            float slipRatioFront = (pagePhysics.WheelSlip[(int)Wheel.FrontLeft] + pagePhysics.WheelSlip[(int)Wheel.FrontRight]) / 2;
            float slipRatioRear = (pagePhysics.WheelSlip[(int)Wheel.RearLeft] + pagePhysics.WheelSlip[(int)Wheel.RearRight]) / 2;

            // understeer
            if (slipRatioFront > slipRatioRear)
            {
                float difference = slipRatioFront - slipRatioRear;
                difference.ClipMax(MaxSlipAngle);
                AddDataAndLimitSize(difference, 0);
            }

            // oversteer
            if (slipRatioRear > slipRatioFront)
            {
                float difference = slipRatioRear - slipRatioFront;
                difference.ClipMax(MaxSlipAngle);
                AddDataAndLimitSize(0, difference);
            }
        }

        private void AddDataAndLimitSize(float understeerData, float oversteerData)
        {
            lock (Understeer)
            {
                Understeer.AddFirst(understeerData);
                if (Understeer.Count > DataCount)
                    Understeer.RemoveLast();
            }
            lock (Oversteer)
            {
                Oversteer.AddFirst(oversteerData);
                if (Oversteer.Count > DataCount)
                    Oversteer.RemoveLast();
            }
        }
    }
}
