using Newtonsoft.Json;
using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
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
    private GraphicsGrid _graphicsGrid;
    private Font _font;

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
        _font = FontUtil.FontSegoeMono(12f * this.Scale);
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
            .TakeWhile(lap => lap.Value.IsValid && (lap.Value.LapType != Broadcast.LapType.Outlap) && (lap.Value.LapType != Broadcast.LapType.Inlap)/*(lap.Value.LapType == Broadcast.LapType.Regular)*/);

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
        if (_averageTimes.Count < _config.InfoPanel.ValidLaps) return 0;

        int fastestTime = int.MaxValue;

        foreach(var avg in _averageTimes)
        {
            if (avg.Value < fastestTime) fastestTime = avg.Value;
        }
        return fastestTime;
    }
    public override void Render(Graphics g)
    {

        // display fastest average lap time and fastest lap time
        string fastestAverageLapTimeValue = "--:--.----";
        string fastestLapTimeValue = "--:--.----";

        int fastestAvgTime = GetFastestAvgLaptime();
        if (fastestAvgTime > 0)
        {
            fastestAverageLapTimeValue = MillisecondsToTimeString(fastestAvgTime);
        }

        int fastestLapIndex = LapTracker.Instance.Laps.GetFastestLapIndex();
        if (fastestLapIndex != -1)
        {
            DbLapData bestlap = LapTracker.Instance.Laps.FirstOrDefault(lap => lap.Value.Index == fastestLapIndex).Value;
            fastestLapTimeValue = MillisecondsToTimeString(bestlap.Time);
        }

        _table.AddRow("", ["fastest lap time:      " + $"{fastestLapTimeValue}"]);
        _table.AddRow("", ["fastest "+ $"{_config.InfoPanel.ValidLaps}"+" lap average: " + $"{fastestAverageLapTimeValue}"]);

        // empty row for spacing
        _table.AddRow("", ["", "", "", ""]);

        // here comes all the lap times
        _table.AddRow("", ["   ", " ", " ", "average"]); ;
        //_table.AddRow("1", [" X ", "02", "--:--.----", "--:--.----"], [Color.White, Color.White, Color.Red, Color.White]);
        //_table.AddRow("1", [" O ", "02--:--.------:--.----"]);

        int line = 0;
        
        foreach (var lap in LapTracker.Instance.Laps) 
        {
            string lapTimeValue = MillisecondsToTimeString(lap.Value.Time);
            string averageLapTimeValue = "--:--.----";
            if (_averageTimes.ContainsKey(lap.Value.Index))
            {
                if (_averageTimes[lap.Value.Index] != 0)
                {
                    averageLapTimeValue = MillisecondsToTimeString(_averageTimes[lap.Key]);
                }
            }
            else
            {
                // average indicator ??
            }
            
            // indcator for in and out lap, invalid laps get red color lap time
            string lapType = "   ";
            if (lap.Value.LapType == Broadcast.LapType.ERROR) lapType = " E ";
            if (lap.Value.LapType == Broadcast.LapType.Outlap) lapType = " O ";
            if (lap.Value.LapType == Broadcast.LapType.Inlap) lapType = " I ";
            
            _table.AddRow((line + 1).ToString("D2"), 
                [$"{lapType}", lap.Value.Index.ToString("D2"), $"{lapTimeValue}", $"{averageLapTimeValue}"],
                [Color.White, Color.White, lap.Value.IsValid ? Color.White : Color.Red, Color.White]);

            line++;
        }
        _table.Draw(g);

    }

    private string MillisecondsToTimeString(int milliseconds)
    {
        return $"{TimeSpan.FromMilliseconds(milliseconds):mm\\:ss\\.fff}";
    }
}

