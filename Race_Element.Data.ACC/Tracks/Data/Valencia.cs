using System;
using System.Collections.Generic;

using RaceElement.Util.DataTypes;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data;

internal class Valencia : AbstractTrackData
{
    public override Guid Guid => new("51bcd9f5-5048-4f98-aff6-ec853e77de5a");
    public override string GameName => "Valencia";
    public override string FullName => "Circuit Ricardo Tormo Valencia";
    public override int TrackLength => 4005;

    public override List<float> Sectors => new() { 0.3785f, 0.7245f };

    public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
    {
       { new FloatRangeStruct(0.0993231f, 0.1736007f), (1, "Jorge Martinez Aspar")},
       { new FloatRangeStruct(0.2048207f, 0.2503794f), (2, "Mick Doohan")},
       { new FloatRangeStruct(0.2570817f, 0.2895989f), (3, "")},
       { new FloatRangeStruct(0.2957831f, 0.330566f), (4, "Nico Terol")},
       { new FloatRangeStruct(0.339013f, 0.3883471f), (5, "")},
       { new FloatRangeStruct(0.4048822f, 0.456717f), (6, "Ángel Nieto")},
       { new FloatRangeStruct(0.4964134f, 0.5328197f), (7, "Curva de la Afición")},
       { new FloatRangeStruct(0.538891f, 0.5952407f), (8, "")},
       { new FloatRangeStruct(0.6008725f, 0.6318818f), (9, "")},
       { new FloatRangeStruct(0.6360093f, 0.6555992f), (10, "")},
       { new FloatRangeStruct(0.6555993f, 0.7019168f), (11, "")},
       { new FloatRangeStruct(0.7489494f, 0.7853982f), (12, "Champi Herreros")},
       { new FloatRangeStruct(0.7853983f, 0.8632079f), (13, "")},
       { new FloatRangeStruct(0.8632080f, 0.9249977f), (14, "Adrián Campos")},
    };
}