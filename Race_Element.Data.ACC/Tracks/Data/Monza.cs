using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Monza : AbstractTrackData
    {
        public override Guid Guid => new Guid("5091ac88-c7c3-4cf1-ac46-e974bc7b73d5");
        public override string FullName => "Monza Circuit";
        public override int TrackLength => 5793;

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            //{ new FloatRangeStruct(0, 0), (1, "Variante del Rettifilo") },
            //{ new FloatRangeStruct(0, 0), (2, "Variante del Rettifilo") },
            //{ new FloatRangeStruct(0, 0), (3, "Curva Biassono") },
            //{ new FloatRangeStruct(0, 0), (4, "Variante della Roggia") },
            //{ new FloatRangeStruct(0, 0), (5, "Variante della Roggia") },
            //{ new FloatRangeStruct(0, 0), (6, "Curva di Lesmo") },
            //{ new FloatRangeStruct(0, 0), (7, "Curva di Lesmo") },
            //{ new FloatRangeStruct(0, 0), (8, "Variante Ascari") },
            //{ new FloatRangeStruct(0, 0), (9, "Variante Ascari") },
            //{ new FloatRangeStruct(0, 0), (10, "Variante Ascari") },
            //{ new FloatRangeStruct(0, 0), (11, "Curva Parabolica") },
        };
    }
}
