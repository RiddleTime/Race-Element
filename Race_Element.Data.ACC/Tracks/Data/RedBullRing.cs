using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace RaceElement.Data.ACC.Tracks.Data;
internal sealed class RedBullRing : AbstractTrackData
{
    public override Guid Guid => new("f40d09f5-d548-4206-978c-61275840e808");
    public override string GameName => "red_bull_ring";
    public override string FullName => "Red Bull Ring";
    public override int TrackLength => 4318;

    public override float FactorScale => 0.29f;
    public override float PitLaneTime => 20f;

    public override List<float> Sectors => [0.28641132f, 0.6864937f];

    public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
    {
        { new FloatRangeStruct(0.07443155f, 0.13285546f), (1, "Niki Lauda")},
        { new FloatRangeStruct(0.19579221f, 0.26379347f), (2, "Münzer")},
        { new FloatRangeStruct(0.28775895f, 0.3495821f), (3, "")},
        { new FloatRangeStruct(0.473018f, 0.5404008f), (4, "Rauch")},
        { new FloatRangeStruct(0.5430957f, 0.5846855f), (5, "")},
        { new FloatRangeStruct(0.5958378f, 0.6766595f), (6, "")},
        { new FloatRangeStruct(0.6804476f, 0.72022116f), (7, "Graz")},
        { new FloatRangeStruct(0.72022117f, 0.78323466f), (8, "")},
        { new FloatRangeStruct(0.8480688f, 0.8989858f), (9, "Jochen Rindt")},
        { new FloatRangeStruct(0.90011495f, 0.9538002f), (10, "")},
    };
}
