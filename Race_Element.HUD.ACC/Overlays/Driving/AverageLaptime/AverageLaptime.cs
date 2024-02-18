using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.OverlayAverageLapTime;

[Overlay(Name = "Average Laptime",
Description = "Shows last valid laps and the average lap time.",
OverlayType = OverlayType.Drive,
OverlayCategory = OverlayCategory.Lap,
Version = 1.00,
Authors = ["FG"])]
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
            [IntRange(2, 10, 1)]
            public int ValidLaps { get; init; } = 2;
        }
    }

    // the avarage of given previous laps calculated for the lap set in the key
    private Dictionary<int, int> _averageTimes;

    private const int InitialWidth = 500, InitialHeight = 750;
    private InfoTable _table;
    private int _fastestAvgTime;
    private GraphicsGrid _graphicsGrid;
    private Font _font;

    public AverageLapTimeOverlay(Rectangle rectangle) : base(rectangle, "Average Laptime")
    {
        this.Width = InitialWidth;

        _averageTimes = [];
        _fastestAvgTime = int.MaxValue;

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

        // Get the last valid laps without inlap or outlap
        var validLastLaps = LapTracker.Instance.Laps.TakeLast(validLaps)
            .TakeWhile(lap => lap.Value.IsValid && (lap.Value.LapType != Broadcast.LapType.Outlap) && (lap.Value.LapType != Broadcast.LapType.Inlap));

        int averageLapTime = 0;
        if (validLastLaps.Count() >= validLaps)
        {
            foreach (var lap in validLastLaps)
            {
                averageLapTime += lap.Value.Time;
            }
            averageLapTime = averageLapTime / validLaps;
        }

        Debug.WriteLine($"lap: {newLap.Index}, add new average lap time {averageLapTime} into dictionary");

        _averageTimes.Add(newLap.Index, averageLapTime);

        if (averageLapTime != 0 && averageLapTime < _fastestAvgTime)
        {
            _fastestAvgTime = averageLapTime;
        }

    }

    public override void Render(Graphics g)
    {

        // display fastest average lap time and fastest lap time
        string fastestAverageLapTimeValue = "--:--.----";
        string fastestLapTimeValue = "--:--.----";

        if (_fastestAvgTime != int.MaxValue)
        {
            fastestAverageLapTimeValue = MillisecondsToTimeString(_fastestAvgTime);
        }

        int fastestLapIndex = LapTracker.Instance.Laps.GetFastestLapIndex();
        if (fastestLapIndex != -1)
        {
            DbLapData bestlap = LapTracker.Instance.Laps.FirstOrDefault(lap => lap.Value.Index == fastestLapIndex).Value;
            fastestLapTimeValue = MillisecondsToTimeString(bestlap.Time);
        }

        _table.AddRow("", ["fastest lap time:      " + $"{fastestLapTimeValue}"]);
        _table.AddRow("", ["fastest " + $"{_config.InfoPanel.ValidLaps}" + " lap average: " + $"{fastestAverageLapTimeValue}"]);

        // empty row for spacing
        _table.AddRow("", ["", "", "", ""]);

        // here comes all the lap times
        _table.AddRow("", ["   ", " ", " ", "average"]); ;
        //_table.AddRow("1", [" X ", "02", "--:--.----", "--:--.----"], [Color.White, Color.White, Color.Red, Color.White]);


        int idx = 0;
        int skipIdx = 0;
        int startIdx = LapTracker.Instance.Laps.Count() - (_config.InfoPanel.ValidLaps * 3);
        foreach (var lap in LapTracker.Instance.Laps)
        {
            if (skipIdx < startIdx)
            {
                skipIdx++;
                continue;
            }

            int averageLapTime = 0;
            string averageLapTimeValue = "--:--.----";
            string lapTimeValue = MillisecondsToTimeString(lap.Value.Time);

            if (!_averageTimes.ContainsKey(lap.Value.Index))
            {
                continue;
            }

            if (_averageTimes[lap.Value.Index] != 0)
            {
                averageLapTime = _averageTimes[lap.Value.Index];
                averageLapTimeValue = MillisecondsToTimeString(averageLapTime);
            }

            // indcator for in and out lap, invalid laps get red color lap time
            string lapType = "   ";
            if (lap.Value.LapType == Broadcast.LapType.ERROR) lapType = " E ";
            if (lap.Value.LapType == Broadcast.LapType.Outlap) lapType = " O ";
            if (lap.Value.LapType == Broadcast.LapType.Inlap) lapType = " I ";

            _table.AddRow((idx + 1).ToString("D2"),
                [$"{lapType}", lap.Value.Index.ToString("D2"), $"{lapTimeValue}", $"{averageLapTimeValue}"],
                [Color.White, Color.White, lap.Value.IsValid ? Color.White : Color.Red, Color.White]);

            idx++;

        }

        var columnHeigth = (int)(Math.Ceiling(this._font.GetHeight()) + 1 * this.Scale);
        this.Height = columnHeigth * (idx + 4); // four lines at the top
        _table.Draw(g);

    }

    private string MillisecondsToTimeString(int milliseconds)
    {
        return $"{TimeSpan.FromMilliseconds(milliseconds):mm\\:ss\\.fff}";
    }
}

