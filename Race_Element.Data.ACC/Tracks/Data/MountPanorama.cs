using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data
{
    internal class MountPanorama : AbstractTrackData
    {
        public override Guid Guid => new Guid("c054299c-0e16-4094-8e76-a9a4da399268");
        public override string GameName => "mount_panorama";
        public override string FullName => "Mount Panorama Circuit";
        public override int TrackLength => 6213;

        public override List<float> Sectors => new List<float>() { 0.313f, 0.760f };

        public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new Dictionary<FloatRangeStruct, (int, string)>()
        {
            { new FloatRangeStruct(0.04306454f, 0.07633644f), (1, "Hell Corner")},
            { new FloatRangeStruct(0.2216722f, 0.2671284f), (2, "Quarry")},
            { new FloatRangeStruct(0.2971156f, 0.312538f), (3, string.Empty)},
            { new FloatRangeStruct(0.3153742f, 0.3337344f), (4, "The Cutting")},
            { new FloatRangeStruct(0.3372542f, 0.3564246f), (5, string.Empty)},
            { new FloatRangeStruct(0.3632621f, 0.3830402f), (6, "Griffin's Mount")},
            { new FloatRangeStruct(0.3833188f, 0.4021347f), (7, "Reid Park")},
            { new FloatRangeStruct(0.4031347f, 0.4219127f), (8, "Frog Hollow")},
            { new FloatRangeStruct(0.4224446f, 0.4552103f), (9, "Sulman Park")},
            { new FloatRangeStruct(0.465421f, 0.507787f), (10, "McPhillamy Park")},
            { new FloatRangeStruct(0.5240199f, 0.5376696f), (11, "Skyline")},
            { new FloatRangeStruct(0.5385813f, 0.5478246f), (12, "The Esses")},
            { new FloatRangeStruct(0.5486601f, 0.5628162f), (13, "The Esses")},
            { new FloatRangeStruct(0.563171f, 0.5718289f), (14, "The Dipper")},
            { new FloatRangeStruct(0.5725902f, 0.5816819f), (15, "The Dipper")},
            { new FloatRangeStruct(0.5878358f, 0.6015362f), (16, string.Empty)},
            { new FloatRangeStruct(0.6016375f, 0.6116911f), (17, string.Empty)},
            { new FloatRangeStruct(0.6119199f, 0.6392431f), (18, "Forrest's Elbow")},
            { new FloatRangeStruct(0.6476261f, 0.6779644f), (19, string.Empty)},
            { new FloatRangeStruct(0.820564f, 0.8566262f), (20, "The Chase")},
            { new FloatRangeStruct(0.8625019f, 0.8952723f), (21, "The Chase")},
            { new FloatRangeStruct(0.8961567f, 0.9169981f), (22, "The Chase")},
            { new FloatRangeStruct(0.9625041f, 0.9959303f), (23, "Murray's Corner")},
        };
    }
}
