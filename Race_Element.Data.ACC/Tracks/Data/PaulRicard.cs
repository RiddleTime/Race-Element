using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class PaulRicard : AbstractTrackData
    {
        public override Guid Guid => new Guid("c46a0299-b5d0-421f-90a7-0c5223deaa63");
        public override string FullName => "Circuit Paul Ricard";
        public override int TrackLength => 5770;

        // https://www.circuitpaulricard.com/assets/pages-content/TurnLocation.svg
        // http://www.rml-adgroup.com/racing/LMS_2010/01_Ricard_Test/images/paul_ricard_track_637.jpg
        // https://www.paradigmshiftracing.com/uploads/4/8/2/6/48261497/paul-ricard-map_orig.png
        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
             { new FloatRangeStruct(0, 0), (1, "\"S\" de la Verrerie")},
             { new FloatRangeStruct(0, 0), (2,  "\"S\" de la Verrerie")},
             { new FloatRangeStruct(0, 0), (3, "Virage de l'hotel")},
             { new FloatRangeStruct(0, 0), (4, "Virage de l'hotel")},
             { new FloatRangeStruct(0, 0), (5, "Virage du camp")},
             { new FloatRangeStruct(0, 0), (6, "Virage de la Sainte-Beaume")},
             { new FloatRangeStruct(0, 0), (7, "L'ecole")},
             { new FloatRangeStruct(0, 0), (-1, "Ligne Droite du Mistral")}, // mistral straight
             { new FloatRangeStruct(0, 0), (8, "Courbe de Signes")},
             { new FloatRangeStruct(0, 0), (9, "Double Droite du Beausset")},
             { new FloatRangeStruct(0, 0), (10, "Virage de Bendor")},
             { new FloatRangeStruct(0, 0), (11, "Courbe du Garlaben")},
             { new FloatRangeStruct(0, 0), (12, "Virage du Lac")},
             { new FloatRangeStruct(0, 0), (13, "Virage du Pont")},
        };
    }
}
