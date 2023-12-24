using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class Kyalami : AbstractTrackData
    {
        public override Guid Guid => new("65e00cd4-6c39-4cb4-acf9-f8977cd56ba1");
        public override string GameName => "Kyalami";
        public override string FullName => "Kyalami Grand Prix Circuit";
        public override int TrackLength => 4522;

        public override List<float> Sectors => new() { 0.316f, 0.710f };

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
        {
            { new FloatRangeStruct(0.03799635f, 0.07576057f), (1, "The Kink")},
            { new FloatRangeStruct(0.1186805f, 0.1687038f), (2, "Crowthorne")},
            { new FloatRangeStruct(0.1716466f, 0.1938228f), (3, "Jukskei Sweep")},
            { new FloatRangeStruct(0.1972911f, 0.2116902f), (4, "Jukskei Sweep")},
            { new FloatRangeStruct(0.2149132f, 0.2511375f), (5, "Barbeque")},
            { new FloatRangeStruct(0.3214881f, 0.3990535f), (6, "Sunset")},
            { new FloatRangeStruct(0.4100153f, 0.4576957f), (7, "Clubhouse")},
            { new FloatRangeStruct(0.4831304f, 0.5110525f), (8, "The Esses")},
            { new FloatRangeStruct(0.5127696f, 0.5624465f), (9, "The Esses")},
            { new FloatRangeStruct(0.6093207f, 0.6448103f), (10, "Leeukop")},
            { new FloatRangeStruct(0.646807f, 0.6739212f), (11, "Leeukop")},
            { new FloatRangeStruct(0.7203508f, 0.7789884f), (12, "Mineshaft")},
            { new FloatRangeStruct(0.7924412f, 0.8349011f), (13, "The Crocodiles")},
            { new FloatRangeStruct(0.8374949f, 0.8632089f), (14, "The Crocodiles")},
            { new FloatRangeStruct(0.866816f, 0.8997451f), (15, "Cheetah")},
            { new FloatRangeStruct(0.9204187f, 0.962459f), (16, "Ingwe")},
        };
    }
}
