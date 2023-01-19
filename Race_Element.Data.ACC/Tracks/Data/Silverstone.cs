using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Silverstone : AbstractTrackData
    {
        public override Guid Guid => new Guid("8636837e-e916-4d4b-8f29-625cf6bf4695");
        public override string FullName => "Silverstone";
        public override int TrackLength => 5891;

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>();
    }
}
