using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Barcelona : AbstractTrackData
    {
        public override Guid Guid => new Guid("c47d348d-4cac-4377-90ff-be3613bc6519");
        public override string GameName => "Barcelona";
        public override string FullName => "Circuit de Barcelona-Catalunya";
        public override int TrackLength => 4655;

        public override List<float> Sectors => new List<float>() { 0.348f, 0.730f };

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.1577717f, 0.1954681f), (1, "Elf") },
            { new FloatRangeStruct(0.1967648f, 0.2195874f), (2, "Elf") },
            { new FloatRangeStruct(0.2282194f, 0.3202321f), (3, "Renault") },
            { new FloatRangeStruct(0.3504574f, 0.4274578f), (4, "Repsol") },
            { new FloatRangeStruct(0.4452616f, 0.4849422f), (5, "Seat") },
            { new FloatRangeStruct(0.5049684f, 0.5296696f), (6, string.Empty) },
            { new FloatRangeStruct(0.5345824f, 0.5626252f), (7, "Würth") },
            { new FloatRangeStruct(0.5643317f, 0.5912151f), (8, "Würth") },
            { new FloatRangeStruct(0.6078672f, 0.6661669f), (9, "Campsa") },
            { new FloatRangeStruct(0.7314663f, 0.7708687f), (10, "La Caixa") },
            { new FloatRangeStruct(0.7779655f, 0.792363f), (11, "Banc de Sabadell") },
            { new FloatRangeStruct(0.7934548f, 0.8429551f), (12, "Banc de Sabadell") },
            { new FloatRangeStruct(0.8548293f, 0.8812364f), (13, "Europcar") },
            { new FloatRangeStruct(0.8875112f, 0.8994124f), (14, "Chicane RACC") },
            { new FloatRangeStruct(0.9000266f, 0.9133028f), (15, "Chicane RACC") },
            { new FloatRangeStruct(0.917703f, 0.9730397f), (16, "New Holland") }
        };

    }
}
