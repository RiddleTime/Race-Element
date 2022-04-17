using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.Controls.LiveryBrowser;

namespace ACCSetupApp.Controls
{
    internal static class LiveryTagging
    {
        public class LiveryTag
        {
            public string Tag { get; set; }
            public Guid Guid { get; set; }

            public List<LiveryTreeCar> Liveries { get; set; }
        }


    }
}
