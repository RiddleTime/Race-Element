using RaceElement.Core.Jobs.Loop;
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
        public readonly List<float> Oversteer = [];

        /// <summary>
        /// Stores understeer (in slip angle) 
        /// </summary>
        public readonly List<float> Understeer = [];

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
                Oversteer.Insert(0, presetOversteer);
                Understeer.Insert(0, presetUndersteer);
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

        /// <summary>
        /// Adds new under and oversteer data to <see cref="Understeer"/> and <see cref="Oversteer"/>.
        /// Also limits the size of both linked lists determined by <see cref="DataCount"/>.
        /// The method locks both of the linked lists.
        /// </summary>
        /// <param name="understeerData"></param>
        /// <param name="oversteerData"></param>
        private void AddDataAndLimitSize(float understeerData, float oversteerData)
        {
            lock (Understeer)
            {
                Understeer.Insert(0, understeerData);
                if (Understeer.Count > DataCount)
                    Understeer.RemoveAt(Understeer.Count - 1);
            }
            lock (Oversteer)
            {
                Oversteer.Insert(0, oversteerData);
                if (Oversteer.Count > DataCount)
                    Oversteer.RemoveAt(Oversteer.Count - 1);
            }
        }
    }
}
