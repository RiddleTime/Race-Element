using RaceElement.Data.Common;
using RaceElement.Data.Games;
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
using static RaceElement.HUD.Overlay.Configuration.OverlaySettings;

namespace RaceElement.HUD.Overlay.Internal;

public abstract class CommonAbstractOverlay : FloatingWindow
{
    public abstract void Render(Graphics g);
    public virtual void BeforeStart() { }
    public virtual void BeforeStop() { }
    public virtual bool ShouldRender() => DefaultShouldRender();


    protected CommonAbstractOverlay(Rectangle rectangle, string Name)
    {
        this.X = rectangle.X;
        this.Y = rectangle.Y;
        this.Width = rectangle.Width;
        this.Height = rectangle.Height;
        this.Name = Name;
        GameWhenStarted = GameManager.CurrentGame;

        try
        {
            if (AllowReposition)
                ApplyOverlaySettings();

            LoadFieldConfig();
        }
        catch (Exception) { }
    }

    public Game GameWhenStarted { get; private set; } = Game.Any;

    private volatile bool Draw = false;

    public bool IsRepositioning { get; internal set; }

    public bool IsPreviewing { get; set; } = false;

    public double RefreshRateHz = 30;

    public bool RequestsDrawItself = false;

    public bool AllowReposition { get; set; } = true;

    public float Scale { get; private set; } = 1f;
    private bool _allowRescale = false;

    public virtual void SetupPreviewData()
    {
    }

    /// <summary>
    /// TODO: Seal in future and decouple.
    /// </summary>
    /// <returns></returns>
    public virtual bool DefaultShouldRender()
    {
        // always visible or repositioning mode
        if (HudSettings.Cached.DemoMode || IsRepositioning) return true;

        // sim data modes
        bool condition = false;
        if (SimDataProvider.LocalCar.Engine.IsRunning)
            condition = true;

        if (GameWhenStarted.HasFlag(Game.RaceRoom))   // TODO: map these conditions for other simulators
        {
            if (SimDataProvider.GameData.IsGamePaused)
                condition = false;
        }

        return condition;
    }

    private void LoadFieldConfig()
    {
        FieldInfo[] fields = this.GetType().GetRuntimeFields().ToArray();
        foreach (var nested in fields)
        {
            if (nested.FieldType.BaseType == typeof(OverlayConfiguration))
            {
                var overlayConfig = (OverlayConfiguration)Activator.CreateInstance(nested.FieldType, []);

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

                if (overlayConfig.GenericConfiguration.AllowRescale)
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

    /// <summary>
    /// TODO refactor?? what the heck was this?
    /// </summary>
    private bool hasClosed = true;
    public virtual void Start(bool addTrackers = true)
    {
        try
        {
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
                renderThread.IsBackground = true;
                renderThread.SetApartmentState(ApartmentState.MTA);
                renderThread.Start();
            }
        }
        catch (Exception ex) { Debug.WriteLine(ex); }
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


    public void Stop()
    {
        try
        {
            BeforeStop();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            LogWriter.WriteToLog(ex);
        }


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
                    {
                        using SolidBrush backgroundBrush = new(Color.FromArgb(95, Color.LimeGreen));
                        e.Graphics.FillRectangle(backgroundBrush, new Rectangle(0, 0, Width, Height));
                    }

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
                    OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(this.Name, GameWhenStarted);
                    if (settings == null)
                        return;
                    Point point = Monitors.IsInsideMonitor(X, Y, this.Size.Width, this.Size.Height, this.Handle);
                    settings.X = point.X;
                    settings.Y = point.Y;

                    OverlaySettings.SaveOverlaySettings(this.Name, settings, GameWhenStarted);

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
