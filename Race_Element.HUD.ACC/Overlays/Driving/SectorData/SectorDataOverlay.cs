using Newtonsoft.Json;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Internal;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.SectorData;

[Overlay(Name = "Sector Data",
Description = "Shows data from previous sectors",
Authors = ["Reinier Klarenberg"])]
internal sealed class SectorDataOverlay : AbstractOverlay
{
    private readonly SectorDataConfiguration _config = new();


    /*

                L2     L3     Best
         S1
         S2
         S3  


        | Sector | vMin   | vMax   |
        | 3      | 110.99 | 236.29 |
        | 2      | 60.97  | 246.56 |
        | 1      | 96.59  | 245.33 |



     */



    private readonly List<SectorDataModel> _Sectors = [];
    private SectorDataJob _datajob;

    public SectorDataOverlay(Rectangle rectangle) : base(rectangle, "Sector Data")
    {
        Width = 300;
        Height = 50;
        RefreshRateHz = 0.5f;
    }

    public override void SetupPreviewData()
    {
        _Sectors.Add(new SectorDataModel() { SectorIndex = 1, VelocityMin = 0, VelocityMax = 0 });
        _Sectors.Add(new SectorDataModel() { SectorIndex = 2, VelocityMin = 0, VelocityMax = 0 });
        _Sectors.Add(new SectorDataModel() { SectorIndex = 0, VelocityMin = 0, VelocityMax = 0 });
    }

    public override void BeforeStart()
    {
        _datajob = new SectorDataJob() { IntervalMillis = 100 };
        _datajob.OnSectorCompleted += SectorCompleted;
        _datajob.Run();

        RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;
    }

    private void Instance_OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e) => _Sectors.Clear();

    private void SectorCompleted(object sender, SectorDataModel e)
    {
        _Sectors.Add(e);
        Debug.WriteLine($"Sector {e.SectorIndex + 1} completed:\n{JsonConvert.SerializeObject(e)}");
    }

    public override void BeforeStop()
    {
        _datajob.OnSectorCompleted -= SectorCompleted;
        _datajob.CancelJoin();
        RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
    }

    public override void Render(Graphics g)
    {

    }
}
