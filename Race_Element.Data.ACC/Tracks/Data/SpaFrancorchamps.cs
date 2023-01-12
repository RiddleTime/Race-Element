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
            { new FloatRangeStruct(0.04729626f, 0.05776058f), "La Source" },
            { new FloatRangeStruct(0.1435904f, 0.1541221f), "Eau Rouge" },
            { new FloatRangeStruct(0.1541221f, 0.1960247f), "Raidillon" },
        };
    }
}
