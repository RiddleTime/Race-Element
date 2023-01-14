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

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>()
        {
            { new FloatRangeStruct(0.03883164f, 0.08408849f), "First Corner" },
            { new FloatRangeStruct(0.1298746f, 0.1984839f), "Snake" },
            { new FloatRangeStruct(0.1984840f, 0.2325576f), "Anti-Banked Curve" },
            { new FloatRangeStruct(0.2386938f, 0.3178378f), "Dunlop" },
            { new FloatRangeStruct(0.3310606f, 0.3528073f), "Degner 1" },
            { new FloatRangeStruct(0.3596497f, 0.3821299f), "Degner 2" },
            { new FloatRangeStruct(0.4306207f, 0.4615963f), "Hairpin" },
            { new FloatRangeStruct(0.4827491f, 0.5779635f), "200R" },
            { new FloatRangeStruct(0.5862223f, 0.6618876f), "Spoon Curve" },
            { new FloatRangeStruct(0.7896389f, 0.8385026f), "130R" },
            { new FloatRangeStruct(0.8555256f, 0.8949211f), "Casio Triangle" },
            { new FloatRangeStruct(0.8986408f, 0.9453408f), "Last Curve" }
        };
    }
}
