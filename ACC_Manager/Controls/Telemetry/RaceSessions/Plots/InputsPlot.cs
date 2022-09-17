using ACC_Manager.Util.SystemExtensions;
using ACCManager.Data.ACC.Database.Telemetry;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using static ACCManager.Data.ACC.Tracks.TrackNames;

namespace ACCManager.Controls.Telemetry.RaceSessions.Plots
{
    internal class InputsPlot
    {
        private readonly TextBlock _textBlockMetrics;
        private readonly TrackData _trackData;
        private readonly int _fullSteeringLock;

        public InputsPlot(TrackData trackData, ref TextBlock textBlockMetrics, int fullSteeringLock)
        {
            _trackData = trackData;
            _textBlockMetrics = textBlockMetrics;
            _fullSteeringLock = fullSteeringLock;
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


            double[] splines = dict.Select(x => (double)x.Value.SplinePosition * _trackData.TrackLength).ToArray();
            if (splines.Length == 0)
                return wpfPlot;

            var gasses = dict.Select(x => (double)x.Value.InputsData.Gas * 100);
            double[] gasDatas = gasses.ToArray();
            double averageGas = gasses.Average();

            var brakes = dict.Select(x => (double)x.Value.InputsData.Brake * 100);
            double[] brakeDatas = brakes.ToArray();
            double averageBrakes = brakes.Average();

            double[] steeringDatas = dict.Select(x => (double)x.Value.InputsData.SteerAngle * _fullSteeringLock / 2).ToArray();
            string fourSpaces = "".FillEnd(4, ' ');
            _textBlockMetrics.Text += $"Av. Throttle: {averageGas:F2}%{fourSpaces}";
            _textBlockMetrics.Text += $"Av. Brake: {averageBrakes:F2}%{fourSpaces}";

            if (splines.Length == 0)
                return wpfPlot;

            Plot plot = wpfPlot.Plot;
            plot.SetAxisLimitsY(-5, 105);

            var axis2 = plot.AddAxis(Edge.Left, axisIndex: 2, title: "Inputs");
            plot.SetOuterViewLimits(0, _trackData.TrackLength, -3, 103, yAxisIndex: 2);

            var gasPlot = plot.AddSignalXY(splines, gasDatas, color: System.Drawing.Color.Green, label: "Throttle");
            gasPlot.FillBelow(upperColor: System.Drawing.Color.FromArgb(95, 0, 255, 0), lowerColor: System.Drawing.Color.Transparent);
            gasPlot.YAxisIndex = 2;

            var brakePlot = plot.AddSignalXY(splines, brakeDatas, color: System.Drawing.Color.Red, label: "Brake");
            brakePlot.FillBelow(upperColor: System.Drawing.Color.FromArgb(140, 255, 0, 0), lowerColor: System.Drawing.Color.Transparent);
            brakePlot.YAxisIndex = 2;

            var steeringPlot = plot.AddSignalXY(splines, steeringDatas, color: System.Drawing.Color.WhiteSmoke, label: "Steering");
            steeringPlot.YAxisIndex = 0;

            plot.SetAxisLimits(xMin: 0, xMax: _trackData.TrackLength, yMin: -1.05 * _fullSteeringLock / 2, yMax: 1.05 * _fullSteeringLock / 2, yAxisIndex: 0);
            plot.SetOuterViewLimits(0, _trackData.TrackLength, -1.05 * _fullSteeringLock / 2, 1.05 * _fullSteeringLock / 2, yAxisIndex: 0);

            plot.XLabel("Meters");
            plot.YLabel("Steering (Degrees)");

            plot.Palette = new ScottPlot.Palettes.PolarNight();

            PlotUtil.SetDefaultPlotStyles(ref plot);


            #region add markers

            MarkerPlot gasMarker = wpfPlot.Plot.AddPoint(0, 0, color: System.Drawing.Color.Green);
            PlotUtil.SetDefaultMarkerStyle(ref gasMarker);

            MarkerPlot brakeMarker = wpfPlot.Plot.AddPoint(0, 0, color: System.Drawing.Color.Red);
            PlotUtil.SetDefaultMarkerStyle(ref brakeMarker);

            MarkerPlot steeringMarker = wpfPlot.Plot.AddPoint(0, 0, color: System.Drawing.Color.WhiteSmoke);
            PlotUtil.SetDefaultMarkerStyle(ref steeringMarker);

            outerGrid.MouseMove += (s, e) =>
            {
                (double mouseCoordsX, _) = wpfPlot.GetMouseCoordinates();

                (double gasX, double gasY, int gasIndex) = gasPlot.GetPointNearestX(mouseCoordsX);
                gasMarker.YAxisIndex = 2;
                gasMarker.SetPoint(gasX, gasY);
                gasMarker.IsVisible = true;
                gasPlot.Label = $"Throttle: {gasDatas[gasIndex]:F3}";


                (double brakeX, double brakeY, int brakeIndex) = brakePlot.GetPointNearestX(mouseCoordsX);
                brakeMarker.YAxisIndex = 2;
                brakeMarker.SetPoint(brakeX, brakeY);
                brakeMarker.IsVisible = true;
                brakePlot.Label = $"Brake: {brakeDatas[brakeIndex]:F3}";


                (double steerX, double steerY, int steerIndex) = steeringPlot.GetPointNearestX(mouseCoordsX);
                steeringMarker.YAxisIndex = 0;
                steeringMarker.SetPoint(steerX, steerY);
                steeringMarker.IsVisible = true;
                steeringPlot.Label = $"Steering: {(steeringDatas[steerIndex]):F3}";

                PlotUtil.MarkerIndex = steerIndex;

                wpfPlot.RenderRequest();
            };

            #endregion

            wpfPlot.AxesChanged += PlotUtil.WpfPlot_AxesChanged;
            if (PlotUtil.AxisLimitsCustom)
                plot.SetAxisLimits(xAxisIndex: 0, xMin: PlotUtil.AxisLimits.XMin, xMax: PlotUtil.AxisLimits.XMax);


            wpfPlot.RenderRequest();

            return wpfPlot;
        }


    }
}
