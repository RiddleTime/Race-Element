using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Imola : AbstractTrackData
    {
        public override Guid Guid => new Guid("678eefc3-a5f0-4b2a-a1cc-03ac62650ede");
        public override string FullName => "Imola (Autodromo Internazionale Enzo e Dino Ferrari)";
        public override int TrackLength => 4959;

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.0005297028f, 0.03412261f), (1, string.Empty) },
            { new FloatRangeStruct(0.112587f, 0.1497782f), (2, "Variante Tamburello") },
            { new FloatRangeStruct(0.1506454f, 0.1665117f), (3, "Variante Tamburello") },
            { new FloatRangeStruct(0.172935f, 0.2106085f), (4, "Variante Tamburello") },
            { new FloatRangeStruct(0.249086f, 0.2795623f), (5, "Villeneuve") },
            { new FloatRangeStruct(0.2797552f, 0.3088191f), (6, "Villeneuve") },
            { new FloatRangeStruct(0.3316593f, 0.3782925f), (7, "Tosa") },
            { new FloatRangeStruct(0.4357499f, 0.4546386f), (8, "Piratella") },
            { new FloatRangeStruct(0.4577822f, 0.4927896f), (9, "Piratella") },
            { new FloatRangeStruct(0.4951028f, 0.5294678f), (10, "Piratella") },
            { new FloatRangeStruct(0.5453337f, 0.5661795f), (11, "Acque Minerali") },
            { new FloatRangeStruct(0.568651f, 0.5879206f), (12, "Acque Minerali") },
            { new FloatRangeStruct(0.5892689f, 0.6074478f), (13, "Acque Minerali") },
            { new FloatRangeStruct(0.6652913f, 0.6895717f), (14, "Variante Alta") },
            { new FloatRangeStruct(0.6900202f, 0.7139801f), (15, "Variante Alta") },
            { new FloatRangeStruct(0.775421f, 0.8178471f), (16, "Rivazza") },
            { new FloatRangeStruct(0.8203523f, 0.855458f), (17, "Rivazza") },
            { new FloatRangeStruct(0.8595361f, 0.8906525f), (18, "Rivazza") },
            { new FloatRangeStruct(0.9167676f, 0.9699543f), (19, string.Empty) },
        };
    }
}
