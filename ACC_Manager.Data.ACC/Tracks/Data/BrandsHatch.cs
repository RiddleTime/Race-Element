using ACC_Manager.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class BrandsHatch : AbstractTrackData
    {
        public override Guid Guid => new Guid("7827823f-6df1-4295-bc24-93bc83d71855");
        public override string FullName => "Brands Hatch Circuit";
        public override int TrackLength => 3908;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
