using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace ACCManager.Data.ACC.Tracks.Data;

internal class Nurburgring : AbstractTrackData
{
    public override Guid Guid => new("20200ee1-89c1-4580-86f1-3ded3018e9e3");
    public override string GameName => "nurburgring";
    public override string FullName => "Nürburgring";
    public override int TrackLength => 5137;

    public override List<float> Sectors => new() { 0.451f, 0.858f };

    // https://www.paradigmshiftracing.com/uploads/4/8/2/6/48261497/nurburgring-map_orig.png
    public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
    {
        { new FloatRangeStruct(0.09318506f, 0.1456866f), (1, "Yohohama Kurve")},
        { new FloatRangeStruct(0.1456867f, 0.184711f), (2, "Mercedes Arena")},
        { new FloatRangeStruct(0.1969276f, 0.2212936f), (3, "Mercedes Arena")},
        { new FloatRangeStruct(0.2212937f, 0.255046f), (4, "")},
        { new FloatRangeStruct(0.3067813f, 0.3421334f), (5, "Valvoline Kurve")},
        { new FloatRangeStruct(0.3421334f, 0.3856323f), (6, "Ford Kurve")},
        { new FloatRangeStruct(0.4532511f, 0.5097467f), (7, "Dunlop Kehre")},
        { new FloatRangeStruct(0.5316759f, 0.5679827f), (8, "Michael Shumacher S")},
        { new FloatRangeStruct(0.5679828f, 0.6083622f), (9, "Michael Shumacher S")},
        { new FloatRangeStruct(0.6443669f, 0.6861078f), (10, "Kumho Kurve")},
        { new FloatRangeStruct(0.6861079f, 0.7350352f), (11, "Bit Kurve")},
        { new FloatRangeStruct(0.7619339f, 0.8232012f), (12, "Advan Bogen")},
        { new FloatRangeStruct(0.8652532f, 0.8884809f), (13, "NGK Schikane")},
        { new FloatRangeStruct(0.8884810f, 0.9110933f), (14, "NGK Schikane")},
        { new FloatRangeStruct(0.9246625f, 0.9747647f), (15, "Coca-Cola Kurve")},
    };
}
