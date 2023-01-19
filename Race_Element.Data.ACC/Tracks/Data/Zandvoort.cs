using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Zandvoort : AbstractTrackData
    {
        public override Guid Guid => new Guid("e7a091a3-b2c1-4903-8768-591a937858ea");
        public override string FullName => "Circuit Zandvoort";
        public override int TrackLength => 4252;

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.05284053f, 0.1141072f), (1, "Tarzanbocht") },
            { new FloatRangeStruct(0.1480813f, 0.1814992f), (1, "Gerlachbocht") },
            { new FloatRangeStruct(0.1887138f, 0.2235506f), (1, "Hugenholzbocht") },
            { new FloatRangeStruct(0.2407838f, 0.2725571f), (1, "Hunzerug") },
            { new FloatRangeStruct(0.3335241f, 0.3628302f), (1, "Rob Slotemakerbocht") },
            { new FloatRangeStruct(0.3733709f, 0.4406184f), (1, "Scheivlak") },
            { new FloatRangeStruct(0.4704477f, 0.5094342f), (1, "Mastersbocht") },
            { new FloatRangeStruct(0.5207254f, 0.558478f), (1, "Bocht 9") },
            { new FloatRangeStruct(0.5789623f, 0.6246402f), (1, "Bocht 10") },
            { new FloatRangeStruct(0.7076252f, 0.7710944f), (1, "Hans Ernst Bocht") },
            { new FloatRangeStruct(0.8137466f, 0.8532166f), (1, "Kumho") },
            { new FloatRangeStruct(0.85321661f, 0.9344479f), (1, "Arie Luyendijk Bocht") }
        };
    }
}
