using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Misano : AbstractTrackData
    {
        public override Guid Guid => new Guid("e8ce417b-5f5c-4921-9b6b-9367e703d3f8");
        public override string FullName => "Misano World Circuit";
        public override int TrackLength => 4226;

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.0450355f, 0.07379355f), (1, string.Empty)},
            { new FloatRangeStruct(0.07942149f, 0.1005504f), (2, string.Empty)},
            { new FloatRangeStruct(0.1065328f, 0.1500808f), (3, "Variante del Parco")},
            { new FloatRangeStruct(0.172984f, 0.2028806f), (4, "Rio")},
            { new FloatRangeStruct(0.2031436f, 0.2225098f), (5, "Rio")},
            { new FloatRangeStruct(0.2269648f, 0.2590365f), (6, string.Empty)},
            { new FloatRangeStruct(0.3045247f, 0.3432829f), (7, string.Empty)},
            { new FloatRangeStruct(0.3681819f, 0.4301231f), (8, "Quercia")},
            { new FloatRangeStruct(0.4802626f, 0.4991981f), (9, string.Empty)},
            { new FloatRangeStruct(0.5003623f, 0.5506533f), (10, "Tramonto")},
            { new FloatRangeStruct(0.6502675f, 0.686968f), (11, "Curvone")},
            { new FloatRangeStruct(0.7175767f, 0.7514548f), (12, string.Empty)},
            { new FloatRangeStruct(0.7564897f, 0.7883291f), (13, string.Empty)},
            { new FloatRangeStruct(0.7984478f, 0.8330685f), (14, "Carro")},
            { new FloatRangeStruct(0.8464203f, 0.8830997f), (15, string.Empty)},
            { new FloatRangeStruct(0.9072627f, 0.9495066f), (16, "Misano")},
        };
    }
}
