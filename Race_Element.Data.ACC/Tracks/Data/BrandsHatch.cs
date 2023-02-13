using RaceElement.Util.DataTypes;
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

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.02583388f, 0.09228041f), (1, "Paddock Hill Bend") },
            { new FloatRangeStruct(0.128806f, 0.1749151f), (2, "Druids") },
            { new FloatRangeStruct(0.2037492f, 0.2366925f), (3, "Graham Hill Bend") },
            { new FloatRangeStruct(0.2918635f, 0.3475159f), (4, "Surtees") },
            { new FloatRangeStruct(0.4960743f, 0.5494739f), (5, "Hawthorn Bend") },
            { new FloatRangeStruct(0.5836455f, 0.6203906f), (6, "Westfield Bend") },
            { new FloatRangeStruct(0.65591f, 0.6856291f), (7, "Dingle Dell") },
            { new FloatRangeStruct(0.6919114f, 0.7209466f), (8, "Sheene's") },
            { new FloatRangeStruct(0.7484918f, 0.7844133f), (9, "Stirlings") },
            { new FloatRangeStruct(0.8544036f, 0.9092523f), (10, "Clark Curve") }
        };
    }
}
