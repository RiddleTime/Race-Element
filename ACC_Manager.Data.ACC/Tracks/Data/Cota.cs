using ACC_Manager.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Cota : AbstractTrackData
    {
        public override Guid Guid => new Guid("f45eac53-7a77-4fe5-812f-064b30ac22df");
        public override string FullName => "Circuit of the Americas";
        public override int TrackLength => 5513;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
