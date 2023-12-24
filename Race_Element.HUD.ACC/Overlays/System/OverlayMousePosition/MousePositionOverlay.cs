using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System.Drawing;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.ACC.Overlays.OverlayMousePosition;

public sealed class MousePositionOverlay : AbstractOverlay
{
    private CachedBitmap _cachedCursor;
    private IKeyboardMouseEvents _globalKbmHook;

    private const int _circleWidth = 6;

    private readonly CachedBitmap.Renderer MouseDownRenderer = g =>
    {
        g.DrawEllipse(Pens.White, _circleWidth, _circleWidth, _circleWidth);
        g.DrawEllipse(Pens.White, _circleWidth, _circleWidth, 3);
        g.FillEllipse(new SolidBrush(Color.FromArgb(140, Color.LimeGreen)), 5, 5, 5);
    };
    private readonly CachedBitmap.Renderer MouseUpRenderer = g =>
    {
        g.DrawEllipse(Pens.White, _circleWidth, _circleWidth, _circleWidth);
        g.DrawEllipse(Pens.White, _circleWidth, _circleWidth, 3);
        g.FillEllipse(new SolidBrush(Color.FromArgb(181, Color.White)), 5, 5, 5);
    };

    public MousePositionOverlay(Rectangle rectangle, string Name) : base(rectangle, Name)
    {
        this.Width = _circleWidth * 2 + 1;
        this.Height = Width;
        //this.RequestsDrawItself = true;
        this.AllowReposition = false;
    }

    public sealed override void BeforeStart()
    {
        _cachedCursor = new CachedBitmap(Width, Height, MouseUpRenderer);
        _globalKbmHook = Hook.GlobalEvents();
        _globalKbmHook.MouseDown += GlobalMouseDown;
        _globalKbmHook.MouseUp += GlobalMouseUp;
        _globalKbmHook.MouseMove += GlobalMouseMove;

        this.X = GetCursorPosition().X - _circleWidth;
        this.Y = GetCursorPosition().Y - _circleWidth;

        this.RequestRedraw();
    }

    public sealed override void BeforeStop()
    {
        _globalKbmHook.MouseDown -= GlobalMouseDown;
        _globalKbmHook.MouseUp -= GlobalMouseUp;
        _globalKbmHook.MouseMove -= GlobalMouseMove;
        _globalKbmHook.Dispose();

        _cachedCursor?.Dispose();
    }

    private void GlobalMouseMove(object sender, MouseEventArgs e)
    {
        Point cursor = GetCursorPosition();
        this.Location = new Point(cursor.X - _circleWidth, cursor.Y - _circleWidth);

        _cachedCursor.SetRenderer(MouseUpRenderer);
    }

    private void GlobalMouseUp(object sender, MouseEventArgs e)
    {
        _cachedCursor.SetRenderer(MouseUpRenderer);
        this.RequestRedraw();
    }

    private void GlobalMouseDown(object sender, MouseEventArgs e)
    {
        _cachedCursor.SetRenderer(MouseDownRenderer);
        this.RequestRedraw();
    }

    float opacity = 0.3f;
    public sealed override void Render(Graphics g)
    {
        //_cachedCursor.Opacity = opacity;
        _cachedCursor.Draw(g, Width, Height);

    }

    public sealed override bool ShouldRender() => true;

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    private static Point GetCursorPosition()
    {
        POINT lpPoint;
        GetCursorPos(out lpPoint);
        // NOTE: If you need error handling
        // bool success = GetCursorPos(out lpPoint);
        // if (!success)

        return lpPoint;
    }

    /// <summary>
    /// Struct representing a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;

        public static implicit operator Point(POINT point)
        {
            return new Point(point.X, point.Y);
        }
    }
}

