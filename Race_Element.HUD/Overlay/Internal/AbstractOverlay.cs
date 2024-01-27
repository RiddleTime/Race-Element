using RaceElement.Broadcast.Structs;
using RaceElement.Data.ACC.Core;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.Util;
using RaceElement.Util.Settings;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using static RaceElement.ACCSharedMemory;
using static RaceElement.HUD.Overlay.Configuration.OverlaySettings;

namespace RaceElement.HUD.Overlay.Internal;

public abstract class AbstractOverlay : FloatingWindow
{
    public abstract void Render(Graphics g);
    public virtual void BeforeStart() { }
    public virtual void BeforeStop() { }
    public virtual bool ShouldRender() => DefaultShouldRender();

    protected AbstractOverlay(Rectangle rectangle, string Name)
    {
        this.X = rectangle.X;
        this.Y = rectangle.Y;
        this.Width = rectangle.Width;
        this.Height = rectangle.Height;
        this.Name = Name;

        try
        {
            if (AllowReposition)
                ApplyOverlaySettings();

            LoadFieldConfig();
        }
        catch (Exception) { }
    }

    private bool Draw = false;

    public bool IsRepositioning { get; internal set; }

    public double RefreshRateHz = 30;

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



    public virtual void SetupPreviewData()
    {
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
            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || !pagePhysics.IgnitionOn)
                shouldRender = false;

            if (pageGraphics.GlobalRed)
                shouldRender = false;

            if (RaceSessionState.IsFormationLap(pageGraphics.GlobalRed, broadCastRealTime.Phase))
                shouldRender = true;

            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_REPLAY)
                shouldRender = false;

            if (broadCastRealTime.FocusedCarIndex != pageGraphics.PlayerCarID)
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

                try
                {
                    overlayConfig.SetConfigFields(savedSettings.Config);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                nested.SetValue(this, overlayConfig);

                if (overlayConfig.AllowRescale)
                {
                    this._allowRescale = true;
                    this.Scale = overlayConfig.GenericConfiguration.Scale;
                }

                this.Alpha = (byte)(255 * overlayConfig.GenericConfiguration.Opacity);

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
                PageStaticTracker.Tracker += PageStaticChanged;
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
                Thread renderThread = new(() =>
                  {
                      double tickRefreshRate = Math.Ceiling(1000 / this.RefreshRateHz);
                      Stopwatch stopwatch = Stopwatch.StartNew();

                      while (Draw)
                      {
                          stopwatch.Restart();

                          if (this._disposed)
                          {
                              this.Stop();
                              break;
                          }


                          if (ShouldRender() || IsRepositioning)
                              this.UpdateLayeredWindow();
                          else
                          {
                              if (!hasClosed)
                              {
                                  hasClosed = true;
                                  if (WindowMode) // Don't destroy the handle of this window since some stream/vr apps cannot "redetect" the hud window.
                                      this.UpdateLayeredWindow();
                                  else
                                      this.Hide();
                              }
                          }

                          int millisToWait = (int)Math.Floor(tickRefreshRate - stopwatch.ElapsedMilliseconds - 0.05);
                          if (millisToWait > 0)
                              Thread.Sleep(millisToWait);
                      }

                  });
                renderThread.SetApartmentState(ApartmentState.MTA);
                renderThread.Start();
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

        try
        {
            BeforeStop();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            LogWriter.WriteToLog(ex);
        }
        PageStaticTracker.Tracker -= PageStaticChanged;
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
                        e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(95, Color.LimeGreen)), new Rectangle(0, 0, Width, Height));

                    if (_allowRescale && Scale != 1f)
                        e.Graphics.ScaleTransform(Scale, Scale);

                    Render(e.Graphics);
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
                this.IsRepositioning = enabled;

                if (enabled)
                    this.SetDraggy(true);
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
