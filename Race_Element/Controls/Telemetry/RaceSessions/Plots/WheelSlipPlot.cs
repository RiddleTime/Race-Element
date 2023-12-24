using RaceElement.Util.SystemExtensions;
using RaceElement.Data.ACC.Database.Telemetry;
using ScottPlot;
using System.Collections.Generic;
using System.Linq;
using static RaceElement.Data.SetupConverter;
using System.Windows.Controls;
using static RaceElement.Data.ACC.Tracks.TrackData;
using ScottPlot.Plottable;

namespace RaceElement.Controls.Telemetry.RaceSessions.Plots
{
    internal class WheelSlipPlot
    {
        private readonly TextBlock _textBlockMetrics;
        private readonly AbstractTrackData _trackData;

        public WheelSlipPlot(AbstractTrackData trackData, ref TextBlock textBlockMetrics)
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

            Plot plot = wpfPlot.Plot;
            plot.Palette = PlotUtil.WheelPositionPallete;
            plot.Benchmark(false);

            double[][] wheelSlips = new double[4][];

            double[] splines = dict.Select(x => (double)x.Value.SplinePosition * _trackData.TrackLength).ToArray();

            if (splines.Length == 0)
                return wpfPlot;

            if (dict.First().Value.PhysicsData == null || dict.First().Value.PhysicsData.WheelSlip == null)
                return wpfPlot;


            double[] understeers = new double[splines.Length];
            double[] oversteers = new double[splines.Length];

            for (int i = 0; i != dict.Count; i++)
            {
                float[] wheelSlip = dict.ElementAt(i).Value.PhysicsData.WheelSlip;

                float slipRatioFront = (wheelSlip[(int)Wheel.FrontLeft] + wheelSlip[(int)Wheel.FrontRight]) / 2;
                float slipRatioRear = (wheelSlip[(int)Wheel.RearLeft] + wheelSlip[(int)Wheel.RearRight]) / 2;

                // understeer
                if (slipRatioFront > slipRatioRear)
                {
                    understeers[i] = slipRatioFront - slipRatioRear;
                    oversteers[i] = 0;
                }
                else if (slipRatioRear > slipRatioFront)
                { // oversteer
                    oversteers[i] = (slipRatioRear - slipRatioFront);
                    understeers[i] = 0;
                }
                else
                {
                    oversteers[i] = 0;
                    understeers[i] = 0;
                }
            }

            var understeerPlot = plot.AddSignalXY(splines, understeers, label: "Understeer", color: System.Drawing.Color.Blue);
            understeerPlot.FillBelow(System.Drawing.Color.Blue, System.Drawing.Color.Transparent);

            var oversteerPlot = plot.AddSignalXY(splines, oversteers, label: "Oversteer", color: System.Drawing.Color.OrangeRed);
            oversteerPlot.FillBelow(System.Drawing.Color.OrangeRed, System.Drawing.Color.Transparent);

            string fourSpaces = "".FillEnd(4, ' ');
            double averageUndersteer = understeers.Average();
            double averageOversteer = oversteers.Average();
            _textBlockMetrics.Text += $"Av. Understeer {averageUndersteer:F3}";
            _textBlockMetrics.Text += $"{fourSpaces}Av. Oversteer {averageOversteer:F3}";

            double minValue = 0;
            double maxValue = int.MinValue;

            maxValue.ClipMin(understeers.Max());
            maxValue.ClipMin(oversteers.Max());

            double padding = 0.5;
            plot.SetAxisLimitsX(xMin: 0, xMax: _trackData.TrackLength);
            plot.SetAxisLimitsY(minValue - padding / 2, maxValue + padding);
            plot.XAxis.SetBoundary(0, _trackData.TrackLength);
            plot.YAxis.SetBoundary(minValue - 0.05, maxValue + padding);
            plot.XLabel("Meters");
            plot.YLabel("Slip Angle");

            #region add markers

            MarkerPlot osMarker = wpfPlot.Plot.AddPoint(0, 0, color: System.Drawing.Color.Blue);
            MarkerPlot usMarker = wpfPlot.Plot.AddPoint(0, 0, color: System.Drawing.Color.OrangeRed);
            PlotUtil.SetDefaultMarkerStyle(ref osMarker);
            PlotUtil.SetDefaultMarkerStyle(ref usMarker);

            outerGrid.MouseMove += (s, e) =>
            {
                (double mouseCoordsX, _) = wpfPlot.GetMouseCoordinates();

                (double usX, double usY, int usIndex) = oversteerPlot.GetPointNearestX(mouseCoordsX);
                usMarker.SetPoint(usX, usY);
                usMarker.IsVisible = true;
                oversteerPlot.Label = $"Oversteer: {oversteers[usIndex]:F3}";

                (double osX, double osY, int osIndex) = understeerPlot.GetPointNearestX(mouseCoordsX);
                osMarker.SetPoint(osX, osY);
                osMarker.IsVisible = true;
                understeerPlot.Label = $"Understeer: {understeers[osIndex]:F3}";

                PlotUtil.MarkerIndex = osIndex;

                wpfPlot.RenderRequest();
            };

            #endregion

            wpfPlot.AxesChanged += PlotUtil.WpfPlot_AxesChanged;
            if (PlotUtil.AxisLimitsCustom)
                plot.SetAxisLimits(xAxisIndex: 0, xMin: PlotUtil.AxisLimits.XMin, xMax: PlotUtil.AxisLimits.XMax);

            PlotUtil.SetDefaultPlotStyles(ref plot);
            PlotUtil.AddCorners(ref wpfPlot, _trackData);

            wpfPlot.RenderRequest();

            return wpfPlot;
        }
    }
}
