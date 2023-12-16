using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class WatkinsGlen : AbstractTrackData
    {
        public override Guid Guid => new Guid("6c4d5fe4-105d-47b0-8699-49c10a92c591");
        public override string GameName => "watkins_glen";
        public override string FullName => "Watkins Glen International";
        public override int TrackLength => 5552;

        public override List<float> Sectors => new List<float>() { 0.3185f, 0.632f };

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.03883164f, 0.08408849f), (1, "The 90") },
            { new FloatRangeStruct(0.1122197f, 0.1562921f), (2, "The Esses") },
            { new FloatRangeStruct(0.1569853f, 0.1971038f), (3, "The Esses") },
            { new FloatRangeStruct(0.1981151f, 0.2610424f), (4, "The Esses") },
            { new FloatRangeStruct(0.3331092f, 0.355317f), (5, "Inner Loop") },
            { new FloatRangeStruct(0.3558367f, 0.3658298f), (6, "Inner Loop") },
            { new FloatRangeStruct(0.367707f, 0.3769202f), (7, "Inner Loop") },
            { new FloatRangeStruct(0.3781044f, 0.395433f), (8, "Inner Loop") },
            { new FloatRangeStruct(0.3954331f, 0.4644593f), (9, "Outer Loop") },
            { new FloatRangeStruct(0.4897895f, 0.5415458f), (10, "Chute") },
            { new FloatRangeStruct(0.583251f, 0.6384665f), (11, "Toe") },
            { new FloatRangeStruct(0.7093781f, 0.757032f), (12, "Heel") },
            { new FloatRangeStruct(0.776354f, 0.8196481f), (13, string.Empty) },
            { new FloatRangeStruct(0.8520821f, 0.889021f), (14, string.Empty) },
            { new FloatRangeStruct(0.9098449f, 0.9501061f), (15, string.Empty) }
        };
    }
}
