using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.ACC.Overlays.Driving.Refuel;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.HUD.ACC.Overlays.OverlayRefuel;

#if DEBUG
[Overlay(Name = "Refuel Info", Version = 1.00,
Description = "Overlay to verify the fuel calculation during the race and help with pit stop strategy.", 
    OverlayType = OverlayType.Drive, Authors =["FG"])]
#endif
internal sealed class RefuelInfoOverlay : AbstractOverlay
{
    internal readonly OverlayRefuelConfiguration _config = new();
    

    private SolidBrush _whiteBrush = new(Color.White);
    private SolidBrush _blackBrush = new(Color.Black);

    // some widget position and size values
    private const int _windowWidth = 410;
    private const int _windowHeight = 220;
    private const int _rowHeight = 25;
    private const int _padding = 10;
    private const int _widgetMinXPos = 0 + _padding;
    private const int _widgetMaxXPos = _windowWidth - _padding;
    private const int _widgetMaxWidth = _widgetMaxXPos - _widgetMinXPos;
    private const int _progressBarHeight = 20;
    private const int _pitBarHeight = 10;

    private double _sessionLength = 0;


    public RefuelInfoOverlay(Rectangle rect) : base(rect, "Refuel Info")
    {
        this.Width = _windowWidth;
        this.Height = _windowHeight;
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

        int row = 1;
        if (this._sessionLength == 0 ) {
            this._sessionLength = pageGraphics.SessionTimeLeft;
        } 

        TextRenderingHint previousHint = g.TextRenderingHint;
        g.TextContrast = 2;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        double stintTimeLeft = pageGraphics.DriverStintTimeLeft;
        stintTimeLeft.ClipMin(-1);

        int pitWindowLength = pageStatic.PitWindowEnd - pageStatic.PitWindowStart;
        pitWindowLength.ClipMin(-1);

        double bestLapTime = pageGraphics.BestTimeMs; bestLapTime.ClipMax(180000);
        double fuelTimeLeft = pageGraphics.FuelEstimatedLaps * bestLapTime;
        double fuelToRaceEnd = ((pageGraphics.SessionTimeLeft / bestLapTime) + this._config.RefuelInfoGrouping.ExtraLaps) * pageGraphics.FuelXLap;

        StringFormat drawFormat = new();
        Font drawFont = FontUtil.FontSegoeMono(13);

        // transparent background
        g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)), new Rectangle(0, 0, _windowWidth, _windowHeight), 10);

        // display amount of laps with current fuel level
        string lapsLeft = $"Laps Left {pageGraphics.FuelEstimatedLaps:F1} @ {pageGraphics.FuelXLap:F2}L per Lap";
        g.DrawString(lapsLeft, drawFont, _whiteBrush, _padding, _rowHeight * row, drawFormat);
        row++;

        // fuel timer
        string fuelTime = $"{TimeSpan.FromMilliseconds(fuelTimeLeft):hh\\:mm\\:ss}";
        string fuelTimeString = $"Time Left {fuelTime}";
        if (stintTimeLeft != -1)
        {
            string stintTime = $" / Stint {TimeSpan.FromMilliseconds(stintTimeLeft):hh\\:mm\\:ss}";
            fuelTimeString += stintTime;
        }
        g.DrawString(fuelTimeString, drawFont, _whiteBrush, _padding, _rowHeight * row, drawFormat);

        row++;
        row++;

        // progress bar background
        DrawProgressBarBackground(g, _widgetMinXPos, _rowHeight * row, _widgetMaxWidth, this._config.RefuelInfoGrouping.SolidProgressBar);

        // race progress bar
        DrawProgressBar(g, _rowHeight * row, stintTimeLeft, bestLapTime, pitWindowLength);
        
        row++;
        row++;

        // fuel to the finsh line
        DisplayFuelToRaceEnd(g, drawFormat, drawFont, _rowHeight*row, fuelToRaceEnd);
        row++;

        // display stint fuel
        DisplayStintFuelLevel(g, drawFormat, drawFont, stintTimeLeft, bestLapTime, _rowHeight*row);

    }

    private void DisplayFuelToRaceEnd(Graphics g, StringFormat drawFormat, Font drawFont, int yPos, double fuelToRaceEnd)
    {
        // display the amount of fuel to finish the race
        string fuelToTheEndeLabel = $"Fuel to the End";
        g.DrawString(fuelToTheEndeLabel, drawFont, _whiteBrush, _padding, yPos, drawFormat);
        string fuelToTheEnd = $"{fuelToRaceEnd:F1}L";
        SolidBrush fuelBrush = new SolidBrush(Color.FromArgb(255, 0, 255, 0));
        if (fuelToRaceEnd > pageStatic.MaxFuel) fuelBrush.Color = Color.FromArgb(255, 255, 0, 0);
        g.DrawString(fuelToTheEnd, drawFont, fuelBrush, _padding + 200, yPos, drawFormat);
    }

    private void DisplayStintFuelLevel(Graphics g, StringFormat drawFormat, Font drawFont, double stintTimeLeft, double bestLapTime, int yPos)
    {

        // no stint timer set
        if (stintTimeLeft == -1) return;

        double stintFuel = (stintTimeLeft / bestLapTime) * pageGraphics.FuelXLap;
        stintFuel -= pagePhysics.Fuel;
        stintFuel += pageGraphics.FuelXLap * _config.RefuelInfoGrouping.ExtraLaps;

        string StintFuelLabel = $"Stint Fuel";
        g.DrawString(StintFuelLabel, drawFont, _whiteBrush, _padding, yPos, drawFormat);
        StintFuelLabel = $"{stintFuel:F1}L";
        g.DrawString(StintFuelLabel, drawFont, _whiteBrush, _padding + 200, yPos, drawFormat);
    }

    private void DrawProgressBar(Graphics g, int yPos, double stintTimeLeft, double bestLapTime, int pitWindowLength)
    {
        double raceProgressPercentage = GetRaceProgressPercentage();
        int raceProgressPercentagePxl = PercentageToPxl(_widgetMaxWidth, raceProgressPercentage);
        g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), new Rectangle(_widgetMinXPos, yPos, (int)raceProgressPercentagePxl, _progressBarHeight));

        // fuel level indicator on race progress bar
        double raceProgressWithFuelPercentage = 0;
        if (_sessionLength != 0)
            raceProgressWithFuelPercentage = bestLapTime * pageGraphics.FuelEstimatedLaps * 100 / _sessionLength;

        raceProgressWithFuelPercentage.Clip(0, 100);
        int raceProgressWithFuelPxl = PercentageToPxl(_widgetMaxWidth, raceProgressWithFuelPercentage);
        g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), new Rectangle(raceProgressWithFuelPxl + _padding, yPos - 10, 2, _progressBarHeight + 16));

        // earliest pit stop indicator for full tank
        double lapsWithMaxFuel = (pageStatic.MaxFuel / pageGraphics.FuelXLap) - _config.RefuelInfoGrouping.ExtraLaps;
        double drivingTimeWithMaxFuel = lapsWithMaxFuel * bestLapTime;
        double pitStopTime = _sessionLength - drivingTimeWithMaxFuel;
        double pitStopTimePercentage = RaceTimeToRacePercentage(_sessionLength - pitStopTime);
        int pitStopTimePxl = PercentageToPxl(_widgetMaxWidth, pitStopTimePercentage);
        g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 255, 0)), new Rectangle(pitStopTimePxl + _padding, yPos - 10, 2, _progressBarHeight + 16));

        // pit window
        DrawPitWindowBar(g, yPos + _progressBarHeight - _pitBarHeight, pitWindowLength);

        // stint indicator
        DrawStintIndicatorBar(g, stintTimeLeft, yPos + _progressBarHeight - _pitBarHeight);

    }

    private void DrawStintIndicatorBar(Graphics g, double stintTimeLeft, int yPos)
    {
        // no stint timer set
        if (stintTimeLeft == -1) return;

        double sessionTimeWithStintTime = pageGraphics.SessionTimeLeft - stintTimeLeft;
        double sessionTimeWithStintTimePercentage = RaceTimeToRacePercentage(_sessionLength - sessionTimeWithStintTime);
        int sessionTimeWithStintTimePxl = PercentageToPxl(_widgetMaxWidth, sessionTimeWithStintTimePercentage);

        double raceProgressPercentage = GetRaceProgressPercentage();
        int raceProgressPercentagePxl = PercentageToPxl(_widgetMaxWidth, raceProgressPercentage);

        g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 165, 0)), new Rectangle(raceProgressPercentagePxl + _padding, yPos - _progressBarHeight/4, (int)(sessionTimeWithStintTimePxl), _pitBarHeight));

    }

    private void DrawPitWindowBar(Graphics g, int yPos, int pitWindowLength)
    {
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
        
        int pitWindowStartPxl = PercentageToPxl(_widgetMaxWidth, pitWindowStartPercentage);
        pitWindowStartPxl.ClipMin(_widgetMinXPos);

        int pitWindowEndPxl = PercentageToPxl(_widgetMaxWidth, pitWindowEndPercentage);
        pitWindowEndPxl.ClipMax(_widgetMaxXPos);

        g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 255, 0)), new Rectangle(pitWindowStartPxl, yPos - _progressBarHeight / 4, (int)(pitWindowEndPxl - pitWindowStartPxl), _pitBarHeight));

    }

    private void DrawProgressBarBackground(Graphics g, int xPos, int yPos, int width, bool solid)
    {
        if (solid)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 255, 255)), new Rectangle(xPos, yPos, width, _progressBarHeight));
        }
        else
        {
            int noOfBars = width / 10;
            int barWidthPx = width / noOfBars;
            for (int i = 0; i < noOfBars; i++)
            {
                g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(100, 255, 255, 255)), new Rectangle(xPos + (i * barWidthPx), yPos, (barWidthPx / 2) + 2, _progressBarHeight), 1);
            }
        }

        // draw checkered flag at the end of the prograss bar
        DrawCheckeredFlag(g, yPos);
    }

    private void DrawCheckeredFlag(Graphics g, int yPos)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i % 2 == 0)
            {
                g.FillRectangle(_whiteBrush, new Rectangle(_widgetMaxXPos - 5, yPos + (i * 5), 5, 5));
                g.FillRectangle(_blackBrush, new Rectangle(_widgetMaxXPos - 10, yPos + (i * 5), 5, 5));
            }
            else
            {
                g.FillRectangle(_blackBrush, new Rectangle(_widgetMaxXPos - 5, yPos + (i * 5), 5, 5));
                g.FillRectangle(_whiteBrush, new Rectangle(_widgetMaxXPos - 10, yPos + (i * 5), 5, 5));
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
