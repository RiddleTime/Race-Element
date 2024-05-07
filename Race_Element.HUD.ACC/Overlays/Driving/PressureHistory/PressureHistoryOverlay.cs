﻿using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
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

    private GraphicsGrid _graphicsGrid;

    public PressureHistoryOverlay(Rectangle rectangle) : base(rectangle, "Pressure History")
    {
    }

    public override void SetupPreviewData()
    {
        _pressureHistory.Add(new()
        {
            Lap = 4,
            Averages = [26.6f, 26.7f, 27.0f, 27.5f],
            Min = [26.5f, 26.6f, 26.9f, 27.4f],
            Max = [26.8f, 27.0f, 27.0f, 27.6f]
        });
    }

    public sealed override void BeforeStart()
    {
        _panel = new InfoPanel(10, 300);

        if (IsPreviewing) return;

        RaceSessionTracker.Instance.OnNewSessionStarted += OnNewSessionStarted;

        _historyJob = new(this) { IntervalMillis = 100 };
        _historyJob.OnNewHistory += OnNewHistory;
        _historyJob.Run();
    }

    private void OnNewSessionStarted(object sender, DbRaceSession e) => _pressureHistory.Clear();
    private void OnNewHistory(object sender, PressureHistoryModel model) => _pressureHistory.Add(model);

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        _historyJob.OnNewHistory -= OnNewHistory;
        _historyJob?.CancelJoin();

        _graphicsGrid?.Dispose();
    }

    private void CreateGraphicsGrid()
    {

    }

    public sealed override void Render(Graphics g)
    {
        if (_pressureHistory.Count == 0)
        {
            _panel.AddLine("..", "Waiting for data");
        }
        else
        {
            PressureHistoryModel last = _pressureHistory.Last();
            if (last == null) return;
            _panel.AddLine($"L{last.Lap}", $"Min - Max  -  Avg");

            string[] tyres = ["FL", "FR", "RL", "RR"];
            for (int i = 0; i < 4; i++)
            {
                _panel.AddLine(tyres[i], $"{last.Min[i]:F1} - {last.Max[i]:F1} - {last.Averages[i]:F1}");
            }
        }
        _panel.Draw(g);
    }
}
