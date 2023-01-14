using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class SpaFrancorchamps : AbstractTrackData
    {
        public override Guid Guid => new Guid("a56b5381-6c59-4380-8a32-679c8734a9a9");
        public override string FullName => "Circuit De Spa-Francorchamps";
        public override int TrackLength => 7004;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>()
        {
            { new FloatRangeStruct(0.03371242f, 0.05937995f), "La Source" },
            { new FloatRangeStruct(0.1435904f, 0.1541221f), "Eau Rouge" },
            { new FloatRangeStruct(0.1541221f, 0.1960247f), "Raidillon" },
            { new FloatRangeStruct(0.2178622f, 0.2374316f), "Kemmel" },
            { new FloatRangeStruct(0.3228342f, 0.3612153f), "Les Combes" },
            { new FloatRangeStruct(0.3668639f, 0.3903512f), "Malmedy" },
            { new FloatRangeStruct(0.4134307f, 0.4498297f), "Bruxelles" },
            { new FloatRangeStruct(0.4582162f, 0.4808743f), "Speaker's Corner" },
            { new FloatRangeStruct(0.5299657f, 0.5979356f), "Pouhon" },
            { new FloatRangeStruct(0.622636f, 0.6775055f), "Les Fagnes" },
            { new FloatRangeStruct(0.6919841f, 0.7136846f), "Campus" },
            { new FloatRangeStruct(0.7219031f, 0.7557863f), "Stavelot" },
            { new FloatRangeStruct(0.7856864f, 0.8112493f), "Courbe Paul Frere" },
            { new FloatRangeStruct(0.8585455f, 0.8978195f), "Blanchimont" },
            { new FloatRangeStruct(0.9418995f, 0.978526f), "Chicane" }
        };
    }
}
