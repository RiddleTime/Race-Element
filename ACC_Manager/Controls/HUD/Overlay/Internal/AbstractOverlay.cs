using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using static ACCSetupApp.ACCSharedMemory;
using static ACCSetupApp.Controls.HUD.Overlay.Internal.OverlayOptions;

namespace ACCSetupApp.Controls.HUD.Overlay.Internal
{
    internal abstract class AbstractOverlay : FloatingWindow
    {
        public string Name { get; private set; }
        private bool Draw = false;

        public bool IsRepositioning { get; internal set; }
        public bool AllowReposition { get; set; } = true;

        public int RefreshRateHz = 30;

        private Window RepositionWindow;

        internal SPageFilePhysics pagePhysics;
        internal SPageFileGraphic pageGraphics;
        internal SPageFileStatic pageStatic;

        internal int ScreenWidth => (int)System.Windows.SystemParameters.PrimaryScreenWidth;
        internal int ScreenHeight => (int)System.Windows.SystemParameters.PrimaryScreenHeight;

        internal bool RequestsDrawItself = false;

        protected AbstractOverlay(Rectangle rectangle, string Name)
        {
            this.X = rectangle.X;
            this.Y = rectangle.Y;
            this.Width = rectangle.Width;
            this.Height = rectangle.Height;
            this.Alpha = 255;
            this.Name = Name;

            if (AllowReposition)
                ApplyOverlaySettings();
        }

        private void ApplyOverlaySettings()
        {
            OverlaySettings settings = OverlayOptions.LoadOverlaySettings(this.Name);
            if (settings != null)
            {
                this.X = settings.X;
                this.Y = settings.Y;
            }
        }

        public abstract void BeforeStart();
        public void Start()
        {
            try
            {
                PageStaticTracker.Instance.Tracker += PageStaticChanged;
                PageGraphicsTracker.Instance.Tracker += PageGraphicsChanged;
                PagePhysicsTracker.Instance.Tracker += PagePhysicsChanged;
                ACCSharedMemory mem = new ACCSharedMemory();

                pageStatic = mem.ReadStaticPageFile();
                pageGraphics = mem.ReadGraphicsPageFile();
                pagePhysics = mem.ReadPhysicsPageFile();

                BeforeStart();
                Draw = true;
                this.Show();


                new Thread(x =>
                {
                    while (Draw)
                    {
                        Thread.Sleep(1000 / RefreshRateHz);
                        if (this._disposed)
                        {
                            this.Stop();
                            return;
                        }

                        if (!RequestsDrawItself)
                            this.UpdateLayeredWindow();
                    }

                    this.Stop();
                }).Start();
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        internal void RequestRedraw()
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

        public abstract void BeforeStop();
        public void Stop()
        {
            this.EnableReposition(false);
            BeforeStop();
            PageStaticTracker.Instance.Tracker -= PageStaticChanged;
            PageGraphicsTracker.Instance.Tracker -= PageGraphicsChanged;
            PagePhysicsTracker.Instance.Tracker -= PagePhysicsChanged;

            Draw = false;
            this.Close();
            this.Dispose();
        }

        public abstract bool ShouldRender();
        public abstract void Render(Graphics g);

        protected sealed override void PerformPaint(PaintEventArgs e)
        {
            if (base.Handle == IntPtr.Zero)
                return;

            if (Draw)
            {
                if (ShouldRender())
                {
                    Render(e.Graphics);
                }
            }
        }

        public void EnableReposition(bool enabled)
        {
            if (!AllowReposition)
                return;

            this.IsRepositioning = enabled;

            if (enabled)
            {
                this.RepositionWindow = new Window()
                {
                    Width = this.Width,
                    Height = this.Height,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    Left = X,
                    Top = Y,
                    Title = this.Name,
                    Topmost = true,
                    BorderBrush = System.Windows.Media.Brushes.Red,
                    BorderThickness = new Thickness(3),
                    ShowInTaskbar = false,
                    AllowsTransparency = true,
                    Opacity = 0.3
                };
                this.RepositionWindow.MouseLeftButtonDown += (s, e) =>
                {
                    this.RepositionWindow.DragMove();
                };

                this.RepositionWindow.LocationChanged += (s, e) =>
                {
                    X = (int)this.RepositionWindow.Left;
                    Y = (int)this.RepositionWindow.Top;
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
                                this.RepositionWindow.Hide();
                                this.RepositionWindow.Close();
                                this.RepositionWindow = null;
                            }
                        }
                        catch (Exception ex) { Debug.WriteLine(ex); }
                    }));
                }



                OverlaySettings settings = OverlayOptions.LoadOverlaySettings(this.Name);
                settings.X = X;
                settings.Y = Y;

                OverlayOptions.SaveOverlaySettings(this.Name, settings);
            }
        }
    }
}
