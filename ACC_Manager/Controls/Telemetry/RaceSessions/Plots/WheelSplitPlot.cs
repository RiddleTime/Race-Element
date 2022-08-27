using ACC_Manager.Util.SystemExtensions;
using ACCManager.Data.ACC.Database.Telemetry;
using ACCManager.Data.ACC.Tracks;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.Data.SetupConverter;
using System.Windows.Controls;
using System.Windows.Input;
using static ACCManager.Data.ACC.Tracks.TrackNames;

namespace ACCManager.Controls.Telemetry.RaceSessions.Plots
{
    internal class WheelSplitPlot
    {
        private readonly TextBlock _textBlockMetrics;
        private readonly TrackData _trackData;

        public WheelSplitPlot(TrackData trackData, ref TextBlock textBlockMetrics)
        {
            _trackData = trackData;
            _textBlockMetrics = textBlockMetrics;
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

            Plot plot = wpfPlot.Plot;
            plot.Palette = PlotUtil.WheelPositionPallete;
            plot.Benchmark(false);

            double[][] wheelSlips = new double[4][];
            double minTemp = int.MaxValue;
            double maxTemp = int.MinValue;
            double[] splines = dict.Select(x => (double)x.Value.SplinePosition * _trackData.TrackLength).ToArray();

            if (splines.Length == 0)
                return wpfPlot;

            if (dict.First().Value.PhysicsData == null || dict.First().Value.PhysicsData.WheelSlip == null)
                return wpfPlot;

            double[] averageWheelSlips = dict.Select(x =>
            {
                float[] wheelSlip = x.Value.PhysicsData.WheelSlip;

                float slipRatioFront = (wheelSlip[(int)Wheel.FrontLeft] + wheelSlip[(int)Wheel.FrontRight]) / 2;
                float slipRatioRear = (wheelSlip[(int)Wheel.RearLeft] + wheelSlip[(int)Wheel.RearRight]) / 2;

                double diff = 0;
                // understeer
                if (slipRatioFront > slipRatioRear)
                    diff = slipRatioFront - slipRatioRear;

                // oversteer
                if (slipRatioRear > slipRatioFront)
                    diff = (slipRatioRear - slipRatioFront) * -1;

                return diff;
            }).ToArray();

            minTemp = averageWheelSlips.Min();
            maxTemp = averageWheelSlips.Max();

            var wheelSlipPlot = plot.AddSignalXY(splines, averageWheelSlips, label: "US-OS", color: System.Drawing.Color.White);
            wheelSlipPlot.FillAboveAndBelow(System.Drawing.Color.Blue, System.Drawing.Color.Red);

            string fourSpaces = "".FillEnd(4, ' ');
            double averageSlip = averageWheelSlips.Average();
            _textBlockMetrics.Text += $"Av. {averageSlip:F3}";
            _textBlockMetrics.Text += $"{fourSpaces} (Understeer is a positive value, Oversteer is a negative value.";


            double padding = 2;
            plot.SetAxisLimitsX(xMin: 0, xMax: _trackData.TrackLength);
            plot.SetAxisLimitsY(minTemp - padding, maxTemp + padding);
            plot.SetOuterViewLimits(0, _trackData.TrackLength, minTemp - padding, maxTemp + padding);
            plot.XLabel("Meters");
            plot.YLabel("Slip Angle");
            PlotUtil.SetDefaultPlotStyles(ref plot);

            wpfPlot.RenderRequest();

            return wpfPlot;
        }
    }
}
