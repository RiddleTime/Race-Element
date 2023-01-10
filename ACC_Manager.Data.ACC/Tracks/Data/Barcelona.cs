using ACC_Manager.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Barcelona : AbstractTrackData
    {
        public override Guid Guid => new Guid("c47d348d-4cac-4377-90ff-be3613bc6519");
        public override string FullName => "Circuit de Barcelona-Catalunya";
        public override int TrackLength => 4655;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
