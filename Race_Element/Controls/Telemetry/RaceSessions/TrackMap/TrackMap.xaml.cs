using RaceElement.Controls.Telemetry.RaceSessions.Plots;
using RaceElement.Data.ACC.Database.Telemetry;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Color = System.Drawing.Color;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for TrackMap.xaml                        
    /// </summary>
    public partial class TrackMap : UserControl
    {
        private Dictionary<long, TelemetryPoint> _dict;
        private Dictionary<long, TelemetryPoint> _limitedDict;
        private ScatterPlot _scatter;
        private MarkerPlot _marker;

        private Thread markerThread;
        private int _markerIndex = -1;
        private int _nextMarkerIndex = -1;

        public TrackMap()
        {
            InitializeComponent();

            PlotUtil.SetDefaultWpfPlotConfiguration(ref wpfPlot);
            wpfPlot.Configuration.Zoom = false;
            var plot = wpfPlot.Plot;
            PlotUtil.SetDefaultPlotStyles(ref plot);
            plot.Frameless(true);
            plot.Grid(false);
            plot.AxisScaleLock(true, EqualScaleMode.ZoomOut);

            this.IsVisibleChanged += TrackMap_IsVisibleChanged;


            PlotUtil.MarkerIndexChanged += PlotUtil_MarkerIndexChanged;
            PlotUtil.AxisLimitsChanged += PlotUtil_AxisLimitsChanged;
        }

        private void PlotUtil_AxisLimitsChanged(object sender, AxisLimits axisLimits)
        {
            new Thread(() =>
            {
                lock (_dict)
                {
                    _limitedDict = _dict.Where(x =>
                    {
                        double trackPosition = PlotUtil.trackData.TrackLength * x.Value.SplinePosition;
                        int buffer = 50;
                        return trackPosition < axisLimits.XMax + buffer && trackPosition > (axisLimits.XMin - buffer);
                    }).ToDictionary(x => x.Key, x => x.Value);
                    if (_limitedDict != null)
                        lock (_limitedDict)
                        {
                            CreatePlot(ref _limitedDict);
                        }
                }
            }).Start();
        }

        private void PlotUtil_MarkerIndexChanged(object sender, int index)
        {
            if (_markerIndex == index)
                return;

            _markerIndex = index;

            if (index != -1)
            {
                if (markerThread == null || !markerThread.IsAlive)
                {
                    markerThread = new Thread(x =>
                    {
                        lock (_dict)
                        {
                            TelemetryPoint tmPoint = _dict.ElementAt(index).Value;

                            var xCoord = tmPoint.PhysicsData.X * -1;
                            var yCoord = tmPoint.PhysicsData.Y;

                            var dataPoint = _scatter.GetPointNearest(xCoord, yCoord);

                            if (_marker != null)
                                wpfPlot.Plot.Remove(_marker);

                            _marker = wpfPlot.Plot.AddPoint(xCoord, yCoord, color: Color.Red, size: 10, shape: MarkerShape.openCircle);
                            PlotUtil.SetDefaultMarkerStyle(ref _marker);
                            _marker.IsVisible = true;

                            Dispatcher.Invoke(() =>
                            {
                                wpfPlot.RenderRequest();
                            });
                        }

                        Thread.Sleep(50);

                        if (_nextMarkerIndex != -1)
                        {
                            new Thread(() =>
                            {
                                PlotUtil_MarkerIndexChanged(null, _nextMarkerIndex);
                                _nextMarkerIndex = -1;
                            }).Start();
                        }

                    });
                    try
                    {
                        if (!markerThread.IsAlive)
                            markerThread.Start();
                    }
                    catch (Exception e)
                    {

                    }
                }
                else
                {
                    _nextMarkerIndex = index;
                }
            }
        }

        private void TrackMap_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
                DrawMap();
            else
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
            }
        }

        public void SetData(ref Dictionary<long, TelemetryPoint> dict)
        {
            this._dict = dict;

            DrawMap();
        }

        public void DrawMap()
        {
            if (_dict == null)
                return;

            CreatePlot(ref _dict);
        }


        public void CreatePlot(ref Dictionary<long, TelemetryPoint> dict)
        {
            lock (dict)
            {
                var plot = wpfPlot.Plot;

                plot.Clear();

                double[] xs = dict.Select(x => (double)x.Value.PhysicsData.X * -1).ToArray();
                double[] ys = dict.Select(x => (double)x.Value.PhysicsData.Y).ToArray();

                _scatter = plot.AddScatter(xs, ys);
                _scatter.LineWidth = 1f;
                _scatter.LineColor = Color.White;
                _scatter.MarkerSize = 0;


                Dispatcher.Invoke(() =>
                {
                    wpfPlot.RenderRequest();
                });
            }
        }
    }
}
