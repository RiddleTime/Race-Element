using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class PaulRicard : AbstractTrackData
    {
        public override Guid Guid => new("c46a0299-b5d0-421f-90a7-0c5223deaa63");
        public override string GameName => "Paul_Ricard";
        public override string FullName => "Circuit Paul Ricard";
        public override int TrackLength => 5770;

        public override List<float> Sectors => new() { 0.264f, 0.591f };

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
        {
             { new FloatRangeStruct(0.08376743f, 0.1196156f), (1, "\"S\" de la Verrerie")},
             { new FloatRangeStruct(0.1196157f, 0.1582872f), (2,  "\"S\" de la Verrerie")},
             { new FloatRangeStruct(0.2153098f, 0.2333817f), (3, "Virage de l'hotel")},
             { new FloatRangeStruct(0.2333818f, 0.2557339f), (4, "Virage de l'hotel")},
             { new FloatRangeStruct(0.2557340f, 0.2690696f), (5, "Virage du camp")},
             { new FloatRangeStruct(0.2701924f, 0.3092493f), (6, "Virage de la Sainte-Beaume")},
             { new FloatRangeStruct(0.3092494f, 0.3487825f), (7, "L'ecole")},
             { new FloatRangeStruct(0.3606598f, 0.6257137f), (-1, "Ligne Droite du Mistral")}, // mistral straight
             { new FloatRangeStruct(0.6409133f, 0.7051377f), (8, "Courbe de Signes")},
             { new FloatRangeStruct(0.7228328f, 0.793738f), (9, "Double Droite du Beausset")},
             { new FloatRangeStruct(0.811732f, 0.8442134f), (10, "Virage de Bendor")},
             { new FloatRangeStruct(0.8442135f, 0.8944696f), (11, "Courbe du Garlaben")},
             { new FloatRangeStruct(0.8997006f, 0.9318284f), (12, "Virage du Lac")},
             { new FloatRangeStruct(0.9354651f, 0.9590782f), (13, "Virage du Pont")},
        };
    }
}
