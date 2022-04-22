using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static ACCSetupApp.ACCSharedMemory;

namespace ACCSetupApp.Controls.HUD.Overlay.Internal
{
    internal abstract class AbstractOverlay : FloatingWindow
    {
        private bool Draw = false;

        internal SPageFilePhysics pagePhysics;
        internal SPageFileGraphic pageGraphics;
        internal SPageFileStatic pageStatic;

        protected AbstractOverlay(Rectangle rectangle)
        {
            this.X = rectangle.X;
            this.Y = rectangle.Y;
            this.Width = rectangle.Width;
            this.Height = rectangle.Height;
            this.Alpha = 255;
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
                        Thread.Sleep(1000 / 30);
                        if (this._disposed)
                        {
                            this.Stop();
                            return;
                        }

                        this.UpdateLayeredWindow();
                    }

                    this.Stop();
                }).Start();
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
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

        protected override void PerformPaint(PaintEventArgs e)
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
    }
}
