using ACCSetupApp.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ACCSetupApp.SharedMemory;

namespace ACCSetupApp.Controls
{
    internal class TelemetryOverlay : Form
    {
        private SharedMemory sharedMemory = new SharedMemory();
        private bool drawOnGame = false;

        private Font inputFont = new Font("Arial", 16);

        private InputDataCollector inputDataCollector;

        public TelemetryOverlay()
        {
            //WindowState = FormWindowState.Maximized;
            TopLevel = true;
            TransparencyKey = System.Drawing.Color.Black;
            AllowTransparency = true;
            ShowInTaskbar = false;
            Capture = false;
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            ShowIcon = false;
            UseWaitCursor = false;
            DoubleBuffered = true;
            Width = 310;
            Height = 150;
            Location = new Point((int)System.Windows.SystemParameters.FullPrimaryScreenWidth / 2 + this.Width, 0);
            this.StartPosition = FormStartPosition.Manual;

            inputDataCollector = new InputDataCollector() { TraceCount = this.Width - 1 };
            this.Paint += Overlay_Paint;
        }

        public void Stop()
        {
            drawOnGame = false;
        }

        public void Start()
        {
            try
            {
                drawOnGame = true;
                this.Show();
                inputDataCollector.Start();
                new Thread(x =>
                {
                    while (drawOnGame)
                    {
                        Thread.Sleep(1000 / 30);
                        if (this == null || this.IsDisposed)
                        {
                            inputDataCollector.Stop();
                            return;
                        }

                        this.BeginInvoke(new Action(() =>
                        {
                            this.InvokePaint(this, new PaintEventArgs(this.CreateGraphics(), this.DisplayRectangle));
                        }));
                    }
                    this.BeginInvoke(new Action(() =>
                    {
                        this.Dispose();
                        inputDataCollector.Stop();
                    }));
                }).Start();
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }
        }
        BufferedGraphicsContext ctx = new BufferedGraphicsContext();
        private void Overlay_Paint(object sender, PaintEventArgs e)
        {
            if (drawOnGame)
            {
                BufferedGraphics bg = ctx.Allocate(Graphics.FromHwnd(this.Handle), new Rectangle(0, 0, this.Width, this.Height));
                bg.Graphics.Clear(System.Drawing.Color.Transparent);

                SPageFilePhysics pagePhysics = sharedMemory.ReadPhysicsPageFile();
                SPageFileGraphic pageGraphics = sharedMemory.ReadGraphicsPageFile();

                bool shouldRender = true;
                if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                    shouldRender = false;

                // draw here
                if (shouldRender)
                {
                    Draw(bg.Graphics, pagePhysics, pageGraphics);
                }

                // render double buffer...
                bg.Render();
                bg.Dispose();
            }
        }

        private void Draw(Graphics g, SPageFilePhysics pageFilePhysics, SPageFileGraphic pageGraphics)
        {
            DrawInputGraph(g);
        }

        private void DrawInputGraph(Graphics g)
        {
            InputGraph graph = new InputGraph(0, 0, this.Width - 1, this.Height - 1, inputDataCollector.Throttle, inputDataCollector.Brake, inputDataCollector.Steering);
            graph.Draw(g);
        }
    }
}
