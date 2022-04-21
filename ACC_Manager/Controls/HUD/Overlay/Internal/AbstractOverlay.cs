using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ACCSetupApp.Controls.HUD.Overlay.Internal
{
    internal abstract class AbstractOverlay : FloatingWindow
    {
        private bool Draw = false;

        public AbstractOverlay(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.Alpha = 255;
        }

        public abstract void BeforeStart();
        public void Start()
        {
            try
            {
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

        public abstract void BeforeStop();
        public void Stop()
        {
            BeforeStop();
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
