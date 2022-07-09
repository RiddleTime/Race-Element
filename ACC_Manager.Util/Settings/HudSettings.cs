using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACC_Manager.Util.Settings
{
    public class HudSettings
    {
        private static bool _demoMode = false;
        public static bool DemoMode
        {
            get { return _demoMode; }
            set { _demoMode = value; }
        }
    }
}
