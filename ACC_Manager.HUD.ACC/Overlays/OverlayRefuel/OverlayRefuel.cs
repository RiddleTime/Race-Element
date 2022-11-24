using ACC_Manager.Util.SystemExtensions;
using ACCManager.Data.ACC.Database.LapDataDB;
using ACCManager.Data.ACC.Tracker.Laps;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.ACC.Overlays.OverlayRefuel
{
#if DEBUG
    [Overlay(Name = "Refuel Info", Version = 1.00,
    Description = "Overlay to verify the fuel calculation during the race and help with pit stop strategy.", OverlayType = OverlayType.Release)]
#endif
    internal sealed class RefuelInfoOverlay : AbstractOverlay
    {

        private RefuelConfiguration config = new RefuelConfiguration();
        private class RefuelConfiguration : OverlayConfiguration
        {
            public RefuelConfiguration()
            {
                this.AllowRescale = true;
            }

            internal bool SolidProgressBar { get; set; } = false;

            [ToolTip("Amount of extra laps for fuel calculation.")]
            [IntRange(1, 5, 1)]
            public int ExtraLaps { get; set; } = 2;

        }

        private SolidBrush _whiteBrush = new SolidBrush(Color.White);
        private SolidBrush _greenBrush = new SolidBrush(Color.Green);

        private const int windowWidth = 400;
        private const int windowHeight = 100;
        private const int padding = 10;
        private const int barYPos = 30;
        private const int progressBarHeight = 20;
        private const int pitBarHeight = 5;
        private const int amountOfLapsForAverageCalculation = 3;

        private AcSessionType _lastSessionType = AcSessionType.AC_UNKNOWN;
        private float _sessionLength = 0;
        private float _pitWindowStartPercentage = 0;
        private float _pitWindowEndPercentage = 0;
        private float _raceProgressWithFuelPercentage = 0;
        private float _refuelTimeWithMaxFuelPercentage = 0;
        private float _lapsWithFuel = 0;
        private float _refuelToTheEnd = 0;

        private float _avgFuelConsumption = 0;
        private float _lastFuelConsumption = 0;

        public RefuelInfoOverlay(Rectangle rect) : base(rect, "Refuel Info")
        {
            this.Width = windowWidth;
            this.Height = windowHeight;
            this.RefreshRateHz = 5;
        }


        public sealed override void BeforeStart()
        {
            LapTracker.Instance.LapFinished += FuelHelperLapFinished;
        }

        public sealed override void BeforeStop()
        {
            LapTracker.Instance.LapFinished -= FuelHelperLapFinished;
        }

        public sealed override void Render(Graphics g)
        {

            int widgetMinXPos = 0 + padding;
            int widgetMaxXPos = windowWidth - padding;
            int widgetMaxWidth = widgetMaxXPos - widgetMinXPos;

            UpdateSessionData();

            StringFormat drawFormat = new StringFormat();
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            TextRenderingHint previousHint = g.TextRenderingHint;
            g.TextContrast = 2;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            // transparent background
            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)), new Rectangle(0, 0, windowWidth, windowHeight), 10);

            // complete progress bar
            DrawProgressBarBackground(g, widgetMinXPos, widgetMaxWidth, this.config.SolidProgressBar);

            // checkered flag
            for (int i = 0; i < 4; i++)
            {
                if (i % 2 == 0)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), new Rectangle(widgetMaxXPos - 5, barYPos + (i * 5), 5, 5));
                    g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), new Rectangle(widgetMaxXPos - 10, barYPos + (i * 5), 5, 5));
                }
                else
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), new Rectangle(widgetMaxXPos - 5, barYPos + (i * 5), 5, 5));
                    g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), new Rectangle(widgetMaxXPos - 10, barYPos + (i * 5), 5, 5));
                }

            }

            if (this._sessionLength == 0) return;

            // pit window
            int pitWindowStartPxl = PercentageToPxl(widgetMaxWidth, this._pitWindowStartPercentage);
            if (pitWindowStartPxl < widgetMinXPos) pitWindowStartPxl = widgetMinXPos;
            int pitWindowEndPxl = PercentageToPxl(widgetMaxWidth, this._pitWindowEndPercentage);
            if (pitWindowEndPxl > widgetMaxXPos) pitWindowEndPxl = widgetMaxXPos;
            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 255, 0)), new Rectangle(pitWindowStartPxl, barYPos + progressBarHeight - pitBarHeight, (int)(pitWindowEndPxl - pitWindowStartPxl), pitBarHeight));

            // race progress
            float raceProgressPercentage = GetRaceProgressPercentage();
            int raceProgressPercentagePxl = PercentageToPxl(widgetMaxWidth, raceProgressPercentage);
            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), new Rectangle(widgetMinXPos, barYPos, (int)raceProgressPercentagePxl, progressBarHeight));

            // earliest pit stop bar
            int maxFuelPx = PercentageToPxl(widgetMaxWidth, this._refuelTimeWithMaxFuelPercentage);
            maxFuelPx += widgetMinXPos;
            maxFuelPx = (maxFuelPx > widgetMaxWidth) ? widgetMaxWidth : maxFuelPx;
            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 255, 0)), new Rectangle(maxFuelPx, barYPos - 10, 2, progressBarHeight + 16));

            // latest pit stop bar
            int raceProgressWithFuelPx = PercentageToPxl(widgetMaxWidth, this._raceProgressWithFuelPercentage);
            raceProgressWithFuelPx += widgetMinXPos;
            raceProgressWithFuelPx = (raceProgressWithFuelPx > widgetMaxWidth) ? widgetMaxWidth : raceProgressWithFuelPx;
            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 0, 0)), new Rectangle(raceProgressWithFuelPx, barYPos - 10, 2, progressBarHeight + 16));

            // latest pit stop lap info
            string pitStopInfo = $"in {(int)this._lapsWithFuel} laps";
            if ((int)this._lapsWithFuel < 2)
            {
                pitStopInfo = "box THIS lap!";
            }

            // if text does not fit into the overlay, move text to the left
            Font drawFont = FontUtil.FontOrbitron(10);
            SizeF pitStopTextSize = g.MeasureString(pitStopInfo, drawFont);
            float textPosition = raceProgressWithFuelPx + 6;
            if ((textPosition + pitStopTextSize.Width) > widgetMaxXPos)
            {
                textPosition -= pitStopTextSize.Width - 5;
            }
            g.DrawString(pitStopInfo, drawFont, new SolidBrush(Color.Red), textPosition, barYPos - 20, drawFormat);

            // fuel consumption indicator
            float fuelDifference = this._avgFuelConsumption - this._lastFuelConsumption;
            string fuelConsumptionIndicator = " ";
            SolidBrush drawBrush;
            if (fuelDifference > 0.01)
            {
                drawBrush = _greenBrush;
                fuelConsumptionIndicator = "\u23F7";
            }
            else if (fuelDifference < -0.01)
            {
                drawBrush = new SolidBrush(Color.Red);
                fuelConsumptionIndicator = "\u23F6";
            }
            else
            {
                drawBrush = _whiteBrush;
                fuelConsumptionIndicator = "=";
            }

            drawFont = FontUtil.FontOrbitron(15);
            g.DrawString($"[{fuelConsumptionIndicator}] {fuelDifference.ToString("0.00")}l", drawFont, drawBrush, widgetMinXPos + 200, barYPos + pitBarHeight + 30, drawFormat);

            // refuel
            drawFont = FontUtil.FontOrbitron(15);
            g.DrawString($"Refuel:", drawFont, _whiteBrush, widgetMinXPos, barYPos + pitBarHeight + 30, drawFormat);
            if (this._refuelToTheEnd <= 0) drawBrush = _greenBrush;
            g.DrawString($"{this._refuelToTheEnd.ToString("0.0")}l ", drawFont, drawBrush, widgetMinXPos + 110, barYPos + pitBarHeight + 30, drawFormat);

            drawFont = FontUtil.FontOrbitron(8);
            g.DrawString($"{config.ExtraLaps} extra laps", drawFont, _whiteBrush, widgetMinXPos, barYPos + pitBarHeight + 50, drawFormat);

            g.TextRenderingHint = previousHint;
            g.SmoothingMode = previous;
        }

        private void FuelHelperLapFinished(object sender, DbLapData lap)
        {

            float fuelLevel = (int)(pagePhysics.Fuel);
            this._avgFuelConsumption = GetAverageFuelConsumption();
            this._lastFuelConsumption = GetLastFuelConsumption();
            int averageLapTime = GetAverageLapTime();

            this._lapsWithFuel = fuelLevel / _lastFuelConsumption;

            float sessionTimeLeft = pageGraphics.SessionTimeLeft;
            float lapsUntilTheEnd = sessionTimeLeft / averageLapTime;
            float fuelUntilTheEnd = _lastFuelConsumption * (lapsUntilTheEnd + config.ExtraLaps);
            this._refuelToTheEnd = fuelUntilTheEnd - fuelLevel;
            this._raceProgressWithFuelPercentage = ((averageLapTime * this._lapsWithFuel) * 100) / _sessionLength;
            this._raceProgressWithFuelPercentage += GetRaceProgressPercentage();


            // the time where we will make it to the end with a full tank.
            float lapsWithMaxFuel = (int)(pageStatic.MaxFuel) / _lastFuelConsumption;
            lapsWithMaxFuel -= config.ExtraLaps;
            //float lapsInSession = sessionLength / averageLapTime;
            int timeWithMaxFuel = (int)(lapsWithMaxFuel * averageLapTime);
            float refuelTimeWithMaxFuel = _sessionLength - timeWithMaxFuel;
            if (refuelTimeWithMaxFuel < 0)
            {
                refuelTimeWithMaxFuel = 0;
            }
            this._refuelTimeWithMaxFuelPercentage = RaceTimeToRacePercentage(_sessionLength - refuelTimeWithMaxFuel);

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
        }

        private float RaceTimeToRacePercentage(float raceTime)
        {
            float percentage = 100 - (raceTime * 100) / this._sessionLength;
            return percentage.ClipMax((float)100);
        }

        private float GetRaceProgressPercentage()
        {
            return RaceTimeToRacePercentage(pageGraphics.SessionTimeLeft);
        }

        private float GetLastFuelConsumption()
        {
            return pageGraphics.FuelXLap;
        }

        private int GetAverageLapTime()
        {
            return LapDataExtensions.GetAverageLapTime(LapTracker.Instance.Laps, amountOfLapsForAverageCalculation);
        }

        private float GetAverageFuelConsumption()
        {
            if (LapTracker.Instance.Laps.Count < 2)
            {
                return GetLastFuelConsumption();
            }
            return (float)(LapDataExtensions.GetAverageFuelUsage(LapTracker.Instance.Laps, amountOfLapsForAverageCalculation)) / 1000;
        }

        private int PercentageToPxl(float width, float percentage)
        {
            return (int)((width * percentage) / 100);
        }

        private void UpdateSessionData()
        {
            if (_lastSessionType != pageGraphics.SessionType)
            {
                // new session, reset
                this._sessionLength = 0;
                this._lastSessionType = pageGraphics.SessionType;
                //return;
            }

            // new session started
            if (this._sessionLength < pageGraphics.SessionTimeLeft)
            {
                // we will not get the session length after race start, save the session length.            {
                this._sessionLength = pageGraphics.SessionTimeLeft;

                // calculate pit window length
                if (pageStatic.PitWindowStart <= 0 || pageStatic.PitWindowStart >= pageStatic.PitWindowEnd || pageGraphics.SessionType != AcSessionType.AC_RACE)
                {
                    this._pitWindowStartPercentage = 0;
                    this._pitWindowEndPercentage = 0;
                }
                else
                {
                    int pitWindowLength = pageStatic.PitWindowEnd - pageStatic.PitWindowStart;
                    float pitWindowStartTime = (this._sessionLength - pitWindowLength) / 2;
                    float pitWindowEndTime = pitWindowStartTime + pitWindowLength;
                    this._pitWindowStartPercentage = (pitWindowStartTime * 100) / this._sessionLength;
                    this._pitWindowEndPercentage = (pitWindowEndTime * 100) / this._sessionLength;

                }

            }

        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }

    }



}
