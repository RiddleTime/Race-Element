using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.Common.Overlays.Flight.Controls;

[Overlay(
    Name = "Controls",
    Description = "Displays the current state of the controls",
    Authors = ["Reinier Klarenberg"],
    SupportedGames = Game.MicrosoftFlightSimulator2020 | Game.MicrosoftFlightSimulator2024
)]
internal sealed class ControlsOverlay : CommonAbstractOverlay
{
    private CachedBitmap? _cachedBackground;

    public ControlsOverlay(Rectangle rectangle) : base(rectangle, "Controls")
    {
        Width = 100;
        Height = 100;
    }

    public sealed override void BeforeStart()
    {
        _cachedBackground = new(Width, Height, g =>
        {
            using SolidBrush brush = new(Color.FromArgb(170, 0, 0, 0));
            g.FillRectangle(brush, new(0, 0, Width, Height));
        });
    }

    public sealed override void BeforeStop()
    {
        _cachedBackground?.Dispose();
    }

    public override void Render(Graphics g)
    {
        double aileronPosition = SimDataProvider.LocalPlane.Controls.AileronPosition;
        double elevatorPosition = SimDataProvider.LocalPlane.Controls.ElevatorPosition;
        double rudderPosition = SimDataProvider.LocalPlane.Controls.RudderPosition;

        _cachedBackground?.Draw(g);

        int centerX = (int)((aileronPosition * 100 + 100) / 2);
        int centerY = (int)((elevatorPosition * 100 + 100) / 2);


        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using SolidBrush joystickBrush = new(Color.FromArgb(255, 255, 255, 255));
        using Pen joystickPen = new(joystickBrush, 2);
        g.DrawLine(joystickPen, Width / 2, Height / 2, centerX, centerY);
    }
}
