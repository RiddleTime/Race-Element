using ACCManager.Data.ACC.Tracks;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Styles;
using System;

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
            plot.Configuration.Quality = ScottPlot.Control.QualityMode.LowWhileDragging;
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

        public static AxisLimits AxisLimits;
        public static bool AxisLimitsCustom { get; internal set; } = false;
        public static event EventHandler<AxisLimits> AxisLimitsChanged;

        public static void WpfPlot_AxesChanged(object sender, EventArgs e)
        {
            WpfPlot wpfPlot = (WpfPlot)sender;
            AxisLimits = wpfPlot.Plot.GetAxisLimits(xAxisIndex: 0);
            AxisLimitsCustom = true;
            AxisLimitsChanged?.Invoke(null, AxisLimits);
            //Debug.WriteLine($"XMin: {AxisLimits.XMin}, XMax: {AxisLimits.XMax}, XCenter: {AxisLimits.XCenter}");
        }

        internal static TrackNames.TrackData trackData;

        internal static event EventHandler<int> MarkerIndexChanged;
        private static int _markerIndex = -1;
        internal static int MarkerIndex
        {
            get { return _markerIndex; }
            set
            {
                _markerIndex = value;
                MarkerIndexChanged?.Invoke(null, value);
            }
        }


    }
}
