using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class PaulRicard : AbstractTrackData
    {
        public override Guid Guid => new Guid("c46a0299-b5d0-421f-90a7-0c5223deaa63");
        public override string FullName => "Circuit Paul Ricard";
        public override int TrackLength => 5770;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
