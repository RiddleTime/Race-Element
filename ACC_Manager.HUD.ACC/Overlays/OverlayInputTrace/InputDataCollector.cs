using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System.Collections.Generic;
using System.Threading;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.ACC.Overlays.OverlayInputTrace
{
    internal class InputDataCollector
    {
        private bool IsCollecting = false;
        public int TraceCount = 300;
        public InputTraceOverlay.InputTraceConfig inputTraceConfig;

        public LinkedList<int> Throttle = new LinkedList<int>();
        public LinkedList<int> Brake = new LinkedList<int>();
        public LinkedList<int> Steering = new LinkedList<int>();

        private AbstractOverlay _overlay;

        public InputDataCollector(AbstractOverlay overlay)
        {
            _overlay = overlay;
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

        public void Start()
        {
            IsCollecting = true;

            for (int i = 0; i < TraceCount; i++)
            {
                Throttle.AddLast(0);
                Brake.AddLast(0);
                Steering.AddLast(50);
            }

            new Thread(x =>
            {
                while (IsCollecting)
                {
                    Thread.Sleep(1000 / inputTraceConfig.InfoPanel.Herz);
                    if (_overlay != null && _overlay.pagePhysics != null)
                    {
                        Collect(_overlay.pagePhysics);
                        _overlay.RequestRedraw();
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
