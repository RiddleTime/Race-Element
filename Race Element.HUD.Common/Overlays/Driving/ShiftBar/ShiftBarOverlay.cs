using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftBar;

#if DEBUG
[Overlay(
    Name = "Shift Bar",
    Description = "A Fancy Bar"
)]
#endif
internal sealed class ShiftBarOverlay : CommonAbstractOverlay
{
    private readonly ShiftBarConfiguration _config = new();
    private sealed class ShiftBarConfiguration : OverlayConfiguration
    {
        public ShiftBarConfiguration() => GenericConfiguration.AllowRescale = true;
    }

    private CachedBitmap _cachedbackground;
    private CachedBitmap _cachedBar;

    private readonly RectangleF WorkingSpace = new(0, 0, 300, 20);
    public ShiftBarOverlay(Rectangle rectangle) : base(rectangle, "Shift Bar")
    {
        Width = (int)WorkingSpace.Width;
        Height = (int)WorkingSpace.Height;
        RefreshRateHz = 80;
    }

    public override void BeforeStart()
    {
        int scaledWorkingWidth = (int)(WorkingSpace.Width * Scale);
        int scaledWorkingHeight = (int)(WorkingSpace.Height * Scale);

        RectangleF scaledWorkingSize = new(0, 0, scaledWorkingWidth, scaledWorkingHeight);

        _cachedbackground = new(scaledWorkingWidth, scaledWorkingHeight, g =>
        {
            int horizontalPadding = 2;
            int verticalPadding = 1;
            RectangleF barArea = new(horizontalPadding, verticalPadding, scaledWorkingWidth - horizontalPadding * 2, scaledWorkingHeight - verticalPadding * 2);
                               
            g.FillRectangle(Brushes.White, barArea);

        });

        _cachedBar = new(scaledWorkingWidth, scaledWorkingHeight, g =>
        {
            int horizontalPadding = 8;
            int verticalPadding = 4;
            RectangleF barArea = new(horizontalPadding, verticalPadding, scaledWorkingWidth - horizontalPadding * 2, scaledWorkingHeight - verticalPadding * 2);

            g.DrawRectangle(Pens.Black, barArea);
        });
    }

    public override void Render(Graphics g)
    {
        int workingSpaceWidth = (int)WorkingSpace.Width;
        int workingSpaceHeight = (int)WorkingSpace.Height;
        _cachedbackground.Draw(g, 0, 0, workingSpaceWidth, workingSpaceHeight);
        _cachedBar.Draw(g, 0, 0, workingSpaceWidth, workingSpaceHeight);
    }
}
