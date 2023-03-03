using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Silverstone : AbstractTrackData
    {
        public override Guid Guid => new Guid("8636837e-e916-4d4b-8f29-625cf6bf4695");
        public override string FullName => "Silverstone";
        public override int TrackLength => 5891;

        // https://www.gt86.org.uk/forums/uploads/monthly_2018_06/silverstone_circuit_map.png.20b9f128d327a818cd3ff8be872270ce.png
        // https://pbs.twimg.com/media/Dfa74LlW0AAcHU_.jpg
        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0, 0), (1, "Abbey")},
            { new FloatRangeStruct(0, 0), (2, "Farm Curve")},
            { new FloatRangeStruct(0, 0), (3, "Village")},
            { new FloatRangeStruct(0, 0), (4, "The Loop")},
            { new FloatRangeStruct(0, 0), (5, "Alntree")},
            { new FloatRangeStruct(0, 0), (6, "Brooklands")},
            { new FloatRangeStruct(0, 0), (7, "Luffield ")},
            { new FloatRangeStruct(0, 0), (8, "Woodcote")},
            { new FloatRangeStruct(0, 0), (9, "Copse")},
            { new FloatRangeStruct(0, 0), (10, "Maggots")},
            { new FloatRangeStruct(0, 0), (11, "Maggots")},
            { new FloatRangeStruct(0, 0), (12, "Becketts")},
            { new FloatRangeStruct(0, 0), (13, "Becketts")},
            { new FloatRangeStruct(0, 0), (14, "Chapel")},
            { new FloatRangeStruct(0, 0), (15, "Stowe")},
            { new FloatRangeStruct(0, 0), (16, "Vale")},
            { new FloatRangeStruct(0, 0), (17, "Vale")},
            { new FloatRangeStruct(0, 0), (18, "Club")},
        };
    }
}
