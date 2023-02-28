using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Monza : AbstractTrackData
    {
        public override Guid Guid => new Guid("5091ac88-c7c3-4cf1-ac46-e974bc7b73d5");
        public override string FullName => "Monza Circuit";
        public override int TrackLength => 5793;

        // https://upload.wikimedia.org/wikipedia/commons/f/f8/Monza_track_map.svg
        // https://static.wikia.nocookie.net/f1wikia/images/c/cc/Monza2000.png/revision/latest?cb=20120502050754
        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>();
    }
}
