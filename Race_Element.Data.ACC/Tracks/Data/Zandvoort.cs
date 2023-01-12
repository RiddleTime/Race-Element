using ACC_Manager.Util.DataTypes;
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

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
