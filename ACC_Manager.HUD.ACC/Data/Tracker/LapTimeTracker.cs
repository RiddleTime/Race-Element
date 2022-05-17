using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Data.Tracker
{
    internal class LapTimingData
    {
        public int Index { get; set; } = -1;
        public int Time { get; set; } = -1;
        public bool IsValid { get; set; } = true;
        public int Sector1 { get; set; } = -1;
        public int Sector2 { get; set; } = -1;
        public int Sector3 { get; set; } = -1;
    }

    internal class LapTimeTracker
    {
        private static LapTimeTracker _instance;
        public static LapTimeTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new LapTimeTracker();
                return _instance;
            }
        }

        private bool IsTracking = false;
        private ACCSharedMemory sharedMemory;
        private int CurrentSector = 0;

        internal List<LapTimingData> LapTimeDatas = new List<LapTimingData>();
        internal LapTimingData CurrentLap;

        public event EventHandler<LapTimingData> LapFinished;

        private LapTimeTracker()
        {
            sharedMemory = new ACCSharedMemory();
            CurrentLap = new LapTimingData();

            if (!IsTracking)
                this.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="sector">1 based indexing </param>
        /// <param name="lap"></param>
        /// <returns></returns>
        internal bool IsSectorFastest(int sector, int time)
        {
            List<LapTimingData> data;
            lock (Instance.LapTimeDatas)
                data = Instance.LapTimeDatas;

            if (sector == 1)
            {
                foreach (LapTimingData timing in data)
                {
                    if (timing.IsValid && timing.Sector1 < time)
                    {
                        return false;
                    }
                }
            }

            if (sector == 2)
            {
                foreach (LapTimingData timing in data)
                {
                    if (timing.IsValid && timing.Sector2 < time)
                    {
                        return false;
                    }
                }
            }

            if (sector == 3)
            {
                foreach (LapTimingData timing in data)
                {
                    if (timing.IsValid && timing.Sector3 < time)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal void Start()
        {
            if (IsTracking)
                return;

            IsTracking = true;
            new Thread(x =>
            {
                while (IsTracking)
                {
                    Thread.Sleep(1000 / 10);

                    var pageGraphics = sharedMemory.ReadGraphicsPageFile();

                    if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF)
                    {
                        LapTimeDatas.Clear();
                        CurrentLap = new LapTimingData() { Index = pageGraphics.CompletedLaps + 1 };
                        Debug.WriteLine("Cleared Lap Times and Current lap");
                    }


                    // collect sector times.
                    if (CurrentSector != pageGraphics.CurrentSectorIndex)
                    {
                        if (CurrentLap.Sector1 == -1 && CurrentSector != 0)
                        {
                            // simply don't collect, we're already into a lap and passed sector 1, can't properly calculate the sector times now.
                        }
                        else
                            switch (pageGraphics.CurrentSectorIndex)
                            {
                                case 1: CurrentLap.Sector1 = pageGraphics.LastSectorTime; break;
                                case 2: CurrentLap.Sector2 = pageGraphics.LastSectorTime - CurrentLap.Sector1; break;
                                case 0: CurrentLap.Sector3 = pageGraphics.LastTimeMs - CurrentLap.Sector2 - CurrentLap.Sector1; break;
                            }

                        CurrentSector = pageGraphics.CurrentSectorIndex;
                    }

                    // finalize lap time data and add it to history.
                    if (CurrentLap.Index - 1 != pageGraphics.CompletedLaps && pageGraphics.LastTimeMs != int.MaxValue)
                    {
                        CurrentLap.Time = pageGraphics.LastTimeMs;

                        Debug.WriteLine($"Finished lap: {CurrentLap.Index} - {CurrentLap.Time}");

                        if (CurrentLap.Sector1 != -1)
                        {
                            lock (LapTimeDatas)
                                LapTimeDatas.Add(CurrentLap);

                            LapFinished?.Invoke(this, CurrentLap);
                        }

                        CurrentLap = new LapTimingData() { Index = pageGraphics.CompletedLaps + 1 };
                    }

                    // invalidate current lap 
                    if (CurrentLap.IsValid != pageGraphics.IsValidLap)
                    {
                        Debug.WriteLine($"Invalidated lap: {CurrentLap.Index}");
                        CurrentLap.IsValid = pageGraphics.IsValidLap;
                    }


                }
            }).Start();

            _instance = null;
        }

        internal void Stop()
        {
            IsTracking = false;
        }
    }


}
