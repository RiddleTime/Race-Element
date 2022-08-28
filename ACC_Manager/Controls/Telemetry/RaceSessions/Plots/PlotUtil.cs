using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Controls.Telemetry.RaceSessions.Plots
{
    internal static class PlotUtil
    {
        private static readonly IStyle DefaultPlotStyle = Style.Black;
        public static readonly IPalette WheelPositionPallete = Palette.OneHalfDark;

        public static void SetDefaultPlotStyles(ref Plot plot)
        {
            plot.YAxis.TickLabelStyle(color: System.Drawing.Color.White);
            plot.XAxis.TickLabelStyle(color: System.Drawing.Color.White);
            plot.YAxis2.TickLabelStyle(color: System.Drawing.Color.White);

            ScottPlot.Renderable.Legend legend = plot.Legend(true, Alignment.UpperLeft);
            legend.FillColor = System.Drawing.Color.FromArgb(160, 0, 0, 0);
            legend.FontColor = System.Drawing.Color.White;
            legend.OutlineColor = System.Drawing.Color.FromArgb(30, 255, 255, 255);
            legend.ShadowColor = System.Drawing.Color.FromArgb(30, 255, 0, 0);
            legend.FontSize = 13;
            legend.FontBold = true;
            legend.IsDetached = true;

            plot.Style(DefaultPlotStyle);
            plot.YAxis.RulerMode(true);
            plot.YAxis2.RulerMode(true);
        }

        public static void SetDefaultWpfPlotConfiguration(ref WpfPlot plot)
        {
            plot.Configuration.DoubleClickBenchmark = false;
            plot.Configuration.LockVerticalAxis = true;
            plot.Configuration.Quality = ScottPlot.Control.QualityMode.High;
            plot.Configuration.MiddleClickDragZoom = false;
            plot.Configuration.MiddleClickAutoAxis = true;
            plot.Configuration.RightClickDragZoom = false;
        }

        public static void SetPoint(this MarkerPlot marker, double x, double y)
        {
            marker.X = x;
            marker.Y = y;
        }

        public static void SetDefaultMarkerStyle(ref MarkerPlot marker)
        {
            marker.MarkerSize = 10;
            marker.MarkerLineWidth = 2;
            marker.MarkerShape = ScottPlot.MarkerShape.openCircle;
            marker.IsVisible = false;
        }
    }
}
