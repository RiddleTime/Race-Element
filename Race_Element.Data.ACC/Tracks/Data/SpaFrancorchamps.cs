using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class SpaFrancorchamps : AbstractTrackData
    {
        public override Guid Guid => new Guid("a56b5381-6c59-4380-8a32-679c8734a9a9");
        public override string FullName => "Circuit De Spa-Francorchamps";
        public override int TrackLength => 7004;

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            // https://upload.wikimedia.org/wikipedia/commons/5/54/Spa-Francorchamps_of_Belgium.svg
            { new FloatRangeStruct(0.03371242f, 0.05937995f), (1, "La Source")},
            { new FloatRangeStruct(0.1435904f, 0.1541221f), (3, "Eau Rouge") },
            { new FloatRangeStruct(0.1541221f, 0.1960247f), (1, "Raidillon") },
            { new FloatRangeStruct(0.2178622f, 0.2374316f), (6, "Kemmel") },
            { new FloatRangeStruct(0.3228342f, 0.3612153f), (1, "Les Combes") },
            { new FloatRangeStruct(0.3668639f, 0.3903512f), (1, "Malmedy") },
            { new FloatRangeStruct(0.4134307f, 0.4498297f), (1, "Bruxelles") },
            { new FloatRangeStruct(0.4582162f, 0.4808743f), (1, "Speaker's Corner") },
            { new FloatRangeStruct(0.5299657f, 0.5979356f), (1, "Pouhon") },
            { new FloatRangeStruct(0.622636f, 0.6775055f), (1, "Les Fagnes") },
            { new FloatRangeStruct(0.6919841f, 0.7136846f), (1, "Campus") },
            { new FloatRangeStruct(0.7219031f, 0.7557863f), (1, "Stavelot") },
            { new FloatRangeStruct(0.7856864f, 0.8112493f), (1, "Courbe Paul Frere") },
            { new FloatRangeStruct(0.8585455f, 0.8978195f), (1, "Blanchimont") },
            { new FloatRangeStruct(0.9418995f, 0.978526f), (1, "Chicane") }
        };
    }
}
