using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Nurburgring : AbstractTrackData
    {
        public override Guid Guid => new Guid("20200ee1-89c1-4580-86f1-3ded3018e9e3");
        public override string FullName => "Nürburgring";
        public override int TrackLength => 5137;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
