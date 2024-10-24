using CPI.Plot3D;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.System.Overlay3D;


[Overlay(Name = "3D",
     Description = "3d testing",
     Version = 1.00,
     OverlayType = OverlayType.Pitwall)]
internal class _3dOverlay : AbstractOverlay
{
    private _3dConfiguration _config = new();
    private sealed class _3dConfiguration : OverlayConfiguration
    {

        public _3dConfiguration()
        {
            GenericConfiguration.AllowRescale = true;
        }

        [ConfigGrouping("Animation", "Change animation properties")]
        public AnimationGrouping Animation { get; set; } = new AnimationGrouping();
        public class AnimationGrouping
        {
            [ToolTip("Refreshrate in Hz.")]
            [IntRange(10, 1000, 10)]
            public int RefreshRate { get; set; } = 200;

            [IntRange(8, 18, 2)]
            public int CubeSize { get; set; } = 10;
        }
    }

    private DateTime _Timestart = DateTime.Now;

    public _3dOverlay(Rectangle rectangle) : base(rectangle, "3D")
    {
        this.Width = 300;
        this.Height = 300;
        this.RefreshRateHz = _config.Animation.RefreshRate;
    }


    float z = 0;
    bool zoomIn = false;

    public DateTime StarTime { get; private set; }

    public override void SetupPreviewData()
    {
        _Timestart = DateTime.Now.Subtract(new TimeSpan(0, 12, 5));
        z = 10;
    }

    public override bool ShouldRender() => true;
    public override void BeforeStart()
    {
        StarTime = DateTime.UtcNow;
    }
    public override void Render(Graphics g)
    {
        using SolidBrush backgroundBrush = new(Color.FromArgb(130, Color.Black));
        g.FillRectangle(backgroundBrush, new Rectangle(0, 0, this.Width, this.Height));

        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        float scaledWidth = this.Width / this.Scale;
        float scaledHeight = this.Height / this.Scale;

        using Plotter3D plotter3d = new(g, new Point3D(scaledWidth / 2 + 110, scaledHeight / 2 - 90, _config.Animation.CubeSize * 2.5f));

        float angle = (float)Math.Sin((float)DateTime.Now.Subtract(_Timestart).TotalSeconds);
        float cubeSize = _config.Animation.CubeSize;

        plotter3d.PenUp();
        plotter3d.TurnRight(Math.Sin(angle) * 1.5 + z);
        plotter3d.TurnUp(-Math.Sin(angle) * 1.2);
        plotter3d.MoveTo(new Point3D(scaledWidth / 2 - cubeSize, scaledHeight / 2 - cubeSize / 2, z * (z > 0 ? -2 : 2)));
        plotter3d.PenColor = !zoomIn ? Color.OrangeRed : Color.Cyan;
        plotter3d.PenWidth = 0.5f;
        plotter3d.PenDown();

        DrawCube(plotter3d, cubeSize);
        plotter3d.PenUp();

        float zAdd = .35f;
        float maxZoom = 25;
        if (zoomIn)
        {
            z += zAdd;
            if (z > maxZoom)
                zoomIn = false;
        }
        else
        {
            z -= zAdd;
            if (z < -maxZoom)
                zoomIn = true;
        }


        plotter3d.PenUp();
        plotter3d.TurnRight(-Math.Sin(angle) * 0.24);
        plotter3d.TurnUp(Math.Sin(angle) * .5);
        plotter3d.MoveTo(new Point3D(scaledWidth / 2 - cubeSize / 2 + cubeSize * 3, scaledHeight / 2 - cubeSize / 2, -z * .3f));
        plotter3d.PenColor = !zoomIn ? Color.Cyan : Color.OrangeRed;
        plotter3d.PenWidth = 0.5f;
        plotter3d.PenDown();

        DrawCube(plotter3d, cubeSize);
        plotter3d.PenUp();
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
