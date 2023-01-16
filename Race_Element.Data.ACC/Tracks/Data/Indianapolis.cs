using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Indianapolis : AbstractTrackData
    {
        public override Guid Guid => new Guid("d3c246d2-edba-429e-af59-6e25357d59d4");
        public override string FullName => "Indianapolis Motor Speedway";
        public override int TrackLength => 4167;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>()
        {
            { new FloatRangeStruct(0.1243783f, 0.1692718f), "Turn 1" },
            { new FloatRangeStruct(0.1696899f, 0.195541f), "Turn 2" },
            { new FloatRangeStruct(0.195921f, 0.2312392f), "Turn 3" },
            { new FloatRangeStruct(0.2340903f, 0.2812699f), "Turn 4" },
            { new FloatRangeStruct(0.2904321f, 0.325332f), "Turn 5" },
            { new FloatRangeStruct(0.3372692f, 0.3911017f), "Turn 6" },
            { new FloatRangeStruct(0.3939147f, 0.4355434f), "Turn 7" },
            { new FloatRangeStruct(0.5369371f, 0.5751812f), "Turn 8" },
            { new FloatRangeStruct(0.5837724f, 0.6061648f), "Turn 9" },
            { new FloatRangeStruct(0.6077616f, 0.6273403f), "Turn 10" },
            { new FloatRangeStruct(0.6314081f, 0.6756219f), "Turn 11" },
            { new FloatRangeStruct(0.6912087f, 0.7700945f), "Turn 12" },
            { new FloatRangeStruct(0.7792981f, 0.8153729f), "Turn 13" },
            { new FloatRangeStruct(0.8205053f, 0.8587886f), "Turn 14" },
            { new FloatRangeStruct(0.8633122f, 0.9517404f), "Turn 15" },
        };
    }
}
