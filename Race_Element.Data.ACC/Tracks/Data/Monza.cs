using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Monza : AbstractTrackData
    {
        public override Guid Guid => new Guid("5091ac88-c7c3-4cf1-ac46-e974bc7b73d5");
        public override string GameName => "monza";
        public override string FullName => "Monza Circuit";
        public override int TrackLength => 5793;

        public override List<float> Sectors => new List<float>() { };

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.1326408f, 0.1643242f), (1, "Variante del Rettifilo") },
            { new FloatRangeStruct(0.1643243f, 0.1892741f), (2, "Variante del Rettifilo") },
            { new FloatRangeStruct(0.1892742f, 0.323231f), (3, "Curva Grande") },
            { new FloatRangeStruct(0.3445441f, 0.3730785f), (4, "Variante della Roggia") },
            { new FloatRangeStruct(0.3730786f, 0.3934931f), (5, "Variante della Roggia") },
            { new FloatRangeStruct(0.4241785f, 0.4670431f), (6, "Curva di Lesmo 1") },
            { new FloatRangeStruct(0.4824106f, 0.5245997f), (7, "Curva di Lesmo 2") },
            { new FloatRangeStruct(0.6568757f, 0.6875821f), (8, "Variante Ascari") },
            { new FloatRangeStruct(0.6875822f, 0.7080253f), (9, "Variante Ascari") },
            { new FloatRangeStruct(0.7080254f, 0.7384073f), (10, "Variante Ascari") },
            { new FloatRangeStruct(0.8639198f, 0.9587275f), (11, "Curva Parabolica") },
        };
    }
}
