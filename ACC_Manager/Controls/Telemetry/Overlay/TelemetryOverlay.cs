using System;
using System.Collections.Generic;
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
    internal class TelemetryOverlay
    {
        private SharedMemory sharedMemory = new SharedMemory();
        private bool drawOnGame = false;

        private Font inputFont = new Font("Arial", 16);

        private InputDataCollector inputDataCollector = new InputDataCollector();

        public void Stop()
        {
            drawOnGame = false;
        }

        public void Start()
        {
            new Thread(x =>
            {
                drawOnGame = true;

                Form overlay = null;
                overlay = new Form()
                {
                    WindowState = FormWindowState.Maximized,
                    TopLevel = true,
                    TransparencyKey = System.Drawing.Color.Black,
                    AllowTransparency = true,
                    ShowInTaskbar = false,
                    Capture = false,
                    TopMost = true,
                    FormBorderStyle = FormBorderStyle.None,
                    ShowIcon = false,
                    UseWaitCursor = false,
                };
                overlay.Show();
                //typeof(Form).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, overlay, new object[] { true });

                inputDataCollector.Start();

                BufferedGraphicsContext ctx = new BufferedGraphicsContext();
                while (drawOnGame)
                {
                    Thread.Sleep(1000 / 30);

                    BufferedGraphics bg = ctx.Allocate(Graphics.FromHwnd(overlay.Handle), new Rectangle(0, 0, overlay.Width, overlay.Height));
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

                if (!drawOnGame)
                {
                    overlay.Dispose();
                    inputDataCollector.Stop();
                }
            }).Start();
        }

        private void Draw(Graphics g, SPageFilePhysics pageFilePhysics, SPageFileGraphic pageGraphics)
        {
            DrawInputs(g, pageFilePhysics);
            DrawInputGraph(g);
        }

        private void DrawInputGraph(Graphics g)
        {
            RectangleF visibleArea = g.VisibleClipBounds;
            int horizontalMid = (int)(visibleArea.Width / 2);
            int visibleHeight = (int)(visibleArea.Height);

            InputGraph graph = new InputGraph(horizontalMid + 250, visibleHeight - 160, 300, 150, inputDataCollector.Throttle, inputDataCollector.Brake);
            graph.Draw(g);
        }

        private void DrawInputs(Graphics g, SPageFilePhysics pagePhysics)
        {
            RectangleF visibleArea = g.VisibleClipBounds;

            int horizontalMid = (int)(visibleArea.Width / 2);
            int visibleHeight = (int)(visibleArea.Height);

            int barWidth = 100;


            float throttle = pagePhysics.Gas;
            g.FillRectangle(Brushes.Green, new Rectangle(horizontalMid - barWidth / 2, visibleHeight - 35, (int)(barWidth * throttle), 30));
            g.DrawRectangle(new Pen(Brushes.White), new Rectangle(horizontalMid - barWidth / 2, visibleHeight - 35, barWidth, 30));
            g.DrawString($"{Math.Round(throttle * 100)}%", inputFont, Brushes.White, horizontalMid - barWidth / 2, visibleHeight - 33);


            float brake = pagePhysics.Brake;
            g.FillRectangle(Brushes.Red, new Rectangle(horizontalMid - barWidth / 2, visibleHeight - 75, (int)(barWidth * brake), 30));
            g.DrawRectangle(new Pen(Brushes.White), new Rectangle(horizontalMid - barWidth / 2, visibleHeight - 75, barWidth, 30));
            g.DrawString($"{Math.Round(brake * 100)}%", inputFont, Brushes.White, horizontalMid - barWidth / 2, visibleHeight - 73);
        }

        private void DrawData(Graphics g)
        {
            SolidBrush b = new SolidBrush(System.Drawing.Color.White);

            SPageFileGraphic pageStatic = sharedMemory.ReadGraphicsPageFile();
            FieldInfo[] members = pageStatic.GetType().GetFields();
            float y = 0;
            float emSize = 16;
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(pageStatic);

                bool isObsolete = false;
                foreach (CustomAttributeData cad in member.CustomAttributes)
                {
                    if (cad.AttributeType == typeof(ObsoleteAttribute)) { isObsolete = true; break; }
                }

                if (!isObsolete && !member.Name.Equals("Buffer") && !member.Name.Equals("Size"))
                {
                    value = FieldTypeValue(member, value);

                    g.DrawString($"{member.Name}: {value}", new Font("Arial", emSize), b, new PointF(0, y += emSize + 3));
                }
            }
        }

        public static object FieldTypeValue(FieldInfo member, object value)
        {

            if (member.FieldType.Name == typeof(byte[]).Name)
            {
                byte[] arr = (byte[])value;
                value = string.Empty;
                foreach (byte v in arr)
                {
                    value += $"{{{v}}}, ";
                }
            }


            if (member.FieldType.Name == typeof(Int32[]).Name)
            {
                Int32[] arr = (Int32[])value;
                value = string.Empty;
                foreach (Int32 v in arr)
                {
                    value += $"{{{v}}}, ";
                }
            }

            if (member.FieldType.Name == typeof(Single[]).Name)
            {
                Single[] arr = (Single[])value;
                value = string.Empty;
                foreach (Single v in arr)
                {
                    value += $"{{{v}}}, ";
                }
            }

            if (member.FieldType.Name == typeof(StructVector3[]).Name)
            {
                StructVector3[] arr = (StructVector3[])value;
                value = string.Empty;
                foreach (StructVector3 v in arr)
                {
                    value += $"{{{v}}}, ";
                }
            }

            if (member.FieldType.Name == typeof(StructVector3).Name)
            {
                value = (StructVector3)value;
            }

            return value;
        }
    }
}
