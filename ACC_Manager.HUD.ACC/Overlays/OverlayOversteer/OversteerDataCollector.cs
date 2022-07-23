using ACC_Manager.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.HUD.ACC.Overlays.OverlaySlipAngle
{
    internal class OversteerDataCollector
    {

        private bool IsCollecting = false;
        public int TraceCount = 300;

        public LinkedList<int> Oversteer = new LinkedList<int>();

        public void Collect(SPageFilePhysics pagePhysics)
        {
            lock (Oversteer)
            {
                float slipRatioFront = (pagePhysics.WheelSlip[(int)Wheel.FrontLeft] + pagePhysics.WheelSlip[(int)Wheel.FrontRight]) / 2;
                float slipRatioRear = (pagePhysics.WheelSlip[(int)Wheel.RearLeft] + pagePhysics.WheelSlip[(int)Wheel.RearRight]) / 2;

                float max = 25;
                float diff = slipRatioRear - slipRatioFront;
                diff += max / 2;
                diff.Clip(0, max);

                float normalized = diff * 100 / max;

                Oversteer.AddFirst((int)(normalized));
                if (Oversteer.Count > TraceCount)
                    Oversteer.RemoveLast();
            }
        }

        public void Start()
        {
            IsCollecting = true;

            for (int i = 0; i < TraceCount; i++)
            {
                Oversteer.AddLast(0);
            }

            new Thread(x =>
            {
                while (IsCollecting)
                {
                    Thread.Sleep(1000 / 30);
                    if (OversteerOverlay.Instance != null && OversteerOverlay.Instance.pagePhysics != null)
                    {
                        Collect(OversteerOverlay.Instance.pagePhysics);
                        OversteerOverlay.Instance.RequestRedraw();
                    }
                }
            }).Start();
        }

        public void Stop()
        {
            IsCollecting = false;
        }
    }
}
