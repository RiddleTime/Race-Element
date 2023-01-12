using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Imola : AbstractTrackData
    {
        public override Guid Guid => new Guid("678eefc3-a5f0-4b2a-a1cc-03ac62650ede");
        public override string FullName => "Imola (Autodromo Internazionale Enzo e Dino Ferrari)";
        public override int TrackLength => 4959;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>();
    }
}
