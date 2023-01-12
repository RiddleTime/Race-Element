using ACC_Manager.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class WatkinsGlen : AbstractTrackData
    {
        public override Guid Guid => new Guid("6c4d5fe4-105d-47b0-8699-49c10a92c591");
        public override string FullName => "Watkins Glen International";
        public override int TrackLength => 5552;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
