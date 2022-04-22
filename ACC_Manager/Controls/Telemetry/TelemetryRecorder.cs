using ACCSetupApp.Controls.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls
{
    internal class TelemetryRecorder
    {
        private int IntervalMillis;
        private bool _isRunning = false;


        private ACCSharedMemory Memory;
        private TelemetryStorage Storage;

        public TelemetryRecorder(ref TelemetryStorage storage, int intervalMillis = 50)
        {
            IntervalMillis = intervalMillis;
            Memory = new ACCSharedMemory();
            Storage = storage;
        }

        public void Record()
        {
            _isRunning = true;
            ThreadPool.QueueUserWorkItem(x =>
            {
                Storage.Clear();
                var interval = new TimeSpan(0, 0, 0, 0, IntervalMillis);
                var nextTick = DateTime.Now + interval;
                while (_isRunning)
                {
                    while (DateTime.Now < nextTick)
                    {
                        Thread.Sleep(nextTick - DateTime.Now);
                    }
                    nextTick += interval;

                    long ticks = DateTime.Now.Ticks;
                    ThreadPool.QueueUserWorkItem(y =>
                    {
                        var physicsPage = Memory.ReadPhysicsPageFile();
                        var graphicsPage = Memory.ReadGraphicsPageFile();


                        Dictionary<string, object> data = new Dictionary<string, object>()
                        {
                            {"Speed", physicsPage.SpeedKmh },
                            {"Throttle", physicsPage.Gas},
                            {"Brake", physicsPage.Brake },
                            {"Lap", graphicsPage.CompletedLaps+1}
                        };

                        Storage.AddDataEntry(ticks, data);
                    });

                }
            });
        }

        public void Stop()
        {
            _isRunning = false;
            Thread.Sleep(IntervalMillis + 50);
        }
    }
}
