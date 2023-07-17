using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Silverstone : AbstractTrackData
    {
        public override Guid Guid => new Guid("8636837e-e916-4d4b-8f29-625cf6bf4695");
        public override string GameName => "Silverstone";
        public override string FullName => "Silverstone";
        public override int TrackLength => 5891;

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0, 0.02048442f), (-1, "National Pits Straight")},
            { new FloatRangeStruct(0.02117836f, 0.07587882f), (1, "Copse")},
            { new FloatRangeStruct(0.1129919f, 0.1418478f), (2, "Maggots")},
            { new FloatRangeStruct(0.1418479f, 0.1576069f), (3, "Maggots")},
            { new FloatRangeStruct(0.1576070f, 0.1858984f), (4, "Becketts")},
            { new FloatRangeStruct(0.1858985f, 0.2108015f), (5, "Becketts")},
            { new FloatRangeStruct(0.2108016f, 0.2444144f), (6, "Chapel")},
            { new FloatRangeStruct(0.2493648f, 0.335451f), (-1, "Hanger Straight")},
            { new FloatRangeStruct(0.3455879f, 0.4131711f), (7, "Stowe")},
            { new FloatRangeStruct(0.437268f, 0.4609339f), (8, "Vale")},
            { new FloatRangeStruct(0.4609340f, 0.4797319f), (9, "Vale")},
            { new FloatRangeStruct(0.4833086f, 0.5199639f), (10, "Club")},
            { new FloatRangeStruct(0.5213624f, 0.5589856f), (-1, "Hamilton Straight")},
            { new FloatRangeStruct(0.5623456f, 0.6019608f), (11, "Abbey")},
            { new FloatRangeStruct(0.6019609f, 0.6471949f), (12, "Farm Curve")},
            { new FloatRangeStruct(0.6521705f, 0.6794665f), (13, "Village")},
            { new FloatRangeStruct(0.6846299f, 0.7075961f), (14, "The Loop")},
            { new FloatRangeStruct(0.7130286f, 0.7599301f), (15, "Alntree")},
            { new FloatRangeStruct(0.7664113f, 0.8201703f), (-1, "Wellington Straight")},
            { new FloatRangeStruct(0.8263848f, 0.8694381f), (16, "Brooklands")},
            { new FloatRangeStruct(0.8694382f, 0.9201857f), (17, "Luffield ")},
            { new FloatRangeStruct(0.9201858f, 0.971256f), (18, "Woodcote")},
        };
    }
}
