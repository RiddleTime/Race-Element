using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class LagunaSeca : AbstractTrackData
    {
        public override Guid Guid => new Guid("0c8d198c-608f-4beb-96f5-5eafb5d3ba6b");
        public override string FullName => "Weathertech Raceway Laguna Seca";
        public override int TrackLength => 3602;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
