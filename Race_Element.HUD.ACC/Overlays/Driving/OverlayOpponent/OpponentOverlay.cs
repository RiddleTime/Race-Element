using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayOpponent;

#if DEBUG
[Overlay(Name = "Opponent", Description = "Shows info about the car in front and behind.", OverlayType = OverlayType.Drive, Version = 1.00)]
#endif
internal class OpponentOverlay : AbstractOverlay
{

    private const int InitialWidth = 300, InitialHeight = 250;
    private InfoTable _table;
    public OpponentOverlay(Rectangle rectangle) : base(rectangle, "Opponent")
    {
        this.Width = InitialWidth;
        this.Height = InitialHeight;

        _table = new InfoTable(12, [100, 100]);
    }

    public override void BeforeStart()
    {

    }

    public override void BeforeStop()
    {
    }

    public override void Render(Graphics g)
    {
        float gapAhead = pageGraphics.gapAheadMillis / 1000f;
        _table.AddRow("Ahead", [$"{gapAhead:F3}"]);

        float gapBehind = pageGraphics.gapBehindMillis * -1 / 1000f;
        _table.AddRow("Behind", [$"{gapBehind:F3}"]);
        _table.Draw(g);
    }

    public override bool ShouldRender() => DefaultShouldRender();
}
