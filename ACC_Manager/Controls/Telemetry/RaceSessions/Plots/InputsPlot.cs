using ACC_Manager.Util.SystemExtensions;
using ACCManager.Data.ACC.Cars;
using ACCManager.Data.ACC.Database.GameData;
using ACCManager.Data.ACC.Database.Telemetry;
using ACCManager.Data.ACC.Tracks;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
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
            WpfPlot wpfPlot = new WpfPlot
            {
                Cursor = Cursors.Hand,
            };

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

            var gasPlot = plot.AddSignalXY(splines, gasDatas, color: System.Drawing.Color.Green, label: "Throttle");
            gasPlot.FillBelow(upperColor: System.Drawing.Color.FromArgb(95, 0, 255, 0), lowerColor: System.Drawing.Color.Transparent);

            var brakePlot = plot.AddSignalXY(splines, brakeDatas, color: System.Drawing.Color.Red, label: "Brake");
            brakePlot.FillBelow(upperColor: System.Drawing.Color.FromArgb(140, 255, 0, 0), lowerColor: System.Drawing.Color.Transparent);

            var steeringPlot = plot.AddSignalXY(splines, steeringDatas, color: System.Drawing.Color.WhiteSmoke, label: "Steering");
            steeringPlot.YAxisIndex = 1;

            plot.SetAxisLimits(xMin: 0, xMax: _trackData.TrackLength, yMin: -1.05 * _fullSteeringLock / 2, yMax: 1.05 * _fullSteeringLock / 2, yAxisIndex: 1);
            plot.SetOuterViewLimits(0, _trackData.TrackLength, -3, 103);
            plot.SetOuterViewLimits(0, _trackData.TrackLength, -1.05 * _fullSteeringLock / 2, 1.05 * _fullSteeringLock / 2, yAxisIndex: 1);

            plot.XLabel("Meters");
            plot.YLabel("Inputs");

            plot.YAxis2.Ticks(true);
            plot.YAxis2.Label("Steering (Degrees)");

            plot.Palette = new ScottPlot.Palettes.PolarNight();

            PlotUtil.SetDefaultPlotStyles(ref plot);

            wpfPlot.RenderRequest();

            return wpfPlot;
        }
    }
}
