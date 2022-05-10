using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayLapDelta
{
    internal class LapTimeCollector
    {
        private bool IsCollecting = false;

        private Dictionary<int, float> LapTimes { get; set; }
        private Dictionary<int, float> Sectors1 { get; set; }
        private Dictionary<int, float> Sectors2 { get; set; }
        private Dictionary<int, float> Sectors3 { get; set; }

        private ACCSharedMemory sharedMemory;

        private int CurrentLap = 0;
        private int CurrentSector = 0;

        internal LapTimeCollector()
        {
            sharedMemory = new ACCSharedMemory();
            LapTimes = new Dictionary<int, float>();
            Sectors1 = new Dictionary<int, float>();
            Sectors2 = new Dictionary<int, float>();
            Sectors3 = new Dictionary<int, float>();
        }

        internal void Start()
        {
            new Thread(x =>
            {
                while (IsCollecting)
                {
                    Thread.Sleep(1000 / 10);

                    var pageGraphics = sharedMemory.ReadGraphicsPageFile();
                    if (CurrentLap != pageGraphics.CompletedLaps)
                    {
                        LapTimes.Add(CurrentLap, pageGraphics.LastTimeMs / 1000);
                        CurrentLap = pageGraphics.CompletedLaps;
                    }

                    if(CurrentSector != pageGraphics.CurrentSectorIndex)
                    {

                    }


                    //Collect(sharedMemory.ReadPhysicsPageFile());
                }
            }).Start();
        }

        internal void Stop()
        {
            IsCollecting = false;
        }
    }
}
