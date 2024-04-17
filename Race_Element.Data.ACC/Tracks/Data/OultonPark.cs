using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data;

internal sealed class OultonPark : AbstractTrackData
{
    public override Guid Guid => new("72794bc2-841c-40e1-8587-3e41f9228ea8");
    public override string GameName => "oulton_park";
    public override string FullName => "Oulton Park";
    public override int TrackLength => 4307;

    public override List<float> Sectors => new() { 0.264f, 0.693f };

    public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
    {
          { new FloatRangeStruct(0.03764982f, 0.09288816f), (1, "Old Hall Corner")},
          { new FloatRangeStruct(0.1238538f, 0.152804f), (2, "Denton's")},
          { new FloatRangeStruct(0.152805f, 0.2145353f), (3, "Cascades")},
          { new FloatRangeStruct(0.2897645f, 0.3609795f), (4, "Island Bend")},
          { new FloatRangeStruct(0.3609796f, 0.4114458f), (5, "Shell Oils Corner")},
          { new FloatRangeStruct(0.4329222f, 0.4631159f), (6, string.Empty)},
          { new FloatRangeStruct(0.4631160f, 0.4793058f), (7, "Britten's")},
          { new FloatRangeStruct(0.4793059f, 0.4899897f), (8, "Britten's")},
          { new FloatRangeStruct(0.4899898f, 0.5153695f), (9, "Britten's")},
          { new FloatRangeStruct(0.5827187f, 0.609301f), (10, "Hislop's")},
          { new FloatRangeStruct(0.609302f, 0.6269855f), (11, "Hislop's")},
          { new FloatRangeStruct(0.6317621f, 0.6643611f), (12, "Knickerbrook")},
          { new FloatRangeStruct(0.6828122f, 0.7284656f), (13, "Clay Hill")},
          { new FloatRangeStruct(0.7415928f, 0.7716756f), (14, string.Empty)},
          { new FloatRangeStruct(0.7733532f, 0.8394995f), (15, "Druid's Corner")},
          { new FloatRangeStruct(0.8977691f, 0.9480175f), (16, "Lodge Corner")},
          { new FloatRangeStruct(0.9534873f, 0.9853203f), (17, "Deer Leap")},
    };
}
