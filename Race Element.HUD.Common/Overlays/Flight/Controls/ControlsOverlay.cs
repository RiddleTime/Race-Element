using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.Common.Overlays.Flight.Controls;

[Overlay(
    Name = "Controls",
    Description = "Displays the current state of basic control surfaces.\nAileron, Elevator and Rudder.",
    Authors = ["Reinier Klarenberg"],
    SupportedGames = Game.MicrosoftFlightSimulator2020 | Game.MicrosoftFlightSimulator2024
)]
internal sealed class ControlsOverlay : CommonAbstractOverlay
{
    private readonly ControlsConfiguration _config = new();
    private class ControlsConfiguration : OverlayConfiguration
    {
        public ControlsConfiguration() => this.GenericConfiguration.AllowRescale = true;
    }

    private CachedBitmap? _cachedBackground;
    private CachedBitmap? _cachedStickHead;
    private CachedBitmap? _cachedRudderBar;

    private const int BaseWidth = 130;
    private const int BaseHeight = 155;

    private const int BaseJoystickSize = 100;
    private const int BaseJoystickY = 64;

    private const int BaseRudderBarY = 126;
    private const int BaseRudderBarWidth = 106;
    private const int BaseRudderBarHeight = 18;

    private int _joystickSize;
    private int _rudderBarWidth;
    private int _rudderBarHeight;

    private Model _model;
    private readonly record struct Model(double Aileron, double Elevator, double Rudder);

    public ControlsOverlay(Rectangle rectangle) : base(rectangle, "Controls")
    {
        Width = BaseWidth;
        Height = BaseHeight;
    }

    public sealed override void SetupPreviewData() => _model = new Model(0.65, 0.5, -0.25);

    public sealed override void BeforeStart()
    {
        // Apply scaling
        int scaledWidth = (int)(Width * Scale);
        int scaledHeight = (int)(Height * Scale);

        _joystickSize = (int)(BaseJoystickSize * Scale);
        _rudderBarWidth = (int)(BaseRudderBarWidth * Scale);
        _rudderBarHeight = (int)(BaseRudderBarHeight * Scale);

        int centerX = scaledWidth / 2;
        int joystickY = (int)(BaseJoystickY * Scale);
        int rudderY = (int)(BaseRudderBarY * Scale);
        int rudderX = centerX - _rudderBarWidth / 2;

        // === Main Background Cache ===
        _cachedBackground = new CachedBitmap(scaledWidth + 1, scaledHeight + 1, g =>
        {
            // Background panel
            using SolidBrush bgBrush = new(Color.FromArgb(165, 0, 0, 0));
            g.FillRoundedRectangle(bgBrush, new Rectangle(2, 2, scaledWidth - 4, scaledHeight - 4), (int)(10 * Scale));

            using Pen borderPen = new(Color.FromArgb(90, 255, 255, 255), 1.5f * Scale);
            g.DrawRoundedRectangle(borderPen, new Rectangle(3, 3, scaledWidth - 6, scaledHeight - 6), (int)(9 * Scale));

            // Joystick base
            using SolidBrush baseBrush = new(Color.FromArgb(55, 55, 55));
            Rectangle joystickBaseBounds = new(centerX - _joystickSize / 2, joystickY - _joystickSize / 2, _joystickSize, _joystickSize);
            g.FillRoundedRectangle(baseBrush, joystickBaseBounds, (int)(4d * Scale));

            using Pen baseRingPen = new(Color.FromArgb(110, 110, 110), 4 * Scale);
            g.DrawRoundedRectangle(baseRingPen, joystickBaseBounds, (int)(4d * Scale));

            // Crosshair
            using Pen crossPen = new(Color.FromArgb(100, 200, 200, 200), 1.5f * Scale);
            g.DrawLine(crossPen, centerX - 18 * Scale, joystickY, centerX + 18 * Scale, joystickY);
            g.DrawLine(crossPen, centerX, joystickY - 18 * Scale, centerX, joystickY + 18 * Scale);

            // Rudder bar background
            using SolidBrush barBg = new(Color.FromArgb(55, 55, 55));
            g.FillRectangle(barBg, rudderX, rudderY, _rudderBarWidth, _rudderBarHeight);

            using Pen barBorder = new(Color.FromArgb(120, 200, 200, 200), 1.5f * Scale);
            g.DrawRectangle(barBorder, rudderX, rudderY, _rudderBarWidth, _rudderBarHeight);

            // Rudder center line
            using Pen centerPen = new(Color.FromArgb(110, 220, 220, 220), 1.5f * Scale);
            g.DrawLine(centerPen, centerX, rudderY - 1, centerX, rudderY + _rudderBarHeight + 1);
        });

        // === Cached Stick Head ===
        int headSize = (int)(14 * Scale);
        _cachedStickHead = new CachedBitmap(headSize + 1, headSize + 1, g =>
        {
            using SolidBrush headBrush = new(Color.FromArgb(255, 0, 180, 255));
            g.FillEllipse(headBrush, 1, 1, headSize - 2, headSize - 2);

            using Pen outline = new(Color.White, 1.8f * Scale);
            g.DrawEllipse(outline, 1, 1, headSize - 2, headSize - 2);
        });

        // === Cached Full Bright Rudder Bar ===
        _cachedRudderBar = new CachedBitmap(_rudderBarWidth + 1, _rudderBarHeight + 1, g =>
        {
            using SolidBrush fillBrush = new(Color.FromArgb(255, 255, 165, 40));
            g.FillRectangle(fillBrush, 0, 0, _rudderBarWidth, _rudderBarHeight);

            using Pen highlight = new(Color.FromArgb(80, 255, 255, 255), 1 * Scale);
            g.DrawRectangle(highlight, 0, 0, _rudderBarWidth - 1, _rudderBarHeight - 1);
        });

        Width = scaledWidth;
        Height = scaledHeight;
    }

    public sealed override void BeforeStop()
    {
        _cachedBackground?.Dispose();
        _cachedStickHead?.Dispose();
        _cachedRudderBar?.Dispose();
    }

    public override void Render(Graphics g)
    {
        if (!IsPreviewing)
            _model = new Model(
                SimDataProvider.LocalPlane.Controls.AileronPosition,
                SimDataProvider.LocalPlane.Controls.ElevatorPosition,
                SimDataProvider.LocalPlane.Controls.RudderPosition
            );

        _cachedBackground?.Draw(g);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingQuality = CompositingQuality.HighQuality;

        int centerX = (int)(BaseWidth * Scale) / 2;
        int joystickY = (int)(BaseJoystickY * Scale);

        // === Joystick ===
        int stickX = (int)(centerX + _model.Aileron * (_joystickSize / 2));
        int stickY = (int)(joystickY + _model.Elevator * (_joystickSize / 2));

        using Pen stickPen = new(Color.FromArgb(240, 255, 255, 255), 3f * Scale);
        g.DrawLine(stickPen, centerX, joystickY, stickX, stickY);

        _cachedStickHead?.Draw(g, new Point(stickX - (int)(7 * Scale), stickY - (int)(7 * Scale)));

        // === Rudder Bar with Clipping ===
        int rudderY = (int)(BaseRudderBarY * Scale);
        int rudderBarX = (int)(centerX - _rudderBarWidth / 2f);

        int fillWidth = (int)(_model.Rudder * (_rudderBarWidth / 2f));

        Rectangle clipRect = fillWidth >= 0
            ? new Rectangle(centerX, rudderY, fillWidth, _rudderBarHeight)
            : new Rectangle(centerX + fillWidth, rudderY, -fillWidth, _rudderBarHeight);

        g.SetClip(clipRect);
        _cachedRudderBar?.Draw(g, new Point(rudderBarX, rudderY));
        g.ResetClip();
    }
}