using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Hardware.ACC.SteeringLock
{
    /// <summary>
    /// From https://github.com/Havner/acc-steering-lock
    /// </summary>
    internal static class WheelSteerLock
    {
        private static IWheelSteerLockSetter[] _instances;

        private static void Initialize()
        {
            if (_instances == null)
            {
                _instances = Assembly.GetExecutingAssembly().GetTypes()
                                     .Where(x => !x.IsAbstract && x.GetInterfaces().Contains(typeof(IWheelSteerLockSetter)))
                                     .Select(x => (IWheelSteerLockSetter)Activator.CreateInstance(x)).ToArray();
            }
        }

        public static IWheelSteerLockSetter Get(string productGuid)
        {
            Initialize();
            return _instances.FirstOrDefault(x => x.Test(productGuid));
        }

        public static bool IsSupported(string productGuid)
        {
            Initialize();
            return _instances.Any(x => x.Test(productGuid));
        }

        public static IEnumerable<string> GetSupportedNames()
        {
            Initialize();
            return _instances.Select(x => x.ControllerName);
        }
    }
}
