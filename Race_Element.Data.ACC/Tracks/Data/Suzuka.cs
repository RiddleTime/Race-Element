using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
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
            { new FloatRangeStruct(0.03883164f, 0.0825785f), (1, "First Curve") },
            { new FloatRangeStruct(0.08290438f, 0.1167337f), (2, "First Curve") },
            { new FloatRangeStruct(0.1298746f, 0.1483641f), (3, "'S' Curves") },
            { new FloatRangeStruct(0.1498575f, 0.170356f), (4, "'S' Curves") },
            { new FloatRangeStruct(0.1708718f, 0.1984839f), (5, "'S' Curves") },
            { new FloatRangeStruct(0.1984840f, 0.2325576f), (6, "'S' Curves") },
            { new FloatRangeStruct(0.2386938f, 0.3178378f), (7, "Dunlop") },
            { new FloatRangeStruct(0.3310606f, 0.3528073f), (8, "Degner 1") },
            { new FloatRangeStruct(0.3596497f, 0.3821299f), (9, "Degner 2") },
            { new FloatRangeStruct(0.4178599f, 0.4266207f), (10, "") },
            { new FloatRangeStruct(0.4306207f, 0.4615963f), (11, "Hairpin") },
            { new FloatRangeStruct(0.4827491f, 0.5779635f), (12, "200R") },
            { new FloatRangeStruct(0.5862223f, 0.6169538f), (13, "Spoon Curve") },
            { new FloatRangeStruct(0.6183642f, 0.6618876f), (14, "Spoon Curve") },
            { new FloatRangeStruct(0.7896389f, 0.8385026f), (15, "130R") },
            { new FloatRangeStruct(0.8555256f, 0.8820519f), (16, "Casio Triangle") },
            { new FloatRangeStruct(0.8828682f, 0.8949211f), (17, "Casio Triangle") },
            { new FloatRangeStruct(0.8986408f, 0.9453408f), (18, "Last Curve") }
        };
    }
}
