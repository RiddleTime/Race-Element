using ACCManager.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.ACC.Data.Tracker.Weather
{
    public class WeatherInfo
    {
        public AcRainIntensity Now { get; set; }
        public AcRainIntensity In10 { get; set; }
        public AcRainIntensity In30 { get; set; }
    }

    internal class WeatherTracker
    {
        private static WeatherTracker _instance;
        public static WeatherTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new WeatherTracker();
                return _instance;
            }
        }

        private bool IsTracking = false;
        private ACCSharedMemory sharedMemory;

        public WeatherInfo Weather;

        public WeatherTracker()
        {
            sharedMemory = new ACCSharedMemory();
            Weather = new WeatherInfo();

            Start();
        }

        private void Start()
        {
            if (IsTracking)
                return;

            IsTracking = true;
            new Thread(x =>
            {
                while (IsTracking)
                {
                    try
                    {
                        Thread.Sleep(1000 / 10);

                        var graphicsPage = sharedMemory.ReadGraphicsPageFile();


                        Weather = new WeatherInfo()
                        {
                            Now = graphicsPage.rainIntensity,
                            In10 = graphicsPage.rainIntensityIn10min,
                            In30 = graphicsPage.rainIntensityIn30min,
                        };
                    }
                    catch (Exception ex)
                    {
                        LogWriter.WriteToLog(ex);
                    }
                }

                _instance = null;
                IsTracking = false;
            }).Start();
        }

        internal void Stop()
        {
            IsTracking = false;
        }
    }
}
