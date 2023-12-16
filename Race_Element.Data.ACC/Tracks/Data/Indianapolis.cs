using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Indianapolis : AbstractTrackData
    {
        public override Guid Guid => new Guid("d3c246d2-edba-429e-af59-6e25357d59d4");
        public override string GameName => "indianapolis";
        public override string FullName => "Indianapolis Motor Speedway";
        public override int TrackLength => 4167;

        public override List<float> Sectors => new List<float>() { 0.390f, 0.719f };

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.1243783f, 0.1692718f), (1, string.Empty) },
            { new FloatRangeStruct(0.1696899f, 0.195541f), (2, string.Empty) },
            { new FloatRangeStruct(0.195921f, 0.2312392f), (3, string.Empty) },
            { new FloatRangeStruct(0.2340903f, 0.2812699f), (4, string.Empty) },
            { new FloatRangeStruct(0.2904321f, 0.325332f), (5, string.Empty) },
            { new FloatRangeStruct(0.3372692f, 0.3911017f), (6, string.Empty) },
            { new FloatRangeStruct(0.3939147f, 0.4355434f), (7, string.Empty) },
            { new FloatRangeStruct(0.5369371f, 0.5751812f), (8, string.Empty) },
            { new FloatRangeStruct(0.5837724f, 0.6061648f), (9, string.Empty) },
            { new FloatRangeStruct(0.6077616f, 0.6273403f), (10, string.Empty) },
            { new FloatRangeStruct(0.6314081f, 0.6756219f), (11, string.Empty) },
            { new FloatRangeStruct(0.6912087f, 0.7700945f), (12, string.Empty) },
            { new FloatRangeStruct(0.7792981f, 0.8153729f), (13,  string.Empty) },
            { new FloatRangeStruct(0.8205053f, 0.8587886f), (14,  string.Empty) },
            { new FloatRangeStruct(0.8633122f, 0.9517404f), (15, string.Empty) },
        };
    }
}
