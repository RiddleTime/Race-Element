using ACC_Manager.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Zolder : AbstractTrackData
    {
        public override Guid Guid => new Guid("eaca1a4d-aa7e-4c31-bfc5-6035bfa30395");
        public override string FullName => "Circuit Zolder";
        public override int TrackLength => 4011;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
