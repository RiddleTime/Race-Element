using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data;

internal sealed class Snetterton : AbstractTrackData
{
    public override Guid Guid => new("9248d360-e1ba-45be-bdec-dc939fb3959b");
    public override string GameName => "snetterton";
    public override string FullName => "Snetterton Circuit";
    public override int TrackLength => 4779;

    public override List<float> Sectors => new() { 0.321f, 0.678f };

    public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
    {
          { new FloatRangeStruct(0.05055097f, 0.1186151f), (1, "Riches")},
          { new FloatRangeStruct(0.1454172f, 0.1875129f), (2, "Montreal")},
          { new FloatRangeStruct(0.2215991f, 0.2735485f ), (3, "Palmer")},
          { new FloatRangeStruct(0.329199f, 0.3779144f), (4, "Agostini")},
          { new FloatRangeStruct(0.4053102f, 0.4402506f), (5, "Hamilton")},
          { new FloatRangeStruct(0.454165f, 0.4927266f), (6, "Oggies")},
          { new FloatRangeStruct(0.5022557f, 0.5485336f), (7, "Williams")},
          { new FloatRangeStruct(0.6869881f, 0.7165877f), (8, "Brundle")},
          { new FloatRangeStruct(0.7165878f, 0.739463f), (9, "Nelson")},
          { new FloatRangeStruct(0.757171f, 0.801405f), (10, "Bomb Hole")},
          { new FloatRangeStruct(0.8077023f, 0.884012f), (11, "Coram")},
          { new FloatRangeStruct(0.884013f, 0.9138725f), (12, "Murrays")},
    };
}
