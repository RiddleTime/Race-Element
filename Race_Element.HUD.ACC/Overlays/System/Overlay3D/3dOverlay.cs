using CPI.Plot3D;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.System.Overlay3D
{

#if DEBUG
    [Overlay(Name = "3D",
     Description = "3d testing",
     Version = 1.00,
     OverlayType = OverlayType.Debug)]
#endif
    internal class _3dOverlay : AbstractOverlay
    {
        private _3dConfiguration _config = new _3dConfiguration();
        private class _3dConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Animation", "Change animation properties")]
            public AnimationGrouping Animation { get; set; } = new AnimationGrouping();
            public class AnimationGrouping
            {
                [ToolTip("Refreshrate in Hz.")]
                [IntRange(1, 100, 1)]
                public int RefreshRate { get; set; } = 30;
            }
        }

        public _3dOverlay(Rectangle rectangle) : base(rectangle, "3D")
        {
            this.Width = 320;
            this.Height = 300;
            this.RefreshRateHz = _config.Animation.RefreshRate;
        }

        int angle = 0;

        float z = 0;
        bool zoomIn = false;

        public override bool ShouldRender() => true;
        public override void Render(Graphics g)
        {
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using Plotter3D plotter3d = new Plotter3D(g, new Point3D(this.Width / 2, this.Height / 2, -90f));
            plotter3d.AngleMeasurement = AngleMeasurement.Degrees;

            plotter3d.PenUp();
            plotter3d.TurnRight(angle * 0.5);
            plotter3d.TurnUp(-angle * 1.2);
            float cubeSize = 25;
            plotter3d.MoveTo(new Point3D(160 - cubeSize / 2, 160 - cubeSize / 2, z));
            plotter3d.PenColor = zoomIn ? Color.OrangeRed : Color.Cyan;
            plotter3d.PenWidth = 1.5f;
            plotter3d.PenDown();

            DrawCube(plotter3d, cubeSize);
            angle += 1;


            float zAdd = .5f;
            if (zoomIn)
            {
                z += zAdd;
                if (z > 45)
                    zoomIn = false;
            }
            else
            {
                z -= zAdd;
                if (z < -45)
                    zoomIn = true;
            }
        }

        public void DrawCube(Plotter3D p, float sideLength)
        {
            for (int i = 0; i < 4; i++)
            {
                DrawSquare(p, sideLength);
                p.Forward(sideLength);
                p.TurnDown(90);
            }
        }

        public void DrawSquare(Plotter3D p, float sideLength)
        {
            for (int i = 0; i < 4; i++)
            {
                p.Forward(sideLength);  // Draw a line sideLength long
                p.TurnRight(90);        // Turn right 90 degrees
            }
        }

        public void DrawRotatedSquare(Plotter3D p, float sideLength, float rightRotationAngle, float downRotationAngle)
        {
            // Since we don't want to draw while repositioning ourselves at the
            // center of the object, we'll lift the pen up
            p.PenUp();
            p.MoveTo(new Point3D(160 - sideLength / 2, 160 - sideLength / 2, 30));

            // Move to the center of the square
            p.Forward(sideLength / 2);
            p.TurnRight(90);
            p.Forward(sideLength / 2);
            p.TurnLeft(90);

            // Now we rotate as much as we want
            p.TurnRight(rightRotationAngle);
            p.TurnDown(downRotationAngle);

            // Now we retrace our steps to get back
            // to the (rotated) starting point
            p.TurnLeft(90);
            p.Forward(sideLength / 2);
            p.TurnLeft(90);
            p.Forward(sideLength / 2);
            p.TurnRight(180);

            // Put the pen back down, so we start drawing again
            p.PenDown();

            // Finally we draw the square as we normally would
            DrawSquare(p, sideLength);
        }
    }
}
