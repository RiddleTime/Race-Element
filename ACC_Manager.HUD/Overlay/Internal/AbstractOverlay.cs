using ACC_Manager.Util.Settings;
using ACC_Manager.Util.SystemExtensions;
using ACCManager.Broadcast.Structs;
using ACCManager.Data.ACC.Session;
using ACCManager.Data.ACC.Tracker;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.Util;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using static ACCManager.ACCSharedMemory;
using static ACCManager.HUD.Overlay.Configuration.OverlaySettings;

namespace ACCManager.HUD.Overlay.Internal
{
    public abstract class AbstractOverlay : FloatingWindow
    {
        public abstract void BeforeStart();
        public abstract void BeforeStop();
        public abstract bool ShouldRender();
        public abstract void Render(Graphics g);

        private bool Draw = false;

        public bool IsRepositioning { get; internal set; }

        public int RefreshRateHz = 30;

        public SPageFilePhysics pagePhysics;
        public SPageFileGraphic pageGraphics;
        public SPageFileStatic pageStatic;
        public RealtimeUpdate broadCastRealTime;
        public TrackData broadCastTrackData;
        public RealtimeCarUpdate broadCastLocalCar;


        public int ScreenWidth => (int)SystemParameters.PrimaryScreenWidth;
        public int ScreenHeight => (int)SystemParameters.PrimaryScreenHeight;

        public bool RequestsDrawItself = false;

        public bool AllowReposition { get; set; } = true;

        private float _scale = 1f;
        public float Scale { get { return _scale; } }
        private bool _allowRescale = false;

        private Window RepositionWindow;


        protected AbstractOverlay(Rectangle rectangle, string Name)
        {
            this.X = rectangle.X;
            this.Y = rectangle.Y;
            this.Width = rectangle.Width;
            this.Height = rectangle.Height;
            this.Alpha = 255;
            this.Name = Name;

            try
            {
                if (AllowReposition)
                    ApplyOverlaySettings();

                LoadFieldConfig();
            }
            catch (Exception) { }
        }

        public bool DefaultShouldRender()
        {
            if (HudSettings.Cached.DemoMode)
                return true;

            bool shouldRender = true;
            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            if (!pagePhysics.IsEngineRunning)
                shouldRender = false;

            if (pageGraphics.GlobalRed)
                shouldRender = false;

            if (RaceSessionState.IsPreSession(pageGraphics.GlobalRed, broadCastRealTime.Phase))
                shouldRender = true;

            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE)
                shouldRender = false;

            if (IsRepositioning) shouldRender = true;

            return shouldRender;
        }

        private void LoadFieldConfig()
        {
            FieldInfo[] fields = this.GetType().GetRuntimeFields().ToArray();
            foreach (var nested in fields)
            {
                if (nested.FieldType.BaseType == typeof(OverlayConfiguration))
                {
                    var overlayConfig = (OverlayConfiguration)Activator.CreateInstance(nested.FieldType, new object[] { });

                    OverlaySettingsJson savedSettings = OverlaySettings.LoadOverlaySettings(this.Name);

                    if (savedSettings == null)
                        return;

                    overlayConfig.SetConfigFields(savedSettings.Config);

                    nested.SetValue(this, overlayConfig);

                    if (overlayConfig.AllowRescale)
                    {
                        this._allowRescale = true;
                        this._scale = overlayConfig.Scale;
                    }

                    if (overlayConfig.Window)
                    {
                        this.WindowMode = true;
                    }
                }
            }
        }

        private void ApplyOverlaySettings()
        {
            OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(this.Name);
            if (settings != null)
            {
                this.X = settings.X;
                this.Y = settings.Y;
            }
        }

        public void Start(bool addTrackers = true)
        {
            try
            {
                if (addTrackers)
                {
                    PageStaticTracker.Instance.Tracker += PageStaticChanged;
                    PageGraphicsTracker.Instance.Tracker += PageGraphicsChanged;
                    PagePhysicsTracker.Instance.Tracker += PagePhysicsChanged;
                    BroadcastTracker.Instance.OnRealTimeUpdate += BroadCastRealTimeChanged;
                    BroadcastTracker.Instance.OnTrackDataUpdate += BroadCastTrackDataChanged;
                    BroadcastTracker.Instance.OnRealTimeLocalCarUpdate += BroadCastRealTimeLocalCarUpdateChanged;
                }

                pageStatic = ACCSharedMemory.Instance.ReadStaticPageFile();
                pageGraphics = ACCSharedMemory.Instance.ReadGraphicsPageFile();
                pagePhysics = ACCSharedMemory.Instance.ReadPhysicsPageFile();

                try
                {
                    BeforeStart();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    LogWriter.WriteToLog(ex);
                }
                if (_allowRescale)
                {
                    this.Width = (int)Math.Ceiling(this.Width * _scale);
                    this.Height = (int)Math.Ceiling(this.Height * _scale);
                }


                Draw = true;
                this.Show();

                new Thread(x =>
                {
                    this.RefreshRateHz.Clip(1, 100);
                    while (Draw)
                    {
                        lock (this)
                        {
                            Thread.Sleep(1000 / RefreshRateHz);
                            if (this == null || this._disposed)
                            {
                                this.Stop();
                                return;
                            }

                            if (!RequestsDrawItself)
                                this.UpdateLayeredWindow();
                        }
                    }

                    this.Stop();
                }).Start();
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        private void BroadCastRealTimeLocalCarUpdateChanged(object sender, RealtimeCarUpdate e)
        {
            broadCastLocalCar = e;
        }

        private void BroadCastTrackDataChanged(object sender, TrackData e)
        {
            broadCastTrackData = e;
        }

        private void BroadCastRealTimeChanged(object sender, Broadcast.Structs.RealtimeUpdate e)
        {
            broadCastRealTime = e;
        }

        public void RequestRedraw()
        {
            this.UpdateLayeredWindow();

        }

        private void PagePhysicsChanged(object sender, SPageFilePhysics e)
        {
            pagePhysics = e;
        }

        private void PageGraphicsChanged(object sender, SPageFileGraphic e)
        {
            pageGraphics = e;
        }

        private void PageStaticChanged(object sender, SPageFileStatic e)
        {
            pageStatic = e;
        }

        public void Stop(bool animate = false)
        {
            if (animate)
                this.HideAnimate(AnimateMode.Blend | AnimateMode.ExpandCollapse, 200);

            this.EnableReposition(false);
            try
            {
                BeforeStop();
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }
            PageStaticTracker.Instance.Tracker -= PageStaticChanged;
            PageGraphicsTracker.Instance.Tracker -= PageGraphicsChanged;
            PagePhysicsTracker.Instance.Tracker -= PagePhysicsChanged;
            BroadcastTracker.Instance.OnRealTimeUpdate -= BroadCastRealTimeChanged;
            BroadcastTracker.Instance.OnTrackDataUpdate -= BroadCastTrackDataChanged;
            BroadcastTracker.Instance.OnRealTimeLocalCarUpdate -= BroadCastRealTimeLocalCarUpdateChanged;

            Draw = false;

            this.Close();
            this.Dispose();
        }

        protected sealed override void PerformPaint(PaintEventArgs e)
        {
            if (base.Handle == IntPtr.Zero)
                return;

            if (Draw)
            {
                if (ShouldRender())
                {
                    try
                    {
                        if (_allowRescale)
                            e.Graphics.ScaleTransform(_scale, _scale);

                        CompositingQuality previousComposingQuality = e.Graphics.CompositingQuality;
                        SmoothingMode previousSmoothingMode = e.Graphics.SmoothingMode;
                        TextRenderingHint previousTextRenderHint = e.Graphics.TextRenderingHint;
                        int previousTextConstrast = e.Graphics.TextContrast;

                        Render(e.Graphics);

                        e.Graphics.CompositingQuality = previousComposingQuality;
                        e.Graphics.SmoothingMode = previousSmoothingMode;
                        e.Graphics.TextRenderingHint = previousTextRenderHint;
                        e.Graphics.TextContrast = previousTextConstrast;


                        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        LogWriter.WriteToLog(ex);
                    }
                }
                else
                {
                    e.Graphics.Clear(Color.Transparent);
                }
            }
        }


        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr window, int index, int value);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr window, int index);

        public void EnableReposition(bool enabled)
        {
            try
            {
                if (!AllowReposition)
                    return;

                this.IsRepositioning = enabled;

                if (enabled)
                {
                    if (this.RepositionWindow != null)
                        return;

                    this.RepositionWindow = new Window()
                    {
                        Width = this.Width,
                        Height = this.Height,
                        WindowStyle = WindowStyle.None,
                        ResizeMode = ResizeMode.NoResize,
                        Left = X,
                        Top = Y,
                        Title = this.Name,
                        ToolTip = this.Name,
                        Topmost = true,
                        BorderBrush = System.Windows.Media.Brushes.Red,
                        BorderThickness = new Thickness(1),
                        ShowInTaskbar = false,
                        AllowsTransparency = true,
                        Opacity = 0.25,
                        Cursor = System.Windows.Input.Cursors.None
                    };


                    this.RepositionWindow.MouseLeftButtonDown += (s, e) =>
                    {
                        if (this.RepositionWindow == null)
                            return;
                        this.RepositionWindow.BorderBrush = System.Windows.Media.Brushes.Green;
                        this.RepositionWindow.BorderThickness = new Thickness(3);
                        this.RepositionWindow.DragMove();
                    };

                    this.RepositionWindow.LocationChanged += (s, e) =>
                    {
                        if (this.RepositionWindow == null)
                            return;
                        X = (int)this.RepositionWindow.Left;
                        Y = (int)this.RepositionWindow.Top;
                    };

                    this.RepositionWindow.Deactivated += (s, e) =>
                    {
                        if (this.RepositionWindow == null)
                            return;
                        this.RepositionWindow.BorderBrush = System.Windows.Media.Brushes.Red;
                        this.RepositionWindow.BorderThickness = new Thickness(1);

                    };

                    this.RepositionWindow.KeyDown += (s, e) =>
                    {
                        if (this.RepositionWindow == null)
                            return;
                        this.RepositionWindow.BorderBrush = System.Windows.Media.Brushes.Green;
                        this.RepositionWindow.BorderThickness = new Thickness(3);
                        switch (e.Key)
                        {
                            case System.Windows.Input.Key.Right:
                                {
                                    this.RepositionWindow.Left += 1;
                                    break;
                                }
                            case System.Windows.Input.Key.Left:
                                {
                                    this.RepositionWindow.Left -= 1;
                                    break;
                                }

                            case System.Windows.Input.Key.Up:
                                {
                                    this.RepositionWindow.Top -= 1;
                                    break;
                                }
                            case System.Windows.Input.Key.Down:
                                {
                                    this.RepositionWindow.Top += 1;
                                    break;
                                }
                            default: break;
                        }

                    };

                    RepositionWindow.Show();
                }
                else
                {
                    if (this.RepositionWindow != null)
                    {
                        this.RepositionWindow.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                if (this.RepositionWindow != null)
                                {
                                    this.RepositionWindow.ToolTip = null;
                                    this.RepositionWindow.Hide();
                                    this.RepositionWindow.Close();
                                    this.RepositionWindow = null;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                                LogWriter.WriteToLog(ex);
                            }
                        }));
                    }

                    OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(this.Name);
                    if (settings == null)
                        return;
                    settings.X = X;
                    settings.Y = Y;

                    OverlaySettings.SaveOverlaySettings(this.Name, settings);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                LogWriter.WriteToLog(ex);
            }
        }
    }
}
