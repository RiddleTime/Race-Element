using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Indianapolis : AbstractTrackData
    {
        public override Guid Guid => new Guid("d3c246d2-edba-429e-af59-6e25357d59d4");
        public override string FullName => "Indianapolis Motor Speedway";
        public override int TrackLength => 4167;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
