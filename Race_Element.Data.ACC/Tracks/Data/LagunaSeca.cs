using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data;

internal sealed class LagunaSeca : AbstractTrackData
{
    public override Guid Guid => new("0c8d198c-608f-4beb-96f5-5eafb5d3ba6b");
    public override string GameName => "Laguna_Seca";
    public override string FullName => "WeatherTech Raceway Laguna Seca";
    public override int TrackLength => 3602;

    public override List<float> Sectors => new() { 0.25f, 0.63f };

    public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
    {
        { new FloatRangeStruct(0.00000001f, 0.05178322f), (1, string.Empty)},
        { new FloatRangeStruct(0.09772267f, 0.1708893f), (2, "Andretti Hairpin")},
        { new FloatRangeStruct(0.1968708f, 0.2429551f), (3, string.Empty)},
        { new FloatRangeStruct(0.2695629f, 0.3257776f), (4, string.Empty)},
        { new FloatRangeStruct(0.399998f, 0.4614799f), (5, string.Empty)},
        { new FloatRangeStruct(0.5247761f, 0.5660338f), (6, string.Empty)},
        { new FloatRangeStruct(0.6468806f, 0.6686942f), (7, string.Empty)},
        { new FloatRangeStruct(0.6687181f, 0.6897181f), (8, "The Corkscrew")},
        { new FloatRangeStruct(0.6899807f, 0.7132857f), (9, "The Corkscrew")},
        { new FloatRangeStruct(0.730754f, 0.7861f), (10, "Rainey Curve")},
        { new FloatRangeStruct(0.8088823f, 0.8508828f), (11, string.Empty)},
        { new FloatRangeStruct(0.892848f, 0.934315f), (12, string.Empty)},
    };
}
