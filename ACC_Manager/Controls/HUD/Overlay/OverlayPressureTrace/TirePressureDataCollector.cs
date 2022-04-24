using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ACCSetupApp.ACCSharedMemory;
using static ACCSetupApp.SetupParser.SetupConverter;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayPressureTrace
{
    internal class TirePressureDataCollector
    {
        private bool IsCollecting = false;
        public int TraceCount = 300;

        public LinkedList<float> FrontLeft = new LinkedList<float>();
        public LinkedList<float> FrontRight = new LinkedList<float>();
        public LinkedList<float> RearLeft = new LinkedList<float>();
        public LinkedList<float> RearRight = new LinkedList<float>();
        private readonly ACCSharedMemory sharedMemory = new ACCSharedMemory();

        public void Collect(SPageFilePhysics filePhysics)
        {
            lock (FrontLeft)
            {
                FrontLeft.AddFirst(CorrectToBounds(filePhysics.WheelPressure[(int)Wheel.FrontLeft]));
                if (FrontLeft.Count > TraceCount)
                {
                    FrontLeft.RemoveLast();
                }
            }
            lock (FrontRight)
            {
                FrontRight.AddFirst(CorrectToBounds(filePhysics.WheelPressure[(int)Wheel.FrontRight]));
                if (FrontRight.Count > TraceCount)
                {
                    FrontRight.RemoveLast();
                }
            }
            lock (RearLeft)
            {
                RearLeft.AddFirst(CorrectToBounds(filePhysics.WheelPressure[(int)Wheel.RearLeft]));
                if (RearLeft.Count > TraceCount)
                {
                    RearLeft.RemoveLast();
                }
            }
            lock (RearRight)
            {
                RearRight.AddFirst(CorrectToBounds(filePhysics.WheelPressure[(int)Wheel.RearRight]));
                if (RearRight.Count > TraceCount)
                {
                    RearRight.RemoveLast();
                }
            }
        }

        private float CorrectToBounds(float value)
        {
            if (value < TirePressureGraph.boundMin)
                value = TirePressureGraph.boundMin;

            if (value > TirePressureGraph.boundMax)
                value = TirePressureGraph.boundMax;

            return value;
        }

        public void Start()
        {
            IsCollecting = true;

            new Thread(x =>
            {
                while (IsCollecting)
                {
                    Thread.Sleep(1000 / 2);
                    Collect(sharedMemory.ReadPhysicsPageFile());
                }
            }).Start();
        }

        public void Stop()
        {
            IsCollecting = false;
        }
    }
}
