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
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using Matrix = System.Drawing.Drawing2D.Matrix;
using Pen = System.Drawing.Pen;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for TrackMap.xaml                        
    /// </summary>
    public partial class TrackMap : UserControl
    {
        private Dictionary<long, TelemetryPoint> dict;

        private float _maxSize = 0;
        private CachedBitmap _cbDrivenCoordinates;

        private int _markerIndex = -1;
        private int _lastX = -1, _lastY = -1;

        private Thread markerThread;

        public TrackMap()
        {
            InitializeComponent();
            this.IsVisibleChanged += TrackMap_IsVisibleChanged;
            PlotUtil.MarkerIndexChanged += PlotUtil_MarkerIndexChanged;
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
                        TelemetryPoint tmPoint = dict.ElementAt(index).Value;

                        if (tmPoint.PhysicsData.X < 0.05 && tmPoint.PhysicsData.X > 0)
                            return;
                        if (tmPoint.PhysicsData.Y < 0.05 && tmPoint.PhysicsData.Y > 0)
                            return;
                        if (tmPoint.PhysicsData.X > -0.05 && tmPoint.PhysicsData.X < 0)
                            return;
                        if (tmPoint.PhysicsData.Y > -0.05 && tmPoint.PhysicsData.Y < 0)
                            return;

                        tmPoint.PhysicsData.X /= _maxSize;
                        tmPoint.PhysicsData.Y /= _maxSize;


                        int halfWidth = _cbDrivenCoordinates.Width / 2;
                        int halfHeight = _cbDrivenCoordinates.Height / 2;

                        var drawPoint = new PointF(halfWidth + tmPoint.PhysicsData.X * halfWidth, halfHeight + tmPoint.PhysicsData.Y * halfHeight);

                        int newX = (int)drawPoint.X;
                        int newY = (int)drawPoint.Y;

                        if (newX == _lastX && newY == _lastY)
                            return;

                        _lastX = newX;
                        _lastY = newY;

                        CachedBitmap mapWithMarker = new CachedBitmap(_cbDrivenCoordinates.Width, _cbDrivenCoordinates.Height, g =>
                        {
                            _cbDrivenCoordinates.Draw(g);

                            GraphicsPath path = new GraphicsPath();
                            int ellipseSize = 12;
                            path.AddEllipse(drawPoint.X - ellipseSize / 2, drawPoint.Y - ellipseSize / 2, ellipseSize, ellipseSize);

                            Matrix transformMatrix = new Matrix();
                            transformMatrix.RotateAt(-90, new PointF(halfWidth, halfHeight));
                            path.Transform(transformMatrix);

                            g.FillPath(Brushes.OrangeRed, path);

                        });

                        this.Dispatcher.Invoke(() =>
                        {
                            image.Stretch = Stretch.UniformToFill;
                            image.Width = _cbDrivenCoordinates.Width;
                            image.Height = _cbDrivenCoordinates.Height;
                            image.Source = ImageControlCreator.CreateImage(mapWithMarker.Width, mapWithMarker.Height, mapWithMarker).Source;
                        }, System.Windows.Threading.DispatcherPriority.Send);

                    });
                    markerThread.Start();
                }
            }
        }

        private void TrackMap_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                DrawMap(ref this.dict);
            }
        }

        public void SetData(ref Dictionary<long, TelemetryPoint> dict)
        {
            this.dict = dict;
        }

        private void DrawMap(ref Dictionary<long, TelemetryPoint> dict)
        {
            PointF[] points = dict.Select(x => new PointF(x.Value.PhysicsData.X, x.Value.PhysicsData.Y)).ToArray();

            Grid parent = (Grid)this.Parent;

            int width = (int)parent.ColumnDefinitions.First().Width.Value;
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

                    _maxSize *= 1.05f;

                    int halfWidth = (int)(width / 2);
                    int halfHeight = (int)(height / 2);

                    var traj = points.Select(x =>
                    {
                        x.X /= _maxSize;
                        x.Y /= _maxSize;
                        return new PointF(halfWidth + x.X * halfWidth, halfHeight + x.Y * halfHeight);
                    }).ToArray();
                    GraphicsPath path = new GraphicsPath(FillMode.Winding);
                    path.AddLines(traj);

                    Matrix transformMatrix = new Matrix();
                    transformMatrix.RotateAt(-90, new PointF(halfWidth, halfHeight));
                    path.Transform(transformMatrix);

                    Pen pen = new Pen(Color.White, 3f);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawPath(pen, path);
                }
            });

            //this.grid.Children.Clear();

            image.Stretch = Stretch.UniformToFill;
            image.Width = _cbDrivenCoordinates.Width;
            image.Height = _cbDrivenCoordinates.Height;
            image.Source = ImageControlCreator.CreateImage(_cbDrivenCoordinates.Width, _cbDrivenCoordinates.Height, _cbDrivenCoordinates).Source;
            //image.Width = 100;
            //image.Height = 100;
            //image.MaxHeight = 100;
            //image.MaxWidth = 100;

            //this.Width = 100;
            //this.Height = 100;
        }
    }
}
