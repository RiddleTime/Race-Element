using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data;

internal sealed class Cota : AbstractTrackData
{
    public override Guid Guid => new("f45eac53-7a77-4fe5-812f-064b30ac22df");
    public override string GameName => "cota";
    public override string FullName => "Circuit of the Americas";
    public override int TrackLength => 5513;

    public override float FactorScale => 0.2f;
    public override float PitLaneTime => 29f;

    public override List<float> Sectors => new() { 0.236f, 0.647f };

    public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
    {
        { new FloatRangeStruct(0.09711581f, 0.1336419f), (1, string.Empty) },
        { new FloatRangeStruct(0.139182f, 0.1873701f), (2, string.Empty) },
        { new FloatRangeStruct(0.1990563f, 0.2206976f), (3, string.Empty) },
        { new FloatRangeStruct(0.2225156f, 0.2368565f), (4, string.Empty) },
        { new FloatRangeStruct(0.2387608f, 0.2539387f), (5, string.Empty) },
        { new FloatRangeStruct(0.2557565f, 0.2953745f), (6, string.Empty) },
        { new FloatRangeStruct(0.2992989f, 0.3273459f), (7, string.Empty) },
        { new FloatRangeStruct(0.3315877f, 0.3512957f), (8, string.Empty) },
        { new FloatRangeStruct(0.3534598f, 0.3714648f), (9, string.Empty) },
        { new FloatRangeStruct(0.3827475f, 0.4132474f), (10, string.Empty) },
        { new FloatRangeStruct(0.4447892f, 0.4825258f), (11, string.Empty) },
        { new FloatRangeStruct(0.6667117f, 0.6991135f), (12, string.Empty) },
        { new FloatRangeStruct(0.7148103f, 0.7378947f), (13, string.Empty) },
        { new FloatRangeStruct(0.7392216f, 0.7586122f), (14, string.Empty) },
        { new FloatRangeStruct(0.7662588f, 0.7915642f), (15, string.Empty) },
        { new FloatRangeStruct(0.8056163f, 0.8274886f), (16, string.Empty) },
        { new FloatRangeStruct(0.8287005f, 0.8437918f), (17, string.Empty) },
        { new FloatRangeStruct(0.8478028f, 0.88618f), (18, string.Empty) },
        { new FloatRangeStruct(0.8997419f, 0.9391019f), (19, string.Empty) },
        { new FloatRangeStruct(0.9560677f, 0.9854119f), (20, string.Empty) }
    };
}
