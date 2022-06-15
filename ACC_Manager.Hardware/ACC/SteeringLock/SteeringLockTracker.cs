using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Hardware.ACC.SteeringLock
{
    public class SteeringLockTracker
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


        private SteeringLockTracker()
        {

        }

    }
}
