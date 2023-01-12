using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Donington : AbstractTrackData
    {
        public override Guid Guid => new Guid("e93925e9-16c8-442a-bea3-ae449d2e04be");
        public override string FullName => "Donington Park";
        public override int TrackLength => 4020;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
