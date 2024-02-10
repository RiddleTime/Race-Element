using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.HUD.ACC.Overlays.OverlayRefuel;

#if DEBUG
[Overlay(Name = "Refuel Info", Version = 1.00,
Description = "Overlay to verify the fuel calculation during the race and help with pit stop strategy.", OverlayType = OverlayType.Drive)]
#endif
internal sealed class RefuelInfoOverlay : AbstractOverlay
{
    private readonly RefuelConfiguration _config = new();
    private class RefuelConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Refuel Info", "Show or hide additional information in the panel.")]
        public InfoPanelGrouping RefuelInfoGrouping { get; init; } = new InfoPanelGrouping();
        public class InfoPanelGrouping
        {
            public bool SolidProgressBar { get; init; } = false;

            [ToolTip("Amount of extra laps for fuel calculation.")]
            [IntRange(1, 5, 1)]
            public int ExtraLaps { get; init; } = 2;
        }

        public RefuelConfiguration()
        {
            this.GenericConfiguration.AllowRescale = true;
        }
    }

    private SolidBrush _whiteBrush = new(Color.White);
    private SolidBrush _blackBrush = new(Color.Black);

    // some widget position and size values
    private const int windowWidth = 400;
    private const int windowHeight = 100;
    private const int padding = 10;
    private const int barYPos = 50;
    private const int widgetMinXPos = 0 + padding;
    private const int widgetMaxXPos = windowWidth - padding;
    private const int widgetMaxWidth = widgetMaxXPos - widgetMinXPos;
    private const int progressBarHeight = 20;
    private const int pitBarHeight = 5;

    private double _sessionLength = 0;


    public RefuelInfoOverlay(Rectangle rect) : base(rect, "Refuel Info")
    {
        this.Width = windowWidth;
        this.Height = windowHeight;
        this.RefreshRateHz = 2;
    }

    public sealed override void BeforeStart()
    {
        LapTracker.Instance.LapFinished += FuelHelperLapFinished;
        RaceSessionTracker.Instance.OnNewSessionStarted += FuelHelperNewSession;
    }

    public sealed override void BeforeStop()
    {
        LapTracker.Instance.LapFinished -= FuelHelperLapFinished;
        RaceSessionTracker.Instance.OnNewSessionStarted -= FuelHelperNewSession;
    }

    private void FuelHelperNewSession(object sender, DbRaceSession e)
    {
        Debug.WriteLine($"FuelHelperNewSession: {e.SessionType}");

        if (e.SessionType == AcSessionType.AC_RACE ||
            e.SessionType == AcSessionType.AC_QUALIFY ||
            e.SessionType == AcSessionType.AC_PRACTICE)
        {
            this._sessionLength = pageGraphics.SessionTimeLeft;
            Debug.WriteLine($"FuelHelperNewSession: {e.SessionType} set session values session length: {this._sessionLength}");
        } 
        else
        {
            Debug.WriteLine($"FuelHelperNewSession: {e.SessionType} reset session values");
            this._sessionLength = -1;
        }
 
    }

    private void FuelHelperLapFinished(object sender, DbLapData lap)
    {
        Debug.WriteLine($"FuelHelperLapFinished: {lap.Index}");
    }

    public sealed override void Render(Graphics g)
    {

        if (this._sessionLength == 0 ) {
            this._sessionLength = pageGraphics.SessionTimeLeft;
        } 

        TextRenderingHint previousHint = g.TextRenderingHint;
        g.TextContrast = 2;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        // transparent background
        g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)), new Rectangle(0, 0, windowWidth, windowHeight), 10);

        // progress bar background
        DrawProgressBarBackground(g, widgetMinXPos, widgetMaxWidth, this._config.RefuelInfoGrouping.SolidProgressBar);

        // pit window
        DrawPitWindowBar(g, barYPos + progressBarHeight - pitBarHeight);

        // stint indicator
        DrawStintIndicatorBar(g, barYPos + progressBarHeight - pitBarHeight);

        // race progress bar
        double raceProgressPercentage = GetRaceProgressPercentage();
        int raceProgressPercentagePxl = PercentageToPxl(widgetMaxWidth, raceProgressPercentage);
        g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), new Rectangle(widgetMinXPos, barYPos, (int)raceProgressPercentagePxl, progressBarHeight));

        double bestLapTime = pageGraphics.BestTimeMs; bestLapTime.ClipMax(180000);
        double lapsUntilTheEnd = pageGraphics.SessionTimeLeft / bestLapTime;
        double fuelTimeLeft = pageGraphics.FuelEstimatedLaps * bestLapTime;
        double fuelToRaceEnd = lapsUntilTheEnd * pageGraphics.FuelXLap;

        // fuel level indicator on race progress bar
        double raceProgressWithFuelPercentage = 0;
        if (_sessionLength != 0)
            raceProgressWithFuelPercentage = bestLapTime * pageGraphics.FuelEstimatedLaps * 100 / _sessionLength;
        
        raceProgressWithFuelPercentage.Clip(0, 100);
        int raceProgressWithFuelPxl = PercentageToPxl(widgetMaxWidth, raceProgressWithFuelPercentage);
        g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), new Rectangle(raceProgressWithFuelPxl + padding, barYPos - 10, 2, progressBarHeight + 16));

        // earliest pit stop indicator for full tank
        double lapsWithMaxFuel = (pageStatic.MaxFuel / pageGraphics.FuelXLap) - _config.RefuelInfoGrouping.ExtraLaps;
        double drivingTimeWithMaxFuel = lapsWithMaxFuel * bestLapTime;
        double pitStopTime = _sessionLength - drivingTimeWithMaxFuel;
        double pitStopTimePercentage = RaceTimeToRacePercentage(_sessionLength - pitStopTime);
        int pitStopTimePxl = PercentageToPxl(widgetMaxWidth, pitStopTimePercentage);
        g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 255, 0)), new Rectangle(pitStopTimePxl + padding, barYPos - 10, 2, progressBarHeight + 16));

        // display amount of laps with current fuel level
        StringFormat drawFormat = new();
        Font drawFont = FontUtil.FontSegoeMono(14);
        string lapsLeft = $"Laps Left {pageGraphics.FuelEstimatedLaps:F1} @ {pageGraphics.FuelXLap:F2}L per Lap";
        g.DrawString(lapsLeft, drawFont, _whiteBrush, padding, padding, drawFormat);

    }

    private void DrawStintIndicatorBar(Graphics g, int yPos)
    {
        double stintTimeLeft = pageGraphics.DriverStintTimeLeft;
        stintTimeLeft.ClipMin(-1);

        // no stint timer set
        if (stintTimeLeft == -1) return;

        double sessionTimeWithStintTime = pageGraphics.SessionTimeLeft - stintTimeLeft;
        double sessionTimeWithStintTimePercentage = RaceTimeToRacePercentage(_sessionLength - sessionTimeWithStintTime);
        //double sessionTimeWithStintTimePercentage = RaceTimeToRacePercentage(sessionTimeWithStintTime);
        int sessionTimeWithStintTimePxl = PercentageToPxl(widgetMaxWidth, sessionTimeWithStintTimePercentage);

        double raceProgressPercentage = GetRaceProgressPercentage();
        int raceProgressPercentagePxl = PercentageToPxl(widgetMaxWidth, raceProgressPercentage);

        g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 165, 0)), new Rectangle(raceProgressPercentagePxl + padding, yPos- pitBarHeight, (int)(sessionTimeWithStintTimePxl), pitBarHeight));

    }

    private void DrawPitWindowBar(Graphics g, int yPos)
    {
        int pitWindowLength = pageStatic.PitWindowEnd - pageStatic.PitWindowStart;
        pitWindowLength.ClipMin(-1);

        // no pitwindow set
        if (pitWindowLength == -1) return;

        double pitWindowStartTime = (this._sessionLength - pitWindowLength) / 2;
        double pitWindowEndTime = pitWindowStartTime + pitWindowLength;

        double pitWindowStartPercentage = 0;
        double pitWindowEndPercentage = 0;

        if (this._sessionLength != 0)
        {
            pitWindowStartPercentage = (pitWindowStartTime * 100) / this._sessionLength;
            pitWindowEndPercentage = (pitWindowEndTime * 100) / this._sessionLength;
        }
        
        int pitWindowStartPxl = PercentageToPxl(widgetMaxWidth, pitWindowStartPercentage);
        pitWindowStartPxl.ClipMin(widgetMinXPos);

        int pitWindowEndPxl = PercentageToPxl(widgetMaxWidth, pitWindowEndPercentage);
        pitWindowEndPxl.ClipMax(widgetMaxXPos);

        g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 255, 0)), new Rectangle(pitWindowStartPxl, yPos, (int)(pitWindowEndPxl - pitWindowStartPxl), pitBarHeight));

    }

    private void DrawProgressBarBackground(Graphics g, int xPos, int width, bool solid)
    {
        if (solid)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 255, 255)), new Rectangle(xPos, barYPos, width, progressBarHeight));
        }
        else
        {
            int noOfBars = width / 10;
            int barWidthPx = width / noOfBars;
            for (int i = 0; i < noOfBars; i++)
            {
                g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(100, 255, 255, 255)), new Rectangle(xPos + (i * barWidthPx), barYPos, (barWidthPx / 2) + 2, progressBarHeight), 1);
            }
        }

        // draw checkered flag at the end of the prograss bar
        DrawCheckeredFlag(g);
    }

    private void DrawCheckeredFlag(Graphics g)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i % 2 == 0)
            {
                g.FillRectangle(_whiteBrush, new Rectangle(widgetMaxXPos - 5, barYPos + (i * 5), 5, 5));
                g.FillRectangle(_blackBrush, new Rectangle(widgetMaxXPos - 10, barYPos + (i * 5), 5, 5));
            }
            else
            {
                g.FillRectangle(_blackBrush, new Rectangle(widgetMaxXPos - 5, barYPos + (i * 5), 5, 5));
                g.FillRectangle(_whiteBrush, new Rectangle(widgetMaxXPos - 10, barYPos + (i * 5), 5, 5));
            }

        }
    }

    private double RaceTimeToRacePercentage(double raceTime)
    {
        if (this._sessionLength == 0) return 0;
        double percentage = 100 - (raceTime * 100) / this._sessionLength;
        return percentage.Clip(0, 100f);
    }

    private double GetRaceProgressPercentage()
    {
        return RaceTimeToRacePercentage(pageGraphics.SessionTimeLeft);
    }

    private int PercentageToPxl(float width, double percentage)
    {
        return (int)((width * percentage) / 100);
    }

}
