using ACCSetupApp.Controls.HUD.Overlay.Internal;
using ACCSetupApp.Controls.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.Controls.HUD.Overlay.OverlayUtil.InfoPanel;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayTrackInfo
{
    internal class TrackInfoOverlay : AbstractOverlay
    {
        private InfoPanel panel = new InfoPanel(11);

        public TrackInfoOverlay(Rectangle rectangle) : base(rectangle, "Track Info Overlay")
        {
            this.Width = 300;
            this.Height = 100;
        }

        public override void BeforeStart() { }

        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));


            panel.AddLine(new InfoLine() { Title = "Flag", Value = ACCSharedMemory.FlagTypeToString(pageGraphics.Flag) });
            panel.AddLine(new InfoLine() { Title = "Session", Value = ACCSharedMemory.SessionTypeToString(pageGraphics.SessionType) });
            panel.AddLine(new InfoLine() { Title = "Track status", Value = pageGraphics.trackGripStatus.ToString() });
            panel.AddLine(new InfoLine() { Title = "Temps", Value = $"Air: {Math.Round(pagePhysics.AirTemp, 2)}, Track: {Math.Round(pagePhysics.RoadTemp, 2)}" });
            panel.AddLine(new InfoLine() { Title = "Wind", Value = $"{Math.Round(pageGraphics.WindSpeed, 2)} km/h" });

            panel.Draw(g);
        }

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif

            return false;
        }


    }
}
