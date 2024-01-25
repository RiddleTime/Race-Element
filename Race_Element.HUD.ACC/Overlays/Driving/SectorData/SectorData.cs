using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.SectorData;

[Overlay(Name = "Sector Data", Description = "Shows data from previous sectors")]
internal class SectorData : AbstractOverlay
{
    private SectorDataJob _datajob;

    private List<SectorDataModel> _Sectors = [];

    public SectorData(Rectangle rectangle) : base(rectangle, "Sector Data")
    {
        Width = 300;
        Height = 50;
    }

    public override void BeforeStart()
    {
        _datajob = new SectorDataJob() { IntervalMillis = 100 };
        _datajob.OnSectorCompleted += SectorCompleted;
        _datajob.Run();
    }

    private void SectorCompleted(object sender, SectorDataModel e)
    {
        _Sectors.Add(e);
    }

    public override void BeforeStop()
    {
        _datajob.OnSectorCompleted -= SectorCompleted;
        _datajob?.CancelJoin();
    }

    public override void Render(Graphics g)
    {
        foreach (SectorDataModel e in _Sectors.TakeLast(3))
            Debug.WriteLine(e);
    }
}
