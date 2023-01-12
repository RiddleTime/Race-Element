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

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>()
        {
            { new FloatRangeStruct(0.05284053f, 0.1141072f), "Tarzanbocht" },
            { new FloatRangeStruct(0.1480813f, 0.1814992f), "Gerlachbocht" },
            { new FloatRangeStruct(0.1887138f, 0.2235506f), "Hugenholzbocht" },
            { new FloatRangeStruct(0.2407838f, 0.2725571f), "Hunzerug" },
            { new FloatRangeStruct(0.3335241f, 0.3628302f), "Rob Slotemakerbocht" },
            { new FloatRangeStruct(0.3733709f, 0.4406184f), "Scheivlak" },
            { new FloatRangeStruct(0.4704477f, 0.5094342f), "Mastersbocht" },
            { new FloatRangeStruct(0.5207254f, 0.558478f), "Bocht 9" },
            { new FloatRangeStruct(0.5789623f, 0.6246402f), "Bocht 10" },
            { new FloatRangeStruct(0.7076252f, 0.7710944f), "Hans Ernst Bocht" },
            { new FloatRangeStruct(0.8137466f, 0.8532166f), "Kumho" },
            { new FloatRangeStruct(0.85321661f, 0.9344479f), "Arie Luyendijk Bocht" }
        };
    }
}
