using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Donington : AbstractTrackData
    {
        public override Guid Guid => new Guid("e93925e9-16c8-442a-bea3-ae449d2e04be");
        public override string FullName => "Donington Park";
        public override int TrackLength => 4020;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>()
        {
            { new FloatRangeStruct(0.0772981f, 0.1404632f), "Redgate" },
            { new FloatRangeStruct(0.1477933f, 0.1860932f), "Hollywood" },
            { new FloatRangeStruct(0.1924699f, 0.2584147f), "Craner Curves" },
            { new FloatRangeStruct(0.2683981f, 0.3242396f), "Old Hairpin" },
            { new FloatRangeStruct(0.340085f, 0.3834941f), "Starkey's Bridge" },
            { new FloatRangeStruct(0.3931186f, 0.4279727f), "Schwantz Curve" },
            { new FloatRangeStruct(0.3931186f, 0.4847209f), "McLeans" },
            { new FloatRangeStruct(0.5150666f, 0.6010549f), "Coppice" },
            { new FloatRangeStruct(0.7011805f, 0.7645504f), "Fogarty Esses" },
            { new FloatRangeStruct(0.8194078f, 0.8732706f), "Melbourne Hairpin" },
            { new FloatRangeStruct(0.9239673f, 0.9740674f), "Goddards" }
        };
    }
}
