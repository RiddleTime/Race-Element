using RaceElement.Data.ACC.Database.Telemetry;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ACC.Tracks.TrackData;
using System.Windows.Controls;
using ScottPlot.Drawing;
using System.Windows.Media.Media3D;
using ScottPlot.Plottable;
using ScottPlot.SnapLogic;

namespace RaceElement.Controls.Telemetry.RaceSessions.Plots
{
    internal class TractionCirclePlot
    {
        private readonly TextBlock _textBlockMetrics;
        private readonly AbstractTrackData _trackData;

        public TractionCirclePlot(AbstractTrackData trackData, ref TextBlock textBlockMetrics)
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
            plot.Benchmark(false);

            if (dict.First().Value.PhysicsData.Acceleration == null)
                return wpfPlot;

            double[] lateralAcceleration = dict.Select(x => (double)x.Value.PhysicsData.Acceleration[0]).ToArray();
            double[] longAcceleration = dict.Select(x => (double)x.Value.PhysicsData.Acceleration[1]).ToArray();

            for (int i = 0; i < lateralAcceleration.Length; i++)
            {
                double x = lateralAcceleration[i];
                double y = longAcceleration[i];
                double colorFraction = Math.Sqrt(x * x + y * y) / 2.5;
                var color = Colormap.Jet.GetColor(colorFraction);

                plot.AddPoint(x, y, color, 3);
            }

            var tractionMarker = wpfPlot.Plot.AddMarkerDraggable(lateralAcceleration[0], longAcceleration[0], size: 20, color: System.Drawing.Color.OrangeRed, shape: MarkerShape.openCircle);
            tractionMarker.MarkerLineWidth = 3;
            tractionMarker.DragSnap = new Nearest2D(lateralAcceleration, longAcceleration);
            tractionMarker.Dragged += (s, e) =>
            {
                int index = new Nearest2D(lateralAcceleration, longAcceleration).SnapIndex(new Coordinate(tractionMarker.X, tractionMarker.Y));
                PlotUtil.MarkerIndex = index;

                tractionMarker.Label = $"Lat: {lateralAcceleration[index]:F3}, Long: {longAcceleration[index]:F3}";
            };
            wpfPlot.MouseLeftButtonUp += (s, e) =>
            {
                var coords = wpfPlot.GetMouseCoordinates();
                int index = new Nearest2D(lateralAcceleration, longAcceleration).SnapIndex(new Coordinate(coords.x, coords.y));
                tractionMarker.SetPoint(lateralAcceleration[index], longAcceleration[index]);

                PlotUtil.MarkerIndex = index;
                tractionMarker.Label = $"Lat: {lateralAcceleration[index]:F3}, Long: {longAcceleration[index]:F3}";
            };

            plot.XAxis.Label("Lateral Acceleration");
            plot.SetAxisLimitsX(-3, 3);

            plot.YAxis.Label("Longitudinal Acceleration");

            PlotUtil.SetDefaultPlotStyles(ref plot);

            wpfPlot.RenderRequest();

            return wpfPlot;
        }
    }
}
