using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace RaceElement.Data.ACC.Tracks.Data;

internal class Nordschleife : AbstractTrackData
{
    public override Guid Guid => new("fae08ea8-03fb-429b-be59-c56c7cd18ef1");

    public override string GameName => "nurburgring_24h";

    public override string FullName => "Nurburgring 24h";

    public override int TrackLength => 25378;

    public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => [];

    public override List<float> Sectors => [];
}
