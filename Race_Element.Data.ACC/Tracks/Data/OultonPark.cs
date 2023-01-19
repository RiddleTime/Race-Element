using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class OultonPark : AbstractTrackData
    {
        public override Guid Guid => new Guid("72794bc2-841c-40e1-8587-3e41f9228ea8");
        public override string FullName => "Oulton Park";
        public override int TrackLength => 4307;

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>();
    }
}
