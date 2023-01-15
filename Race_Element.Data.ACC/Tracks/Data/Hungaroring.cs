using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Hungaroring : AbstractTrackData
    {
        public override Guid Guid => new Guid("f59e6015-077b-40e0-a822-71104f253ea2");
        public override string FullName => "Hungaroring";
        public override int TrackLength => 4381;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>()
        {
            { new FloatRangeStruct(0.117462f, 0.1763413f), "Turn 1" },
            { new FloatRangeStruct(0.2342479f, 0.2827142f), "Turn 2" },
            { new FloatRangeStruct(0.291177f, 0.3305657f), "Turn 3" },
            { new FloatRangeStruct(0.3976955f, 0.4400478f), "Turn 4" },
            { new FloatRangeStruct(0.4505019f, 0.5123134f), "Turn 5" },
            { new FloatRangeStruct(0.5293858f, 0.55264f), "Turn 6" },
            { new FloatRangeStruct(0.5531477f, 0.5745245f), "Turn 7" },
            { new FloatRangeStruct(0.5845071f, 0.6076503f), "Turn 8" },
            { new FloatRangeStruct(0.611815f, 0.645991f), "Turn 9" },
            { new FloatRangeStruct(0.6554344f, 0.69012f), "Turn 10" },
            { new FloatRangeStruct(0.6952906f, 0.7409733f), "Turn 11" },
            { new FloatRangeStruct(0.7882351f, 0.8321922f), "Turn 12" },
            { new FloatRangeStruct(0.8449233f, 0.8961363f), "Turn 13" },
            { new FloatRangeStruct(0.9104271f, 0.9746534f), "Turn 14" }
        };
    }
}
