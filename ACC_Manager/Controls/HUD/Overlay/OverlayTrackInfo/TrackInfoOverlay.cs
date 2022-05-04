using ACCSetupApp.Controls.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayTrackInfo
{
    internal class TrackInfoOverlay : AbstractOverlay
    {
        public TrackInfoOverlay(Rectangle rectangle) : base(rectangle, "Track Info Overlay")
        {
            this.Width = 200;
            this.Height = 100;
        }

        public override void BeforeStart() { }

        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));

            InfoDrawing info = new InfoDrawing();
            info.AddLine(new InfoLine() { Title = "Time", Value = pageGraphics.CurrentTime });
            info.AddLine(new InfoLine() { Title = "Flag", Value = ACCSharedMemory.FlagTypeToString(pageGraphics.Flag) });
            info.AddLine(new InfoLine() { Title = "Session", Value = pageGraphics.SessionType.ToString() });
            info.AddLine(new InfoLine() { Title = "Track status", Value = pageGraphics.trackGripStatus.ToString() });
            info.AddLine(new InfoLine() { Title = "Temperature", Value = $"Air: {Math.Round(pagePhysics.AirTemp, 2)}, Track: {Math.Round(pagePhysics.RoadTemp, 2)}" });
            info.AddLine(new InfoLine() { Title = "Wind", Value = $"Speed: {Math.Round(pageGraphics.WindSpeed, 2)}" });

            info.Draw(g, this.Width);


            //g.DrawString($"")
        }

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif

            return false;
        }

        internal class InfoDrawing
        {
            public Font Font = new Font("Arial", 10);
            private List<InfoLine> Lines = new List<InfoLine>();

            public void AddLine(InfoLine info)
            {
                Lines.Add(info);
            }

            public void Draw(Graphics g, int maxWidth)
            {
                int lineY = 0;
                foreach (InfoLine line in Lines)
                {
                    g.DrawString($"{line.Title}: {line.Value}", Font, Brushes.White, new PointF(0, lineY));
                    lineY += Font.Height;
                }
            }
        }

        internal class InfoLine
        {
            internal string Title { get; set; }
            internal string Value { get; set; }
        }
    }
}
