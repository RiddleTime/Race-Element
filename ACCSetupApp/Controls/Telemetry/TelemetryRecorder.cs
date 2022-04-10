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


        private SharedMemory Memory;
        private TelemetryStorage Storage;

        public TelemetryRecorder(ref TelemetryStorage storage, int intervalMillis = 50)
        {
            IntervalMillis = intervalMillis;
            Memory = new SharedMemory();
            Storage = storage;
        }

        public void Record()
        {
            _isRunning = true;
            ThreadPool.QueueUserWorkItem(x =>
            {
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

                        Dictionary<string, object> data = new Dictionary<string, object>()
                        {
                            {"Speed", physicsPage.SpeedKmh },
                            {"Gear", physicsPage.Gear - 1 },
                            {"RPM", physicsPage.Rpms},
                            {"Fuel",physicsPage.Fuel },
                            {"Pressures", physicsPage.WheelPressure },
                            { "SteerAngle",physicsPage.SteerAngle },
                            {"TyreTemp.Core",physicsPage.TyreCoreTemperature }
                        };

                        Storage.AddDataEntry(ticks, data);
                    });

                }
            });
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
}
