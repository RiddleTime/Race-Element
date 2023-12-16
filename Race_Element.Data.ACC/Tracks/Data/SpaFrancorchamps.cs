using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class SpaFrancorchamps : AbstractTrackData
    {
        public override Guid Guid => new Guid("a56b5381-6c59-4380-8a32-679c8734a9a9");
        public override string GameName => "Spa";
        public override string FullName => "Circuit de Spa-Francorchamps";
        public override int TrackLength => 7004;

        public override List<float> Sectors => new List<float>() { 0.330f, 0.716f };

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.03371242f, 0.05937995f), (1, "La Source")},
            { new FloatRangeStruct(0.08617536f, 0.109296f), (2, string.Empty)},
            { new FloatRangeStruct(0.1435904f, 0.1520347f), (3, "Eau Rouge") },
            { new FloatRangeStruct(0.1528861f, 0.1662145f), (4, "Eau Rouge") },
            { new FloatRangeStruct(0.167726f, 0.1960247f), (5, "Raidillon") },
            { new FloatRangeStruct(0.2178622f, 0.2374316f), (6, "Kemmel") },
            { new FloatRangeStruct(0.3228342f, 0.3479101f), (7, "Les Combes") },
            { new FloatRangeStruct(0.3484506f, 0.3612153f), (8, "Les Combes") },
            { new FloatRangeStruct(0.3668639f, 0.3903512f), (9, "Malmedy") },
            { new FloatRangeStruct(0.4134307f, 0.4498297f), (10, "Bruxelles") },
            { new FloatRangeStruct(0.4582162f, 0.4808743f), (11, "Speaker's Corner") },
            { new FloatRangeStruct(0.5299657f, 0.5979356f), (12, "Pouhon") },
            { new FloatRangeStruct(0.622636f, 0.650161f), (13, "Les Fagnes") },
            { new FloatRangeStruct(0.651161f, 0.6814184f), (14, "Les Fagnes") },
            { new FloatRangeStruct(0.6919841f, 0.7136846f), (15, "Campus") },
            { new FloatRangeStruct(0.7219031f, 0.8112493f), (16, "Courbe Paul Frère") },
            { new FloatRangeStruct(0.8262254f, 0.8535456f), (17, "Blanchimont") },
            { new FloatRangeStruct(0.8585456f, 0.8978195f), (18, "Blanchimont") },
            { new FloatRangeStruct(0.9418995f, 0.9643196f), (19, "Chicane") },
            { new FloatRangeStruct(0.9644473f, 0.978526f), (20, "Chicane") },
        };
    }
}
