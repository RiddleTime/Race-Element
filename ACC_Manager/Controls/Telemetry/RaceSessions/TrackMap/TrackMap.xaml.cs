using ACC_Manager.Util.SystemExtensions;
using ACCManager.Controls.Telemetry.RaceSessions.Plots;
using ACCManager.Controls.Util.SetupImage;
using ACCManager.Data.ACC.Database.Telemetry;
using ACCManager.HUD.Overlay.OverlayUtil;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for TrackMap.xaml                        
    /// </summary>
    public partial class TrackMap : UserControl
    {
        private Dictionary<long, TelemetryPoint> _dict;

        private float _maxSize = 0;
        private CachedBitmap _cbDrivenCoordinates;
        private CachedBitmap _cbZoomedArea;

        private Thread markerThread;
        private int _markerIndex = -1;
        private int _nextMarkerIndex = -1;

        private int _lastX = -1, _lastY = -1;

        private float _xTranslate = 0;
        private float _yTranslate = 0;


        public TrackMap()
        {
            InitializeComponent();
            this.IsVisibleChanged += TrackMap_IsVisibleChanged;
            PlotUtil.MarkerIndexChanged += PlotUtil_MarkerIndexChanged;
            PlotUtil.AxisLimitsChanged += PlotUtil_AxisLimitsChanged;
        }

        private void PlotUtil_AxisLimitsChanged(object sender, ScottPlot.AxisLimits e)
        {
            DrawZoomedArea(ref _dict, e);
        }

        private void PlotUtil_MarkerIndexChanged(object sender, int index)
        {
            if (_markerIndex == index)
                return;

            _markerIndex = index;

            if (_cbDrivenCoordinates != null && index != -1)
            {
                if (markerThread == null || !markerThread.IsAlive)
                {
                    markerThread = new Thread(x =>
                    {
                        TelemetryPoint tmPoint = _dict.ElementAt(index).Value;

                        bool somethingWrong = false;
                        if (tmPoint.PhysicsData.X < 1 && tmPoint.PhysicsData.X >= 0)
                            somethingWrong = true;
                        if (tmPoint.PhysicsData.Y < 1 && tmPoint.PhysicsData.Y >= 0)
                            somethingWrong = true;
                        if (tmPoint.PhysicsData.X > -1 && tmPoint.PhysicsData.X <= 0)
                            somethingWrong = true;
                        if (tmPoint.PhysicsData.Y > -1 && tmPoint.PhysicsData.Y <= 0)
                            somethingWrong = true;

                        if (somethingWrong)
                            return;

                        float xPoint = (tmPoint.PhysicsData.X + _xTranslate) / _maxSize;
                        float yPoint = (tmPoint.PhysicsData.Y + _yTranslate) / _maxSize;

                        var drawPoint = new PointF(xPoint * _cbDrivenCoordinates.Width, yPoint * _cbDrivenCoordinates.Height);

                        int newX = (int)drawPoint.X;
                        int newY = (int)drawPoint.Y;

                        if (newX == _lastX && newY == _lastY)
                            return;

                        _lastX = newX;
                        _lastY = newY;


                        CachedBitmap mapWithMarker = new CachedBitmap(_cbDrivenCoordinates.Width, _cbDrivenCoordinates.Height, g =>
                        {
                            _cbDrivenCoordinates?.Draw(g);
                            _cbZoomedArea?.Draw(g);

                            GraphicsPath path = new GraphicsPath();
                            int ellipseSize = 12;
                            path.AddEllipse(drawPoint.X - ellipseSize / 2, drawPoint.Y - ellipseSize / 2, ellipseSize, ellipseSize);

                            Pen pen = new Pen(Color.OrangeRed, 2f);
                            g.DrawPath(pen, path);

                            path = new GraphicsPath();
                        });

                        this.Dispatcher.Invoke(() =>
                        {
                            image.Stretch = Stretch.UniformToFill;
                            image.Width = _cbDrivenCoordinates.Width;
                            image.Height = _cbDrivenCoordinates.Height;
                            image.Source = ImageControlCreator.CreateImage(mapWithMarker.Width, mapWithMarker.Height, mapWithMarker).Source;
                        }, System.Windows.Threading.DispatcherPriority.Send);

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
                    markerThread.Start();
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
            {
                DrawMap(ref this._dict);
            }
        }

        public void SetData(ref Dictionary<long, TelemetryPoint> dict)
        {
            this._dict = dict;
            _cbZoomedArea = null;
        }

        public void DrawMap()
        {
            if (_dict == null)
                return;

            DrawMap(ref _dict);
        }

        private void DrawZoomedArea(ref Dictionary<long, TelemetryPoint> dict, ScottPlot.AxisLimits axisLimits)
        {
            if (_cbDrivenCoordinates == null || axisLimits.XMax == PlotUtil.trackData.TrackLength && axisLimits.XMin == 0)
            {
                _cbZoomedArea = null;
                return;
            }

            PointF[] points = dict.Where(x =>
            {
                double trackPosition = PlotUtil.trackData.TrackLength * x.Value.SplinePosition;
                return trackPosition < axisLimits.XMax && trackPosition > axisLimits.XMin;
            }).Select(x => new PointF(x.Value.PhysicsData.X, x.Value.PhysicsData.Y)).ToArray();

            if (points.Length >= dict.Count || points.Length < 10)
            {
                _cbZoomedArea = null;
                return;
            }

            new Thread(() =>
            {
                _cbZoomedArea = new CachedBitmap(_cbDrivenCoordinates.Width, _cbDrivenCoordinates.Height, g =>
                {
                    if (points.Length > 0)
                    {
                        _cbDrivenCoordinates?.Draw(g);
                        points = points.Select(x => new PointF(x.X + _xTranslate, x.Y + _yTranslate)).ToArray();
                        points = points.Select(x =>
                        {
                            float xPoint = x.X / _maxSize;
                            float yPoint = x.Y / _maxSize;
                            return new PointF(xPoint * _cbDrivenCoordinates.Width, yPoint * _cbDrivenCoordinates.Height);
                        }).ToArray();
                        GraphicsPath path = new GraphicsPath(FillMode.Winding);
                        path.AddLines(points);

                        Color color = Color.Red;
                        color = Color.FromArgb(190, color.R, color.G, color.B);
                        Pen pen = new Pen(color, 3f);
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.DrawPath(pen, path);
                    }
                });

                this.Dispatcher.Invoke(() =>
                {
                    image.Stretch = Stretch.Uniform;
                    image.Width = _cbDrivenCoordinates.Width;
                    image.Height = _cbDrivenCoordinates.Height;
                    image.Source = ImageControlCreator.CreateImage(_cbDrivenCoordinates.Width, _cbDrivenCoordinates.Height, _cbZoomedArea).Source;
                }, System.Windows.Threading.DispatcherPriority.Send);
            }).Start();
        }

        private void DrawMap(ref Dictionary<long, TelemetryPoint> dict)
        {
            PointF[] points = dict.Select(x => new PointF(x.Value.PhysicsData.X, x.Value.PhysicsData.Y)).ToArray();

            Grid parent = (Grid)this.Parent;

            int width = (int)(parent.ColumnDefinitions.First().Width.Value);
            int height = width;

            _cbDrivenCoordinates = new CachedBitmap(width, height, g =>
            {
                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;
                foreach (PointF point in points)
                {
                    maxX.ClipMin(point.X);
                    minX.ClipMax(point.X);
                    maxY.ClipMin(point.Y);
                    minY.ClipMax(point.Y);
                }

                _xTranslate = 0;
                _yTranslate = 0;

                if (minX >= 0)
                    _xTranslate = minX * -1;
                else
                    _xTranslate = minX * -1;

                if (minY >= 0)
                    _yTranslate = minY * -1;
                else
                    _yTranslate = minY * -1;

                _xTranslate += 100;
                _yTranslate += 100;

                points = points.Select(x => new PointF(x.X + _xTranslate, x.Y + _yTranslate)).ToArray();
                minX = float.MaxValue; maxX = float.MinValue;
                minY = float.MaxValue; maxY = float.MinValue;
                foreach (PointF point in points)
                {
                    maxX.ClipMin(point.X);
                    minX.ClipMax(point.X);
                    maxY.ClipMin(point.Y);
                    minY.ClipMax(point.Y);
                }

                if (points.Length > 0)
                {
                    _maxSize = 0;
                    if (minX * -1 > _maxSize)
                        _maxSize = minX * -1;
                    if (maxX > _maxSize)
                        _maxSize = maxX;
                    if (minY * -1 > _maxSize)
                        _maxSize = minY * -1;
                    if (maxY > _maxSize)
                        _maxSize = maxY;

                    _maxSize *= 1.1f;

                    points = points.Select(x =>
                    {
                        float xPoint = x.X / _maxSize;
                        float yPoint = x.Y / _maxSize;
                        return new PointF(xPoint * width, yPoint * height);
                    }).ToArray();
                    GraphicsPath path = new GraphicsPath(FillMode.Winding);
                    path.AddLines(points);

                    Pen pen = new Pen(Color.White, 2f);
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.DrawPath(pen, path);
                }
            });

            image.Stretch = Stretch.Uniform;
            image.Width = _cbDrivenCoordinates.Width;
            image.Height = _cbDrivenCoordinates.Height;
            image.Source = ImageControlCreator.CreateImage(_cbDrivenCoordinates.Width, _cbDrivenCoordinates.Height, _cbDrivenCoordinates).Source;
        }
    }
}
