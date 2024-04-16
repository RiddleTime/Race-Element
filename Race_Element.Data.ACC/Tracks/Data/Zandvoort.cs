using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data;

internal sealed class Zandvoort : AbstractTrackData
{
    public override Guid Guid => new("e7a091a3-b2c1-4903-8768-591a937858ea");
    public override string GameName => "Zandvoort";
    public override string FullName => "Circuit Zandvoort";
    public override int TrackLength => 4252;

    public override List<float> Sectors => new() { 0.318f, 0.654f };

    public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
    {
        { new FloatRangeStruct(0.05284053f, 0.1141072f), (1, "Tarzanbocht") },
        { new FloatRangeStruct(0.1480813f, 0.1814992f), (2, "Gerlachbocht") },
        { new FloatRangeStruct(0.1887138f, 0.2235506f), (3, "Hugenholzbocht") },
        { new FloatRangeStruct(0.2407838f, 0.2725571f), (4, "Hunzerug") },
        { new FloatRangeStruct(0.2839962f,  0.3221979f), (5, "Rob Slotemakerbocht") },
        { new FloatRangeStruct(0.3257863f, 0.3628302f), (6, "Rob Slotemakerbocht") },
        { new FloatRangeStruct(0.3733709f, 0.4406184f), (7, "Scheivlak") },
        { new FloatRangeStruct(0.4704477f, 0.5094342f), (8, "Mastersbocht") },
        { new FloatRangeStruct(0.5207254f, 0.558478f), (9, "Bocht 9") },
        { new FloatRangeStruct(0.5789623f, 0.6246402f), (10, "Bocht 10") },
        { new FloatRangeStruct(0.7076252f, 0.7403311f), (11, "Hans Ernst Bocht") },
        { new FloatRangeStruct(0.7405928f, 0.7710944f), (12, "Hans Ernst Bocht") },
        { new FloatRangeStruct(0.8137466f, 0.8532166f), (13, "Kumho") },
        { new FloatRangeStruct(0.85321661f, 0.9344479f), (14, "Arie Luyendijk Bocht") }
    };
}
