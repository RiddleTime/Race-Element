using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace RaceElement.Data.ACC.Tracks.Data;

internal class Nordschleife : AbstractTrackData
{
    public override Guid Guid => new("fae08ea8-03fb-429b-be59-c56c7cd18ef1");

    public override string GameName => "nurburgring_24h";

    public override string FullName => "24h Nürburgring";

    public override int TrackLength => 25378;

    public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
    {
        { new FloatRangeStruct(0, 1), (1, "YOKOHAMA-S")}, // first right hander
        { new FloatRangeStruct(0, 2), (2, "YOKOHAMA-")}, // left hander
        { new FloatRangeStruct(0, 3), (3, "Valvoline Kurve")},  // long left hander downhill
        { new FloatRangeStruct(0, 4), (4, "PAGID Racing Curve")}, // right hander.. etc..
        { new FloatRangeStruct(0, 5), (5, "Goodyear hairpin")},
        { new FloatRangeStruct(0, 6), (6, "Michael Shumacher S")},
        { new FloatRangeStruct(0, 7), (7, "Michael Shumacher S")},
        { new FloatRangeStruct(0, 9), (8, "")},
        { new FloatRangeStruct(0, 10), (9, "")},
        { new FloatRangeStruct(0, 11), (10, "")},
        { new FloatRangeStruct(0, 12), (11, "")},
        { new FloatRangeStruct(0, 13), (12, "")},
        { new FloatRangeStruct(0, 14), (13, "")},
        { new FloatRangeStruct(0, 15), (14, "")},
        { new FloatRangeStruct(0, 16), (15, "")},
    };

    public override List<float> Sectors => [];
}
