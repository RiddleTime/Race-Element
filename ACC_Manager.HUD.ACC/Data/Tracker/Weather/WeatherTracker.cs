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
        public long TimeStamp = -1;
        public AcRainIntensity Now { get; set; }
        public AcRainIntensity In10 { get; set; }
        public AcRainIntensity In30 { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            WeatherInfo other = obj as WeatherInfo;
            return this.Now == other.Now && this.In10 == other.In10 && this.In30 == other.In30;
        }
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

        public event EventHandler<WeatherInfo> OnWeatherChanged;

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


                        var newWeather = new WeatherInfo()
                        {
                            Now = graphicsPage.rainIntensity,
                            In10 = graphicsPage.rainIntensityIn10min,
                            In30 = graphicsPage.rainIntensityIn30min,
                            TimeStamp = DateTime.Now.ToFileTimeUtc()
                        };

                        if (!newWeather.Equals(Weather))
                        {
                            OnWeatherChanged.Invoke(this, newWeather);
                        }

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
