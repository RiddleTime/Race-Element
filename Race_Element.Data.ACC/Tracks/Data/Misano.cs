using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Misano : AbstractTrackData
    {
        public override Guid Guid => new Guid("e8ce417b-5f5c-4921-9b6b-9367e703d3f8");
        public override string FullName => "Misano World Circuit";
        public override int TrackLength => 4226;

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>();
    }
}
