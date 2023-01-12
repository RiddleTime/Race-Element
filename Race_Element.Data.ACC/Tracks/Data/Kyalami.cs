using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Kyalami : AbstractTrackData
    {
        public override Guid Guid => new Guid("65e00cd4-6c39-4cb4-acf9-f8977cd56ba1");
        public override string FullName => "Kyalami Grand Prix Circuit";
        public override int TrackLength => 4522;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
