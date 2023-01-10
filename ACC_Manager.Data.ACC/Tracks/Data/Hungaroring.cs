using ACC_Manager.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Hungaroring : AbstractTrackData
    {
        public override Guid Guid => new Guid("f59e6015-077b-40e0-a822-71104f253ea2");
        public override string FullName => "Hungaroring";
        public override int TrackLength => 4381;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
