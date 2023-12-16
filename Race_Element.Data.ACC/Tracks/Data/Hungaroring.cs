using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Hungaroring : AbstractTrackData
    {
        public override Guid Guid => new Guid("f59e6015-077b-40e0-a822-71104f253ea2");
        public override string GameName => "Hungaroring";
        public override string FullName => "Hungaroring";
        public override int TrackLength => 4381;

        public override List<float> Sectors => new List<float>() { 0.397f, 0.748f };

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.117462f, 0.1763413f), (1, string.Empty) },
            { new FloatRangeStruct(0.2342479f, 0.2827142f), (2, string.Empty) },
            { new FloatRangeStruct(0.291177f, 0.3305657f), (3, string.Empty) },
            { new FloatRangeStruct(0.3976955f, 0.4400478f), (4, string.Empty) },
            { new FloatRangeStruct(0.4505019f, 0.5123134f), (5, string.Empty) },
            { new FloatRangeStruct(0.5293858f, 0.55264f), (6, string.Empty) },
            { new FloatRangeStruct(0.5531477f, 0.5745245f), (7, string.Empty) },
            { new FloatRangeStruct(0.5845071f, 0.6076503f), (8, string.Empty) },
            { new FloatRangeStruct(0.611815f, 0.645991f), (9, string.Empty) },
            { new FloatRangeStruct(0.6554344f, 0.69012f), (10, string.Empty) },
            { new FloatRangeStruct(0.6952906f, 0.7409733f), (11, string.Empty) },
            { new FloatRangeStruct(0.7882351f, 0.8321922f), (12, string.Empty) },
            { new FloatRangeStruct(0.8449233f, 0.8961363f), (13, string.Empty) },
            { new FloatRangeStruct(0.9104271f, 0.9746534f), (14, string.Empty) }
        };
    }
}
