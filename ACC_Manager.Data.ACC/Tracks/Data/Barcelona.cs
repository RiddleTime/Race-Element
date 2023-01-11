using ACC_Manager.Util.DataTypes;
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

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>()
        {
            { new FloatRangeStruct(0.1577717f, 0.2195874f), "Elf" },
            { new FloatRangeStruct(0.2282194f, 0.3202321f), "Renault" },
            { new FloatRangeStruct(0.3537325f, 0.4274578f), "Repsol" },
            { new FloatRangeStruct(0.4452616f, 0.4849422f), "Seat" },
            { new FloatRangeStruct(0.5345824f, 0.5912151f), "Würth" },
            { new FloatRangeStruct(0.6078672f, 0.6661669f), "Campsa" },
            { new FloatRangeStruct(0.7314663f, 0.7708687f), "La Caixa" },
            { new FloatRangeStruct(0.7779655f, 0.8429551f), "Banc de Sabadell" },
            { new FloatRangeStruct(0.8548293f, 0.8812364f), "Europcar" },
            { new FloatRangeStruct(0.8875112f, 0.9133028f), "Chicane RACC" },
            { new FloatRangeStruct(0.917703f, 0.9730397f), "New Holland" }
        };
    }
}
