using RaceElement.Util.SystemExtensions;
using RaceElement.Data.ACC.Database.Telemetry;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace RaceElement.Controls.Telemetry.RaceSessions.Plots
{
    internal class SpeedGearPlot
    {
        private readonly TextBlock _textBlockMetrics;
        private readonly AbstractTrackData _trackData;

        public SpeedGearPlot(AbstractTrackData trackData, ref TextBlock textBlockMetrics)
        {
            _trackData = trackData;
            _textBlockMetrics = textBlockMetrics;
        }

        internal WpfPlot Create(Grid outerGrid, Dictionary<long, TelemetryPoint> dict)
        {
            WpfPlot wpfPlot = new();

            PlotUtil.SetDefaultWpfPlotConfiguration(ref wpfPlot);

            wpfPlot.Height = outerGrid.ActualHeight;
            wpfPlot.MaxHeight = outerGrid.MaxHeight;
            wpfPlot.MinHeight = outerGrid.MinHeight;
            outerGrid.SizeChanged += (se, ev) =>
            {
                wpfPlot.Height = outerGrid.ActualHeight;
                wpfPlot.MaxHeight = outerGrid.MaxHeight;
                wpfPlot.MinHeight = outerGrid.MinHeight;
            };

            double[] splines = dict.Select(x => (double)x.Value.SplinePosition * _trackData.TrackLength).ToArray();
            if (splines.Length == 0)
                return wpfPlot;

            if (dict.First().Value.PhysicsData == null)
                return wpfPlot;


            var gears = dict.Select(x => (double)x.Value.InputsData.Gear - 1);
            double[] gearDatas = gears.ToArray();
            double maxGear = gears.Max();
            double averageGear = gears.Average();

            var speeds = dict.Select(x => (double)x.Value.PhysicsData.Speed);
            double[] speedData = speeds.ToArray();
            double minSpeed = speedData.Min();
            double maxSpeed = speedData.Max();
            double averageSpeed = speedData.Average();

            string fourSpaces = "".FillEnd(4, ' ');
            _textBlockMetrics.Text += $"{fourSpaces}Av. Speed: {averageSpeed:F3} km/h";
            _textBlockMetrics.Text += $"{fourSpaces}Min Speed: {minSpeed:F3} km/h";
            _textBlockMetrics.Text += $"{fourSpaces}Max Speed: {maxSpeed:F3} km/h";

            if (splines.Length == 0)
                return wpfPlot;

            Plot plot = wpfPlot.Plot;
            plot.SetAxisLimitsY(-5, 105);

            // gearing axis
            var gearingPlot = plot.AddScatterStep(splines, gearDatas, color: System.Drawing.Color.OrangeRed, label: "Gear");
            gearingPlot.YAxisIndex = 0;
            plot.SetAxisLimits(xMin: 0, xMax: _trackData.TrackLength, yMin: 0, yMax: 1.05 * maxGear, yAxisIndex: 0);
            plot.XAxis.SetBoundary(0, _trackData.TrackLength);
            plot.YAxis.SetBoundary(0, 1.05 * maxGear);

            // speed axis
            plot.AddAxis(Edge.Left, axisIndex: 2, title: "Speed (km/h)");
            var speedPlot = plot.AddSignalXY(splines, speedData, color: System.Drawing.Color.White, label: "Speed");
            //speedPlot.FillBelow(upperColor: System.Drawing.Color.FromArgb(95, 0, 255, 0), lowerColor: System.Drawing.Color.Transparent);
            speedPlot.YAxisIndex = 2;

            plot.XAxis2.SetBoundary(0, _trackData.TrackLength);
            plot.YAxis2.SetBoundary(-3, maxSpeed + 3);
            plot.SetAxisLimits(0, _trackData.TrackLength, -3, maxSpeed + 3, yAxisIndex: 2);

            plot.XLabel("Meters");
            plot.YLabel("Gear");

            plot.Palette = new ScottPlot.Palettes.PolarNight();

            PlotUtil.SetDefaultPlotStyles(ref plot);


            #region add markers

            MarkerPlot speedMarker = wpfPlot.Plot.AddPoint(0, 0, color: System.Drawing.Color.White);
            PlotUtil.SetDefaultMarkerStyle(ref speedMarker);

            MarkerPlot gearMarker = wpfPlot.Plot.AddPoint(0, 0, color: System.Drawing.Color.OrangeRed);
            PlotUtil.SetDefaultMarkerStyle(ref gearMarker);

            outerGrid.MouseMove += (s, e) =>
            {
                (double mouseCoordsX, _) = wpfPlot.GetMouseCoordinates();

                (double speedX, double speedY, int gasIndex) = speedPlot.GetPointNearestX(mouseCoordsX);
                speedMarker.YAxisIndex = 2;
                speedMarker.SetPoint(speedX, speedY);
                speedMarker.IsVisible = true;
                speedPlot.Label = $"Speed: {speedData[gasIndex]:F3}";

                (double gearX, double gearY, int gearIndex) = gearingPlot.GetPointNearestX(mouseCoordsX);
                gearMarker.YAxisIndex = 0;
                gearMarker.SetPoint(gearX, gearY);
                gearMarker.IsVisible = true;
                gearingPlot.Label = $"Gear: {gearDatas[gearIndex]:F0}";

                PlotUtil.MarkerIndex = gearIndex;

                wpfPlot.RenderRequest();
            };

            #endregion

            wpfPlot.AxesChanged += PlotUtil.WpfPlot_AxesChanged;
            if (PlotUtil.AxisLimitsCustom)
                plot.SetAxisLimits(xAxisIndex: 0, xMin: PlotUtil.AxisLimits.XMin, xMax: PlotUtil.AxisLimits.XMax);

            PlotUtil.AddCorners(ref wpfPlot, _trackData);

            wpfPlot.RenderRequest();

            return wpfPlot;
        }

    }
}
