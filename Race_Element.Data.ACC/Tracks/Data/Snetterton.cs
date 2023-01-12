using ACC_Manager.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Snetterton : AbstractTrackData
    {
        public override Guid Guid => new Guid("9248d360-e1ba-45be-bdec-dc939fb3959b");
        public override string FullName => "Snetterton Circuit";
        public override int TrackLength => 4779;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
