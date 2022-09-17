using ACC_Manager.Util.SystemExtensions;
using ACCManager.Data;
using ACCManager.Data.ACC.Database.Telemetry;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using static ACCManager.Data.ACC.Tracks.TrackNames;

namespace ACCManager.Controls.Telemetry.RaceSessions.Plots
{
    internal class BrakeTempsPlot
    {
        private readonly TextBlock _textBlockMetrics;
        private readonly TrackData _trackData;

        public BrakeTempsPlot(TrackData trackData, ref TextBlock textBlockMetrics)
        {
            _trackData = trackData;
            _textBlockMetrics = textBlockMetrics;
        }

        internal WpfPlot Create(Grid outerGrid, Dictionary<long, TelemetryPoint> dict)
        {
            WpfPlot wpfPlot = new WpfPlot();

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

            double[][] brakeTemps = new double[4][];
            double minTemp = int.MaxValue;
            double maxTemp = int.MinValue;

            double[] splines = dict.Select(x => (double)x.Value.SplinePosition * _trackData.TrackLength).ToArray();
            if (splines.Length == 0)
                return wpfPlot;

            SignalPlotXY[] brakePlots = new SignalPlotXY[4];
            string fourSpaces = "".FillEnd(4, ' ');
            for (int i = 0; i < 4; i++)
            {
                var temps = dict.Select(x => (double)x.Value.BrakeData.BrakeTemperature[i]);
                _textBlockMetrics.Text += $"Av. {Enum.GetNames(typeof(SetupConverter.Wheel))[i]}: {temps.Average():F2}{fourSpaces}";
                brakeTemps[i] = temps.ToArray();

                minTemp.ClipMax(brakeTemps[i].Min());
                maxTemp.ClipMin(brakeTemps[i].Max());

                brakePlots[i] = plot.AddSignalXY(splines, brakeTemps[i], label: Enum.GetNames(typeof(SetupConverter.Wheel))[i]);
            }

            double padding = 10;
            plot.SetAxisLimitsX(xMin: 0, xMax: _trackData.TrackLength);
            plot.SetAxisLimitsY(minTemp - padding, maxTemp + padding);
            plot.SetOuterViewLimits(0, _trackData.TrackLength, minTemp - padding, maxTemp + padding);
            plot.XLabel("Meters");
            plot.YLabel("Celsius");

            #region add markers

            MarkerPlot[] tyreMarkers = new MarkerPlot[4];
            for (int i = 0; i != tyreMarkers.Length; i++)
            {
                tyreMarkers[i] = wpfPlot.Plot.AddPoint(0, 0, color: brakePlots[i].Color);
                PlotUtil.SetDefaultMarkerStyle(ref tyreMarkers[i]);
            }

            outerGrid.MouseMove += (s, e) =>
            {
                (double mouseCoordsX, _) = wpfPlot.GetMouseCoordinates();

                for (int i = 0; i != tyreMarkers.Length; i++)
                {
                    (double x, double y, int index) = brakePlots[i].GetPointNearestX(mouseCoordsX);
                    PlotUtil.MarkerIndex = index;
                    tyreMarkers[i].SetPoint(x, y);
                    tyreMarkers[i].IsVisible = true;
                    brakePlots[i].Label = $"{Enum.GetNames(typeof(SetupConverter.Wheel))[i]}: {brakeTemps[i][index]:F3}";
                }

                wpfPlot.RenderRequest();
            };

            #endregion

            wpfPlot.AxesChanged += PlotUtil.WpfPlot_AxesChanged;
            if (PlotUtil.AxisLimitsCustom)
                plot.SetAxisLimits(xAxisIndex: 0, xMin: PlotUtil.AxisLimits.XMin, xMax: PlotUtil.AxisLimits.XMax);

            PlotUtil.SetDefaultPlotStyles(ref plot);
            wpfPlot.RenderRequest();

            return wpfPlot;
        }
    }
}
