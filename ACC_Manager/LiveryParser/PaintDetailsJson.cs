using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.LiveryParser
{
    // (Sponsors.json + Decals.json)
    public class PaintDetailsJson
    {
        public class Root
        {
            public int baseRoughness { get; set; }
            public int clearCoat { get; set; }
            public int clearCoatRoughness { get; set; }
            public int metallic { get; set; }
        }
    }
}
