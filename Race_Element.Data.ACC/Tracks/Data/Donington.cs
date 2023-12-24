using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Donington : AbstractTrackData
    {
        public override Guid Guid => new("e93925e9-16c8-442a-bea3-ae449d2e04be");
        public override string GameName => "donington";
        public override string FullName => "Donington Park";
        public override int TrackLength => 4020;

        public override List<float> Sectors => new() { 0.233f, 0.668f };

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
        {
            { new FloatRangeStruct(0.0772981f, 0.1404632f), (1, "Redgate") },
            { new FloatRangeStruct(0.1477933f, 0.1860932f), (2, "Hollywood") },
            { new FloatRangeStruct(0.1924699f, 0.2584147f), (3, "Craner Curves") },
            { new FloatRangeStruct(0.2683981f, 0.3242396f), (4, "Old Hairpin") },
            { new FloatRangeStruct(0.340085f, 0.3834941f), (5, "Starkey's Bridge") },
            { new FloatRangeStruct(0.3931186f, 0.4279727f), (6, "Schwantz Curve") },
            { new FloatRangeStruct(0.3931186f, 0.4847209f), (7, "McLeans") },
            { new FloatRangeStruct(0.5150666f, 0.6010549f), (8, "Coppice") },
            { new FloatRangeStruct(0.7011805f, 0.7645504f), (9, "Fogarty Esses") },
            { new FloatRangeStruct(0.8194078f, 0.8732706f), (10, "Melbourne Hairpin") },
            { new FloatRangeStruct(0.9239673f, 0.9740674f), (11, "Goddards") }
        };
    }
}
