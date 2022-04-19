using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ACCSetupApp.SharedMemory;

namespace ACCSetupApp.Controls
{
    internal class InputDataCollector
    {
        private bool IsCollecting = false;
        private int TraceCount = 300;

        public LinkedList<int> Throttle = new LinkedList<int>();
        public LinkedList<int> Brake = new LinkedList<int>();
        public LinkedList<int> Steering = new LinkedList<int>();

        SharedMemory sharedMemory = new SharedMemory();

        public void Collect(SPageFilePhysics filePhysics)
        {
            lock (Throttle)
            {
                Throttle.AddFirst((int)(filePhysics.Gas * 100));
                if (Throttle.Count > TraceCount)
                {
                    Throttle.RemoveLast();
                }
            }
            lock (Brake)
            {
                Brake.AddFirst((int)(filePhysics.Brake * 100));
                if (Brake.Count > TraceCount)
                {
                    Brake.RemoveLast();
                }
            }

            lock (Steering)
            {
                Steering.AddFirst((int)((filePhysics.SteerAngle + 1.0) / 2 * 100));
                if (Steering.Count > TraceCount)
                {
                    Steering.RemoveLast();
                }
            }
        }

        public void Start()
        {
            IsCollecting = true;
            new Thread(x =>
            {
                while (IsCollecting)
                {
                    Thread.Sleep(1000 / 25);
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
