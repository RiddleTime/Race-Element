using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class WatkinsGlen : AbstractTrackData
    {
        public override Guid Guid => new Guid("6c4d5fe4-105d-47b0-8699-49c10a92c591");
        public override string FullName => "Watkins Glen International";
        public override int TrackLength => 5552;

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.03883164f, 0.08408849f), (1, "The 90") },
            { new FloatRangeStruct(0.1122197f, 0.2610424f), (1, "The Esses") },
            { new FloatRangeStruct(0.3331092f, 0.395433f), (1, "Inner Loop") },
            { new FloatRangeStruct(0.3954331f, 0.4644593f), (1, "Outer Loop") },
            { new FloatRangeStruct(0.4897895f, 0.5415458f), (1, "Chute") },
            { new FloatRangeStruct(0.583251f, 0.6384665f), (1, "Toe") },
            { new FloatRangeStruct(0.7093781f, 0.757032f), (1, "Heel") },
            { new FloatRangeStruct(0.776354f, 0.8196481f), (1, "Turn 9") },
            { new FloatRangeStruct(0.8520821f, 0.889021f), (1, "Turn 10") },
            { new FloatRangeStruct(0.9098449f, 0.9501061f), (1, "Turn 11") }
        };
    }
}
