using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Cota : AbstractTrackData
    {
        public override Guid Guid => new Guid("f45eac53-7a77-4fe5-812f-064b30ac22df");
        public override string FullName => "Circuit of the Americas";
        public override int TrackLength => 5513;

        public override Dictionary<FloatRangeStruct, string> CornerNames => new Dictionary<FloatRangeStruct, string>()
        {
            { new FloatRangeStruct(0.09711581f, 0.1336419f), "Turn 1" },
            { new FloatRangeStruct(0.139182f, 0.1873701f), "Turn 2" },
            { new FloatRangeStruct(0.1990563f, 0.2206976f), "Turn 3" },
            { new FloatRangeStruct(0.2225156f, 0.2368565f), "Turn 4" },
            { new FloatRangeStruct(0.2387608f, 0.2539387f), "Turn 5" },
            { new FloatRangeStruct(0.2557565f, 0.2953745f), "Turn 6" },
            { new FloatRangeStruct(0.2992989f, 0.3273459f), "Turn 7" },
            { new FloatRangeStruct(0.3315877f, 0.3512957f), "Turn 8" },
            { new FloatRangeStruct(0.3534598f, 0.3714648f), "Turn 9" },
            { new FloatRangeStruct(0.3827475f, 0.4132474f), "Turn 10" },
            { new FloatRangeStruct(0.4447892f, 0.4825258f), "Turn 11" },
            { new FloatRangeStruct(0.6667117f, 0.6991135f), "Turn 12" },
            { new FloatRangeStruct(0.7148103f, 0.7378947f), "Turn 13" },
            { new FloatRangeStruct(0.7392216f, 0.7586122f), "Turn 14" },
            { new FloatRangeStruct(0.7662588f, 0.7915642f), "Turn 15" },
            { new FloatRangeStruct(0.8056163f, 0.8274886f), "Turn 16" },
            { new FloatRangeStruct(0.8287005f, 0.8437918f), "Turn 17" },
            { new FloatRangeStruct(0.8478028f, 0.88618f), "Turn 18" },
            { new FloatRangeStruct(0.8997419f, 0.9391019f), "Turn 19" },
            { new FloatRangeStruct(0.9560677f, 0.9854119f), "Turn 20" }
        };
    }
}
