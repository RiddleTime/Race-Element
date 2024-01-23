using Newtonsoft.Json;
using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.OverlayAverageLapTime;

#if DEBUG
[Overlay(Name = "Average Laptime", Description = "Shows last valid laps and the average lap time.", OverlayType = OverlayType.Drive, Version = 1.00)]
#endif
internal class AverageLapTimeOverlay : AbstractOverlay
{

    private readonly AverageLapTimeOverlayConfig _config = new();

    private sealed class AverageLapTimeOverlayConfig : OverlayConfiguration
    {
        [ConfigGrouping("Info Panel", "Show or hide additional information in the panel.")]
        public InfoPanelGrouping InfoPanel { get; init; } = new InfoPanelGrouping();
        public class InfoPanelGrouping
        {
            [ToolTip("Number of valid laps in a row for average time calculation.")]
            [IntRange(1, 10, 1)]
            public int ValidLaps { get; init; } = 0;
        }
    }

    // the avarage of given previous laps calculated for the lap set in the key
    private Dictionary<int, int> _averageTimes;

    private const int InitialWidth = 500, InitialHeight = 250;
    private InfoTable _table;

    public AverageLapTimeOverlay(Rectangle rectangle) : base(rectangle, "Average Laptime")
    {
        this.Width = InitialWidth;
        this.Height = InitialHeight;

        _averageTimes = [];

        // index / type / lap / time / average
        _table = new InfoTable(12, [40, 40, 150, 150]);
    }

    public override void BeforeStart()
    {
        LapTracker.Instance.LapFinished += Collector_LapFinished;
    }

    public override void BeforeStop()
    {
        LapTracker.Instance.LapFinished -= Collector_LapFinished;
    }

    private void Collector_LapFinished(object sender, DbLapData newLap)
    {

        int validLaps = _config.InfoPanel.ValidLaps;
        // get the last valid laps without inlap or outlap
        var validLastLaps = LapTracker.Instance.Laps.TakeLast(validLaps)
            .TakeWhile(lap => lap.Value.IsValid && (lap.Value.LapType == Broadcast.LapType.Regular));

        int averageLapTime = 0;
        if (validLastLaps.Count() >= validLaps)
        {
            foreach(var lap in validLastLaps)
            {
                averageLapTime += lap.Value.Time;
            }
            averageLapTime = averageLapTime / validLaps;
        }
        _averageTimes.Add(newLap.Index, averageLapTime);


    }

    private int GetFastestAvgLaptime()
    {
        int fastestTime = int.MaxValue;

        foreach(var avg in _averageTimes)
        {
            if (avg.Value < fastestTime) fastestTime = avg.Value;
        }
        return fastestTime;
    }
    public override void Render(Graphics g)
    {
        string fastestLapTimeValue = "--:--.----";
        int fastestTime = GetFastestAvgLaptime();
        if (fastestTime > 0)
        {
            TimeSpan fastestTimeTimeSpan = TimeSpan.FromMilliseconds(fastestTime);
            fastestLapTimeValue = $"{fastestTimeTimeSpan:mm\\:ss\\.fff}";
        }
        _table.AddRow("", [$"{_config.InfoPanel.ValidLaps}", " ", "fastest", $"{fastestLapTimeValue}"]);

        // empty row for spacing
        _table.AddRow("", ["", "", "", ""]);

        // here comes all the lap times
        _table.AddRow("", [" T ", "lap", "time", "average"]); ;
        //_table.AddRow("1", [" T ", "02", "--:--.----", "--:--.----"]); 

        var laps = LapTracker.Instance.Laps;
        int line = 0;
        bool isInvalidLapInList = false;
        foreach (var lap in laps) 
        {
            TimeSpan laptime = TimeSpan.FromMilliseconds(lap.Value.Time);
            string lapTimeValue = $"{laptime:mm\\:ss\\.fff}";

            string averageLapTimeValue = "--:--.----";
            if (_averageTimes.ContainsKey(lap.Key))
            {
                if (_averageTimes[lap.Key] != 0)
                {
                    TimeSpan avergeLapTimeTimeSpan = TimeSpan.FromMilliseconds(_averageTimes[lap.Key]);
                    averageLapTimeValue = $"{avergeLapTimeTimeSpan:mm\\:ss\\.fff}";
                }
            }
            

            string lapType = " V ";
            if (lap.Value.LapType == Broadcast.LapType.Outlap) lapType = " O ";
            if (lap.Value.LapType == Broadcast.LapType.Inlap) lapType = " I ";
            if (!lap.Value.IsValid) lapType = " X ";

            _table.AddRow((line + 1).ToString("D2"), [$"{lapType}", lap.Value.Index.ToString("D2"), $"{lapTimeValue}", $"{averageLapTimeValue}"]);

            if (!lap.Value.IsValid)
            {
                isInvalidLapInList = true;
            }

            // do not show all laps
            if ((isInvalidLapInList) && (line > (_config.InfoPanel.ValidLaps * 2)))
            {
                continue;
            }

            line++;
        }
        _table.Draw(g);
        

    }
}

