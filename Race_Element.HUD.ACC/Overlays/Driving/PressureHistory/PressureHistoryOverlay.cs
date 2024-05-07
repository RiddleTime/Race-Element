using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.PressureHistory;

[Overlay(Name = "Pressure History",
Description = "TODO",
OverlayCategory = OverlayCategory.Physics,
OverlayType = OverlayType.Drive,
Authors = ["Reinier Klarenberg"],
Version = 1.0)]
internal sealed class PressureHistoryOverlay : AbstractOverlay
{
    private readonly PressureHistoryConfiguration _config = new();
    private sealed class PressureHistoryConfiguration : OverlayConfiguration
    {

    }

    private PressureHistoryJob _historyJob;
    private readonly List<PressureHistoryModel> _pressureHistory = [];
    private InfoPanel _panel;

    public PressureHistoryOverlay(Rectangle rectangle) : base(rectangle, "Pressure History")
    {
    }

    public sealed override void BeforeStart()
    {
        _panel = new InfoPanel(10, 400);

        if (IsPreviewing) return;

        RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;

        _historyJob = new(this) { IntervalMillis = 100 };
        _historyJob.OnNewHistory += OnNewHistory;
        _historyJob.Run();
    }

    private void Instance_OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e)
    {
        _pressureHistory.Clear();
    }

    private void OnNewHistory(object sender, PressureHistoryModel model) => _pressureHistory.Add(model);

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        _historyJob.OnNewHistory -= OnNewHistory;
        _historyJob?.CancelJoin();
    }

    public sealed override void Render(Graphics g)
    {
        PressureHistoryModel last = _pressureHistory.Last();
        if (last == null) return;
        _panel.AddLine($"L{last.Lap}", $"Min - Max  -  Avg");

        string[] tyres = ["FL", "FR", "RL", "RR"];
        for (int i = 0; i < 4; i++)
        {
            _panel.AddLine(tyres[i], $"{last.Min[i]:F1} - {last.Max[i]:F1} - {last.Averages[i]:F1}");
        }

        _panel.Draw(g);
    }
}
