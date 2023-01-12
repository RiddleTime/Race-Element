using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class MountPanorama : AbstractTrackData
    {
        public override Guid Guid => new Guid("c054299c-0e16-4094-8e76-a9a4da399268");
        public override string FullName => "Mount Panorama Circuit";
        public override int TrackLength => 6213;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
