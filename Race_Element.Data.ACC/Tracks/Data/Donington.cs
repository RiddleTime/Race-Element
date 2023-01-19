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

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.0772981f, 0.1404632f), (1,"Redgate") },
            { new FloatRangeStruct(0.1477933f, 0.1860932f), (1, "Hollywood") },
            { new FloatRangeStruct(0.1924699f, 0.2584147f), (1, "Craner Curves") },
            { new FloatRangeStruct(0.2683981f, 0.3242396f), (1, "Old Hairpin") },
            { new FloatRangeStruct(0.340085f, 0.3834941f), (1, "Starkey's Bridge") },
            { new FloatRangeStruct(0.3931186f, 0.4279727f), (1, "Schwantz Curve") },
            { new FloatRangeStruct(0.3931186f, 0.4847209f), (1, "McLeans") },
            { new FloatRangeStruct(0.5150666f, 0.6010549f), (1, "Coppice") },
            { new FloatRangeStruct(0.7011805f, 0.7645504f), (1, "Fogarty Esses") },
            { new FloatRangeStruct(0.8194078f, 0.8732706f), (1, "Melbourne Hairpin") },
            { new FloatRangeStruct(0.9239673f, 0.9740674f), (1, "Goddards") }
        };
    }
}
