using ACC_Manager.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class BrandsHatch : AbstractTrackData
    {
        public override Guid Guid => new Guid("7827823f-6df1-4295-bc24-93bc83d71855");
        public override string FullName => "Brands Hatch Circuit";
        public override int TrackLength => 3908;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>()
        {
            { new FloatRangeStruct(0.02583388f, 0.09228041f), "Paddock Hill Bend" },
            { new FloatRangeStruct(0.128806f, 0.1749151f), "Druids" },
            { new FloatRangeStruct(0.2037492f, 0.2366925f), "Graham Hill Bend" },
            { new FloatRangeStruct(0.2918635f, 0.3475159f), "Surtees" },
            { new FloatRangeStruct(0.4960743f, 0.5494739f), "Hawthorn Bend" },
            { new FloatRangeStruct(0.5836455f, 0.6203906f), "Westfield Bend" },
            { new FloatRangeStruct(0.65591f, 0.6856291f), "Dingle Dell" },
            { new FloatRangeStruct(0.6919114f, 0.7209466f), "Sheene's" },
            { new FloatRangeStruct(0.7484918f, 0.7844133f), "Stirlings" },
            { new FloatRangeStruct(0.8544036f, 0.9092523f), "Clark Curve" }
        };
    }
}
