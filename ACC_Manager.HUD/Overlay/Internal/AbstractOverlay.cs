using ACC_Manager.Util.Settings;
using ACCManager.Broadcast.Structs;
using ACCManager.Data.ACC.Core;
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
using System.Threading;
using System.Windows.Forms;
using static ACCManager.ACCSharedMemory;
using static ACCManager.HUD.Overlay.Configuration.OverlaySettings;
using Point = System.Drawing.Point;

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

        public bool RequestsDrawItself = false;

        public bool AllowReposition { get; set; } = true;

        public float Scale { get; private set; } = 1f;
        private bool _allowRescale = false;

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

            if (IsRepositioning)
                return true;

            if (!AccProcess.IsRunning)
                return false;

            bool shouldRender = true;

            if (pageGraphics != null)
            {
                if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                    shouldRender = false;

                if (pageGraphics.GlobalRed)
                    shouldRender = false;

                if (RaceSessionState.IsFormationLap(pageGraphics.GlobalRed, broadCastRealTime.Phase))
                    shouldRender = true;

                if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE)
                    shouldRender = false;
            }

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

                    string name = this.GetType().GetCustomAttribute<OverlayAttribute>().Name;
                    OverlaySettingsJson savedSettings = OverlaySettings.LoadOverlaySettings(name);

                    if (savedSettings == null)
                        return;

                    overlayConfig.SetConfigFields(savedSettings.Config);

                    nested.SetValue(this, overlayConfig);

                    if (overlayConfig.AllowRescale)
                    {
                        this._allowRescale = true;
                        this.Scale = overlayConfig.GenericConfiguration.Scale;
                    }

                    if (overlayConfig.GenericConfiguration.Window)
                        this.WindowMode = overlayConfig.GenericConfiguration.Window;

                    this.AlwaysOnTop = overlayConfig.GenericConfiguration.AlwaysOnTop;
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

       public bool hasClosed = true;
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

                pageStatic = ACCSharedMemory.Instance.ReadStaticPageFile(false);
                pageGraphics = ACCSharedMemory.Instance.ReadGraphicsPageFile(false);
                pagePhysics = ACCSharedMemory.Instance.ReadPhysicsPageFile(false);

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
                    this.Width = (int)Math.Ceiling(this.Width * Scale);
                    this.Height = (int)Math.Ceiling(this.Height * Scale);
                }


                Draw = true;
                this.Show();
                this.hasClosed = false;
                if (!RequestsDrawItself)
                {
                    new Thread(x =>
                    {
                        double refreshRate = 1.0 / this.RefreshRateHz;
                        DateTime lastRefreshTime = DateTime.UtcNow;

                        while (Draw)
                        {
                            DateTime nextRefreshTime = lastRefreshTime.AddSeconds(refreshRate);
                            TimeSpan waitTime = nextRefreshTime - DateTime.UtcNow;
                            lastRefreshTime = nextRefreshTime;
                            if (waitTime.Ticks > 0)
                                Thread.Sleep(waitTime);

                            if (this == null || this._disposed)
                            {
                                Debug.WriteLine("!! ----   Stop render loop");
                                this.Stop();
                                return;
                            }

                            if (ShouldRender() || IsRepositioning)
                                this.UpdateLayeredWindow();
                            else
                            {
                                if (!hasClosed)
                                {
                                    hasClosed = true;
                                    this.Hide();
                                    Debug.WriteLine("Hidden");
                                }
                            }
                        }

                        Debug.WriteLine("Render loop finished");
                        //this.Stop();
                    }).Start();
                }
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
            if (hasClosed)
            {
                this.Show();
                hasClosed = false;
            }

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

            //this.EnableReposition(false);
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
        }

        protected sealed override void PerformPaint(PaintEventArgs e)
        {
            if (base.Handle == IntPtr.Zero)
                return;
            Debug.WriteLine("rendering");
            if (Draw)
            {
                if (ShouldRender() || IsRepositioning)
                {
                    try
                    {
                        if (hasClosed)
                        {
                            this.Show();
                            hasClosed = false;
                        }

                        if (IsRepositioning)
                            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(95, Color.Red)), new Rectangle(0, 0, Width, Height));


                        if (_allowRescale)
                            e.Graphics.ScaleTransform(Scale, Scale);

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
                    if (!hasClosed)
                    {
                        e.Graphics.Clear(Color.Transparent);
                        hasClosed = true;
                    }
                }
            }
        }

        public void EnableReposition(bool enabled)
        {
            if (!AllowReposition)
                return;

            lock (this)
            {
                try
                {
                    if (enabled)
                    {
                        this.IsRepositioning = enabled;
                        this.SetDraggy(true);
                    }
                    else
                    {
                        if (base.Handle == null)
                        {
                            this.Show();
                            return;
                        }

                        // save overlay settings
                        OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(this.Name);
                        if (settings == null)
                            return;
                        Point point = Monitors.IsInsideMonitor(X, Y, this.Size.Width, this.Size.Height, this.Handle);
                        settings.X = point.X;
                        settings.Y = point.Y;

                        OverlaySettings.SaveOverlaySettings(this.Name, settings);

                        this.SetDraggy(false);
                        this.IsRepositioning = enabled;
                        UpdateLayeredWindow();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }
    }
}
