using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Barcelona : AbstractTrackData
    {
        public override Guid Guid => new Guid("c47d348d-4cac-4377-90ff-be3613bc6519");
        public override string FullName => "Circuit de Barcelona-Catalunya";
        public override int TrackLength => 4655;

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.1577717f, 0.2195874f), (1,"Elf") },
            { new FloatRangeStruct(0.2282194f, 0.3202321f), (1, "Renault") },
            { new FloatRangeStruct(0.3504574f, 0.4274578f), (1, "Repsol") },
            { new FloatRangeStruct(0.4452616f, 0.4849422f), (1, "Seat") },
            { new FloatRangeStruct(0.5345824f, 0.5912151f), (1, "Würth") },
            { new FloatRangeStruct(0.6078672f, 0.6661669f), (1, "Campsa") },
            { new FloatRangeStruct(0.7314663f, 0.7708687f), (1, "La Caixa") },
            { new FloatRangeStruct(0.7779655f, 0.8429551f), (1, "Banc de Sabadell") },
            { new FloatRangeStruct(0.8548293f, 0.8812364f), (1, "Europcar") },
            { new FloatRangeStruct(0.8875112f, 0.9133028f), (1, "Chicane RACC") },
            { new FloatRangeStruct(0.917703f, 0.9730397f), (1, "New Holland") }
        };
    }
}
