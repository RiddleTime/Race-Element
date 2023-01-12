using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Suzuka : AbstractTrackData
    {
        public override Guid Guid => new Guid("1c9b35a5-dacb-40e2-adc9-134d75f75c3f");
        public override string FullName => "Suzuka Circuit";
        public override int TrackLength => 5807;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
