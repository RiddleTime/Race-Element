using RaceElement.Util.SystemExtensions;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayFuelInfo
{
    [Overlay(Name = "Fuel Info", Version = 1.00, OverlayType = OverlayType.Release,
    Description = "A panel showing information about the fuel: laps left, fuel to end of race. Optionally showing stint information.")]
    internal sealed class FuelInfoOverlay : AbstractOverlay
    {
        private readonly InfoPanel _infoPanel;

        private readonly FuelInfoConfig _config = new FuelInfoConfig();
        private sealed class FuelInfoConfig : OverlayConfiguration
        {
            [ConfigGrouping("Info Panel", "Show or hide additional information in the panel.")]
            public InfoPanelGrouping InfoPanel { get; set; } = new InfoPanelGrouping();
            public class InfoPanelGrouping
            {
                [ToolTip("Sets the number of additional laps as a fuel buffer.")]
                [IntRange(0, 3, 1)]
                public int FuelBufferLaps { get; set; } = 0;

                [ToolTip("Displays Fuel time remaining which is green if it's higher than stint time or session time and red if it is not.")]
                public bool FuelTime { get; set; } = true;

                [ToolTip("Displays stint time remaining and the suggested amount of fuel to the end of the stint or the session.")]
                public bool StintInfo { get; set; } = true;
            }

            [ConfigGrouping("Colors", "Adjust colors for the fuel bar.")]
            public ColorsGrouping Colors { get; set; } = new ColorsGrouping();
            public class ColorsGrouping
            {
                [ToolTip("Change the color of the fuel bar when full fuel.")]
                public Color FullColor { get; set; } = Color.FromArgb(255, Color.Green);

                [ToolTip("Change the medium fuel percentage for the fuel bar to change color.")]
                [FloatRange(0.30f, 0.75f, 0.01f, 2)]
                public float MediumPercent { get; set; } = 0.5f;
                [ToolTip("Change the color of the fuel bar when medium fuel.")]
                public Color MediumColor { get; set; } = Color.FromArgb(255, 255, 135, 0);

                [ToolTip("Change the low fuel percentage for the fuel bar to change color.")]
                [FloatRange(0.01f, 0.25f, 0.01f, 2)]
                public float LowPercent { get; set; } = 0.15f;
                [ToolTip("Change the color of the fuel bar when low fuel.")]
                public Color LowColor { get; set; } = Color.FromArgb(255, Color.Red);
            }

            public FuelInfoConfig()
            {
                this.AllowRescale = true;
            }
        }

        public FuelInfoOverlay(Rectangle rectangle) : base(rectangle, "Fuel Info")
        {
            this.Width = 222;
            _infoPanel = new InfoPanel(10, this.Width - 1) { FirstRowLine = 1 };
            this.Height = this._infoPanel.FontHeight * 6 + 1;
            RefreshRateHz = 2;
        }

        public sealed override void BeforeStart()
        {
            if (!_config.InfoPanel.StintInfo)
                this.Height -= _infoPanel.FontHeight * 2;

            if (!_config.InfoPanel.FuelTime)
                this.Height -= _infoPanel.FontHeight;
        }

        public sealed override void Render(Graphics g)
        {
            // Some global variants
            double lapBufferVar = pageGraphics.FuelXLap * this._config.InfoPanel.FuelBufferLaps;
            double bestLapTime = pageGraphics.BestTimeMs; bestLapTime.ClipMax(180000);
            double fuelTimeLeft = pageGraphics.FuelEstimatedLaps * bestLapTime;
            double stintDebug = pageGraphics.DriverStintTimeLeft; stintDebug.ClipMin(-1);
            //**********************
            // Workings
            double stintFuel = pageGraphics.DriverStintTimeLeft / bestLapTime * pageGraphics.FuelXLap + pageGraphics.UsedFuelSinceRefuel;
            double fuelToEnd = pageGraphics.SessionTimeLeft / bestLapTime * pageGraphics.FuelXLap;
            double fuelToAdd = FuelToAdd(lapBufferVar, stintDebug, stintFuel, fuelToEnd);
            string fuelTime = $"{TimeSpan.FromMilliseconds(fuelTimeLeft):hh\\:mm\\:ss}";
            string stintTime = $"{TimeSpan.FromMilliseconds(stintDebug):hh\\:mm\\:ss}";
            //**********************
            Brush fuelBarBrush = GetFuelBarBrush();

            Brush fuelTimeBrush = GetFuelTimeBrush(fuelTimeLeft, stintDebug);
            //Start (Basic)
            _infoPanel.AddProgressBarWithCenteredText($"{pagePhysics.Fuel:F2} L", 0, pageStatic.MaxFuel, pagePhysics.Fuel, fuelBarBrush);
            _infoPanel.AddLine("Laps Left", $"{pageGraphics.FuelEstimatedLaps:F1} @ {pageGraphics.FuelXLap:F2}L");
            _infoPanel.AddLine("Fuel-End", $"{fuelToEnd + lapBufferVar:F1} : Add {fuelToAdd:F0}");
            //End (Basic)
            //Magic Start (Advanced)
            if (this._config.InfoPanel.FuelTime)
                _infoPanel.AddLine("Fuel Time", fuelTime, fuelTimeBrush);

            if (_config.InfoPanel.StintInfo)
            {
                _infoPanel.AddLine("Stint Time", stintTime);

                if (stintDebug == -1)
                    _infoPanel.AddLine("Stint Fuel", "No Stints");
                else
                    _infoPanel.AddLine("Stint Fuel", $"{stintFuel + lapBufferVar:F1}");
            }
            //Magic End (Advanced)
            _infoPanel.Draw(g);
        }

        private double FuelToAdd(double lapBufferVar, double stintDebug, double stintFuel, double fuelToEnd)
        {
            double fuel;
            if (stintDebug == -1)
                fuel = Math.Min(Math.Ceiling(fuelToEnd - pagePhysics.Fuel), pageStatic.MaxFuel) + lapBufferVar;
            else
                fuel = Math.Min(stintFuel - pagePhysics.Fuel, pageStatic.MaxFuel) + lapBufferVar;
            fuel.ClipMin(0);
            return fuel;
        }

        private Brush GetFuelBarBrush()
        {
            float percentage = pagePhysics.Fuel / pageStatic.MaxFuel;

            Color color = _config.Colors.FullColor;
            if (percentage <= _config.Colors.MediumPercent) color = _config.Colors.MediumColor;
            if (percentage <= _config.Colors.LowPercent) color = _config.Colors.LowColor;

            return new SolidBrush(color);
        }

        private Brush GetFuelTimeBrush(double fuelTimeLeft, double stintDebug)
        {
            Brush brush;
            if (stintDebug > -1)
                brush = fuelTimeLeft <= stintDebug ? Brushes.Red : Brushes.LimeGreen;
            else
                brush = fuelTimeLeft <= pageGraphics.SessionTimeLeft ? Brushes.Red : Brushes.LimeGreen;
            return brush;
        }
    }
}