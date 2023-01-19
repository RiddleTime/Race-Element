using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Suzuka : AbstractTrackData
    {
        public override Guid Guid => new Guid("1c9b35a5-dacb-40e2-adc9-134d75f75c3f");
        public override string FullName => "Suzuka Circuit";
        public override int TrackLength => 5807;

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.03883164f, 0.08408849f), (1, "First Corner") },
            { new FloatRangeStruct(0.1298746f, 0.1984839f), (1, "Snake") },
            { new FloatRangeStruct(0.1984840f, 0.2325576f), (1, "Anti-Banked Curve") },
            { new FloatRangeStruct(0.2386938f, 0.3178378f), (1, "Dunlop") },
            { new FloatRangeStruct(0.3310606f, 0.3528073f), (1, "Degner 1") },
            { new FloatRangeStruct(0.3596497f, 0.3821299f), (1, "Degner 2") },
            { new FloatRangeStruct(0.4306207f, 0.4615963f), (1, "Hairpin") },
            { new FloatRangeStruct(0.4827491f, 0.5779635f), (1, "200R") },
            { new FloatRangeStruct(0.5862223f, 0.6618876f), (1, "Spoon Curve") },
            { new FloatRangeStruct(0.7896389f, 0.8385026f), (1, "130R") },
            { new FloatRangeStruct(0.8555256f, 0.8949211f), (1, "Casio Triangle") },
            { new FloatRangeStruct(0.8986408f, 0.9453408f), (1, "Last Curve") }
        };
    }
}
