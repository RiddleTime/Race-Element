using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCManager.Hardware.ACC.SteeringLock
{
    public class SteeringLockTracker : IDisposable
    {
        private static SteeringLockTracker _instance;
        public static SteeringLockTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new SteeringLockTracker();
                return _instance;
            }
        }

        public bool IsTracking = false;

        private string _lastCar;
        private int _lastRotation;
        private IWheelSteerLockSetter _wheel;

        private SteeringLockTracker()
        {

        }


        public void Dispose()
        {
            IsTracking = false;

            ResetRotation();
            _instance = null;
        }

        public void StartTracking()
        {
            new Thread(() =>
            {
                while (IsTracking)
                {
                    Thread.Sleep(1000);


                }

                ResetRotation();
            }).Start();

        }

        internal void DetectDevices()
        {
            DirectInput di = new DirectInput();

            foreach (DeviceInstance device in di.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {
                Debug.WriteLine("AccSteeringLock: detected: " + device.ProductGuid + " " + device.ProductName);
                _wheel = WheelSteerLock.Get(device.ProductGuid.ToString());
                if (_wheel != null)
                {
                    Debug.WriteLine("AccSteeringLock: found supported wheel: " + device.ProductName + " handled by: " + _wheel.ControllerName);
                    break;
                }
            }

            if (_wheel == null)
                Debug.WriteLine(" no supported wheel found.");
        }

        internal void ResetRotation()
        {
            if (_wheel == null) return;
            if (_lastRotation <= 0) return;

            Debug.WriteLine("AccSteeringLock: resetting rotation from: " + _lastRotation);
            if (!_wheel.Apply(_lastRotation, true, out _lastRotation))
                Debug.WriteLine("AccSteeringLock: IWheelSteerLockSetter::Apply() failed.");
            _lastRotation = 0;
        }

    }
}
