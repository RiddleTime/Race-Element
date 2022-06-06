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

        private bool _isTracking = false;
        private ACCSharedMemory sharedMemory;

        private WeatherInfo _lastWeather;

        public event EventHandler<WeatherInfo> OnWeatherChanged;

        private WeatherTracker()
        {
            sharedMemory = new ACCSharedMemory();
            _lastWeather = new WeatherInfo();

            Start();
        }

        private void Start()
        {
            if (_isTracking)
                return;

            _isTracking = true;
            new Thread(x =>
            {
                while (_isTracking)
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

                        if (!newWeather.Equals(_lastWeather))
                        {
                            OnWeatherChanged?.Invoke(this, newWeather);
                            _lastWeather = newWeather;
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        LogWriter.WriteToLog(ex);
                    }
                }

                _instance = null;
                _isTracking = false;
            }).Start();
        }

        internal void Stop()
        {
            _isTracking = false;
        }
    }
}
