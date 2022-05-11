using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayLapDelta
{
    internal class LapTimeCollector
    {
        private bool IsCollecting = false;

        public Dictionary<int, bool> LapValids { get; set; }
        // <LapIndex, Time> (divide time by 1000)
        public Dictionary<int, int> LapTimes { get; set; }
        public Dictionary<int, int> Sectors1 { get; set; }
        public Dictionary<int, int> Sectors2 { get; set; }
        public Dictionary<int, int> Sectors3 { get; set; }

        private ACCSharedMemory sharedMemory;

        private bool CurrentValid = true;
        private int CurrentLap = 0;
        private int CurrentSector = 0;

        internal LapTimeCollector()
        {
            sharedMemory = new ACCSharedMemory();
            LapTimes = new Dictionary<int, int>();
            Sectors1 = new Dictionary<int, int>();
            Sectors2 = new Dictionary<int, int>();
            Sectors3 = new Dictionary<int, int>();

            var pageGraphics = sharedMemory.ReadGraphicsPageFile();

            CurrentLap = pageGraphics.CompletedLaps;
            CurrentSector = pageGraphics.CurrentSectorIndex;
        }

        internal void Start()
        {
            IsCollecting = true;
            new Thread(x =>
            {
                while (IsCollecting)
                {
                    Thread.Sleep(1000 / 10);

                    var pageGraphics = sharedMemory.ReadGraphicsPageFile();

                    if (CurrentValid != pageGraphics.IsValidLap)
                    {
                        CurrentValid = false;
                    }

                    if (CurrentSector != pageGraphics.CurrentSectorIndex)
                    {
                        if (Sectors1.Count == 0 && CurrentSector != 0)
                        {
                            Debug.WriteLine($"Not sector 1 {CurrentSector}");
                        }
                        else
                            switch (pageGraphics.CurrentSectorIndex)
                            {
                                case 1: Sectors1.Add(CurrentLap, pageGraphics.LastSectorTime); break;
                                case 2: Sectors2.Add(CurrentLap, pageGraphics.LastSectorTime - Sectors1[CurrentLap]); break;
                                case 0: Sectors3.Add(CurrentLap, pageGraphics.LastTimeMs - Sectors2[CurrentLap] - Sectors1[CurrentLap]); break;
                            }

                        CurrentSector = pageGraphics.CurrentSectorIndex;
                        Debug.WriteLine("collected sector time");
                    }

                    if (CurrentLap != pageGraphics.CompletedLaps)
                    {
                        LapTimes.Add(CurrentLap, pageGraphics.LastTimeMs);
                        LapValids.Add(CurrentLap, CurrentValid);
                        CurrentLap = pageGraphics.CompletedLaps;
                        CurrentValid = true;
                    }
                }
            }).Start();
        }

        internal void Stop()
        {
            IsCollecting = false;
        }
    }
}
