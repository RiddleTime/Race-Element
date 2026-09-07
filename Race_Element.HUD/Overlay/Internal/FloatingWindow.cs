using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static RaceElement.HUD.Overlay.Internal.WindowStructs;

namespace RaceElement.HUD.Overlay.Internal;

/// <summary>
/// Transparent layered native window used as the base for HUD overlays.
/// </summary>
public class FloatingWindow : NativeWindow, IDisposable
{
    public string Name { get; internal set; } = string.Empty;
    internal bool WindowMode { get; set; }
    internal bool AlwaysOnTop { get; set; } = true;

    [Flags]
    public enum AnimateMode
    {
        Blend,
        SlideRightToLeft,
        SlideLeftToRight,
        SlideTopToBottom,
        SlideBottomToTop,
        RollRightToLeft,
        RollLeftToRight,
        RollTopToBottom,
        RollBottomToTop,
        ExpandCollapse
    }

    protected bool _disposed;
    private byte _alpha = 255;
    private Size _size = new(1, 1);
    private Rectangle _drawingRect;
    private Point _location = new(50, 50);

    // Mouse state
    private int _deltaX;
    private int _deltaY;
    private bool _captured;
    private bool _isMouseIn;
    private bool _onMouseMove;
    private bool _onMouseDown;
    private bool _onMouseUp;
    private Point _lastMouseDown = Point.Empty;

    #region Painting

    /// <summary>
    /// Override to provide custom painting. Called on a temporary bitmap that is then
    /// presented via UpdateLayeredWindow.
    /// </summary>
    protected virtual void PerformPaint(PaintEventArgs e) { }

    #endregion

    #region Updating

    protected internal void Invalidate() => UpdateLayeredWindow();

    protected void UpdateLayeredWindow()
    {
        if (_disposed || base.Handle == IntPtr.Zero)
            return;

        try
        {
            using var bitmap = new Bitmap(Size.Width, Size.Height, PixelFormat.Format32bppPArgb);
            using var graphics = Graphics.FromImage(bitmap);

            PerformPaint(new PaintEventArgs(graphics, _drawingRect));

            IntPtr screenDc = User32.GetDC(IntPtr.Zero);
            IntPtr memDc = Gdi32.CreateCompatibleDC(screenDc);
            IntPtr hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
            IntPtr oldBitmap = Gdi32.SelectObject(memDc, hBitmap);

            var size = new SIZE { cx = Size.Width, cy = Size.Height };
            var dstPoint = new POINT(Location.X, Location.Y);
            var srcPoint = new POINT(0, 0);

            var blend = new BLENDFUNCTION
            {
                BlendOp = 0,
                BlendFlags = 0,
                SourceConstantAlpha = _alpha,
                AlphaFormat = 1 // AC_SRC_ALPHA
            };

            User32.UpdateLayeredWindow(
                base.Handle,
                screenDc,
                ref dstPoint,
                ref size,
                memDc,
                ref srcPoint,
                0,
                ref blend,
                2); // ULW_ALPHA

            Gdi32.SelectObject(memDc, oldBitmap);
            User32.ReleaseDC(IntPtr.Zero, screenDc);
            Gdi32.DeleteObject(hBitmap);
            Gdi32.DeleteDC(memDc);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    #endregion

    #region Show / Hide / Animate

    public virtual void Show()
    {
        if (base.Handle == IntPtr.Zero)
            CreateWindowOnly(GetExStyle());

        User32.ShowWindow(base.Handle, User32.SW_SHOWNOACTIVATE);
    }

    public virtual void SetDraggy(bool toggle)
    {
        if (base.Handle == IntPtr.Zero)
            CreateWindowOnly(toggle ? GetExStyleDrag() : GetExStyle());

        User32.SetWindowLong(base.Handle, -20, (uint)(toggle ? GetExStyleDrag() : GetExStyle()));
        User32.ShowWindow(base.Handle, User32.SW_SHOWNOACTIVATE);
    }

    public virtual void Show(int x, int y)
    {
        _location = new Point(x, y);
        Show();
    }

    public virtual void ShowAnimate(AnimateMode mode, uint time)
    {
        uint flags = GetAnimateFlags(mode);

        if (base.Handle == IntPtr.Zero)
            CreateWindowOnly(GetExStyle());

        if ((flags & User32.AW_BLEND) != 0)
            AnimateWithBlend(show: true, time);
        else
            User32.AnimateWindow(base.Handle, time, flags);
    }

    public virtual void ShowAnimate(int x, int y, AnimateMode mode, uint time)
    {
        _location = new Point(x, y);
        ShowAnimate(mode, time);
    }

    public virtual void Hide()
    {
        if (base.Handle == IntPtr.Zero)
            return;

        User32.ShowWindow(base.Handle, User32.SW_HIDE);
    }

    public virtual void HideAnimate(AnimateMode mode, uint time)
    {
        if (base.Handle == IntPtr.Zero)
            return;

        uint flags = GetAnimateFlags(mode) | User32.AW_HIDE;

        if ((flags & User32.AW_BLEND) != 0)
            AnimateWithBlend(show: false, time);
        else
            User32.AnimateWindow(base.Handle, time, flags);

        Hide();
    }

    public virtual void Close()
    {
        if (Handle != IntPtr.Zero)
            Hide();

        Dispose();
    }

    private static uint GetAnimateFlags(AnimateMode mode) => mode switch
    {
        AnimateMode.Blend => User32.AW_BLEND,
        AnimateMode.ExpandCollapse => User32.AW_CENTER,
        AnimateMode.SlideLeftToRight => User32.AW_HOR_POSITIVE | User32.AW_SLIDE,
        AnimateMode.SlideRightToLeft => User32.AW_HOR_NEGATIVE | User32.AW_SLIDE,
        AnimateMode.SlideTopToBottom => User32.AW_VER_POSITIVE | User32.AW_SLIDE,
        AnimateMode.SlideBottomToTop => User32.AW_VER_NEGATIVE | User32.AW_SLIDE,
        AnimateMode.RollLeftToRight => User32.AW_HOR_POSITIVE,
        AnimateMode.RollRightToLeft => User32.AW_HOR_NEGATIVE,
        AnimateMode.RollBottomToTop => User32.AW_VER_NEGATIVE,
        AnimateMode.RollTopToBottom => User32.AW_VER_POSITIVE,
        _ => 0
    };

    private void AnimateWithBlend(bool show, uint time)
    {
        byte originalAlpha = _alpha;
        byte step = (byte)Math.Max(1, originalAlpha / Math.Max(1, time / 10));

        if (show)
        {
            _alpha = 0;
            UpdateLayeredWindow();
            User32.ShowWindow(base.Handle, User32.SW_SHOWNOACTIVATE);
        }

        if (show)
        {
            for (byte i = 0; i <= originalAlpha; i += step)
            {
                _alpha = i;
                UpdateLayeredWindow();
                if (i > originalAlpha - step)
                    break;
            }
        }
        else
        {
            for (int i = originalAlpha; i >= 0; i -= step)
            {
                _alpha = (byte)i;
                UpdateLayeredWindow();
                if (i < step)
                    break;
            }
        }

        _alpha = originalAlpha;
        if (show)
            UpdateLayeredWindow();
    }

    private void CreateWindowOnly(int exStyle)
    {
        _location = Monitors.IsInsideMonitor(
            _location.X, _location.Y, _size.Width, _size.Height, Handle);

        var createParams = new CreateParams
        {
            Caption = Name,
            X = _location.X,
            Y = _location.Y,
            Height = _size.Height,
            Width = _size.Width,
            Parent = IntPtr.Zero,
            Style = unchecked((int)User32.WS_POPUP),
            ExStyle = exStyle
        };

        try
        {
            CreateHandle(createParams);
            UpdateLayeredWindow();
        }
        catch (InvalidOperationException)
        {
            // Handle already exists or invalid state – ignore
        }
    }

    public int GetExStyle()
    {
        int exStyle = User32.WS_EX_LAYERED | User32.WS_EX_TRANSPARENT;

        if (!WindowMode)
        {
            exStyle |= User32.WS_EX_TOOLWINDOW;
            exStyle |= User32.WS_EX_NOACTIVATE;
        }

        if (AlwaysOnTop)
            exStyle |= User32.WS_EX_TOPMOST;

        return exStyle;
    }

    public int GetExStyleDrag() =>
        User32.WS_EX_LAYERED | User32.WS_EX_TOPMOST | User32.WS_EX_NOACTIVATE | 0x00020000;

    #endregion

    #region WndProc

    private void PerformWmPaint_WmPrintClient(ref Message m, bool isPaintMessage)
    {
        try
        {
            var ps = new PAINTSTRUCT();
            IntPtr hdc = isPaintMessage ? User32.BeginPaint(m.HWnd, ref ps) : m.WParam;

            var rect = new RECT();
            User32.GetWindowRect(base.Handle, ref rect);
            var bounds = new Rectangle(0, 0, rect.right - rect.left, rect.bottom - rect.top);

            using (var graphics = Graphics.FromHdc(hdc))
            using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
            using (var g = Graphics.FromImage(bitmap))
            {
                PerformPaint(new PaintEventArgs(g, bounds));
                graphics.DrawImageUnscaled(bitmap, 0, 0);
            }

            if (isPaintMessage)
                User32.EndPaint(m.HWnd, ref ps);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case 15: // WM_PAINT
                PerformWmPaint_WmPrintClient(ref m, isPaintMessage: true);
                break;

            case 0x318: // WM_PRINTCLIENT
                PerformWmPaint_WmPrintClient(ref m, isPaintMessage: false);
                return;

            case 0x21: // WM_MOUSEACTIVATE
                m.Result = (IntPtr)3; // MA_NOACTIVATE
                return;

            case 0x200: // WM_MOUSEMOVE
                if (!_isMouseIn)
                {
                    OnMouseEnter();
                    _isMouseIn = true;
                }

                var pMove = new Point(m.LParam.ToInt32());
                OnMouseMove(new MouseEventArgs(Control.MouseButtons, 1, pMove.X, pMove.Y, 0));

                if (_onMouseMove)
                {
                    PerformWmMouseMove(ref m);
                    _onMouseMove = false;
                }
                break;

            case 0x201: // WM_LBUTTONDOWN
                {
                    _lastMouseDown = new Point(m.LParam.ToInt32());
                    var screenPt = MousePositionToScreen(new POINT { X = _lastMouseDown.X, Y = _lastMouseDown.Y });
                    _deltaX = screenPt.X - Location.X;
                    _deltaY = screenPt.Y - Location.Y;

                    OnMouseDown(new MouseEventArgs(Control.MouseButtons, 1, _lastMouseDown.X, _lastMouseDown.Y, 0));

                    if (_onMouseDown)
                    {
                        PerformWmMouseDown(ref m);
                        _onMouseDown = false;
                    }
                    return;
                }

            case 0x202: // WM_LBUTTONUP
                {
                    var pUp = new Point(m.LParam.ToInt32());
                    OnMouseUp(new MouseEventArgs(Control.MouseButtons, 1, pUp.X, pUp.Y, 0));

                    if (_onMouseUp)
                    {
                        PerformWmMouseUp(ref m);
                        _onMouseUp = false;
                    }
                    return;
                }

            case 0x02A3: // WM_MOUSELEAVE
                if (_isMouseIn)
                {
                    OnMouseLeave();
                    _isMouseIn = false;
                }
                break;
        }

        base.WndProc(ref m);
    }

    #endregion

    #region Mouse Helpers

    private POINT MousePositionToClient(POINT point)
    {
        User32.ScreenToClient(base.Handle, ref point);
        return point;
    }

    private POINT MousePositionToScreen(POINT point)
    {
        User32.ClientToScreen(base.Handle, ref point);
        return point;
    }

    private void PerformWmMouseDown(ref Message m)
    {
        var clientLoc = MousePositionToClient(new POINT { X = Location.X, Y = Location.Y });
        if (new Rectangle(clientLoc, Size).Contains(_lastMouseDown))
        {
            _captured = true;
            User32.SetCapture(base.Handle);
        }
    }

    private void PerformWmMouseMove(ref Message m)
    {
        var screenPos = Control.MousePosition;
        var clientPos = MousePositionToClient(new POINT { X = screenPos.X, Y = screenPos.Y });

        Cursor.Current = new Rectangle(0, 0, Size.Width, Size.Height).Contains(clientPos.X, clientPos.Y)
            ? Cursors.Hand
            : Cursors.Arrow;

        if (_captured)
            Location = new Point(screenPos.X - _deltaX, screenPos.Y - _deltaY);
    }

    private void PerformWmMouseUp(ref Message m)
    {
        if (!_captured)
            return;

        _captured = false;
        User32.ReleaseCapture();
    }

    protected virtual void OnMouseMove(MouseEventArgs e)
    {
        MouseMove?.Invoke(this, e);
        _onMouseMove = true;
    }

    protected virtual void OnMouseDown(MouseEventArgs e)
    {
        MouseDown?.Invoke(this, e);
        _onMouseDown = true;
    }

    protected virtual void OnMouseUp(MouseEventArgs e)
    {
        MouseUp?.Invoke(this, e);
        _onMouseUp = true;
    }

    protected virtual void OnMouseEnter() => MouseEnter?.Invoke(this, EventArgs.Empty);
    protected virtual void OnMouseLeave() => MouseLeave?.Invoke(this, EventArgs.Empty);

    #endregion

    #region Events

    public event EventHandler? SizeChanged;
    public event EventHandler? LocationChanged;
    public event EventHandler? Move;
    public event EventHandler? Resize;
    public event MouseEventHandler? MouseDown;
    public event MouseEventHandler? MouseUp;
    public event MouseEventHandler? MouseMove;
    public event EventHandler? MouseEnter;
    public event EventHandler? MouseLeave;

    protected virtual void OnLocationChanged(EventArgs e)
    {
        OnMove(EventArgs.Empty);
        LocationChanged?.Invoke(this, e);
    }

    protected virtual void OnSizeChanged(EventArgs e)
    {
        OnResize(EventArgs.Empty);
        SizeChanged?.Invoke(this, e);
    }

    protected virtual void OnMove(EventArgs e) => Move?.Invoke(this, e);
    protected virtual void OnResize(EventArgs e) => Resize?.Invoke(this, e);

    #endregion

    #region Size & Location

    protected virtual void SetBoundsCore(int x, int y, int width, int height)
    {
        if (X == x && Y == y && Width == width && Height == height)
            return;

        if (base.Handle != IntPtr.Zero)
        {
            uint flags = 20; // SWP_NOZORDER | SWP_NOACTIVATE
            if (X == x && Y == y) flags |= 2;          // SWP_NOMOVE
            if (Width == width && Height == height) flags |= 1; // SWP_NOSIZE

            User32.SetWindowPos(base.Handle, IntPtr.Zero, x, y, width, height, flags);
        }
        else
        {
            Location = new Point(x, y);
            Size = new Size(width, height);
        }
    }

    public virtual Point Location
    {
        get => _location;
        set
        {
            if (base.Handle != IntPtr.Zero)
            {
                SetBoundsCore(value.X, value.Y, _size.Width, _size.Height);
                var rect = new RECT();
                User32.GetWindowRect(base.Handle, ref rect);
                _location = new Point(rect.left, rect.top);
            }
            else
            {
                _location = value;
            }
        }
    }

    public virtual Size Size
    {
        get => _size;
        set
        {
            if (base.Handle != IntPtr.Zero)
            {
                SetBoundsCore(_location.X, _location.Y, value.Width, value.Height);
                var rect = new RECT();
                User32.GetWindowRect(base.Handle, ref rect);
                _size = new Size(rect.right - rect.left, rect.bottom - rect.top);
            }
            else
            {
                _size = value;
            }

            _drawingRect = new Rectangle(0, 0, _size.Width, _size.Height);
        }
    }

    public int Height
    {
        get => _size.Height;
        set => Size = new Size(_size.Width, value);
    }

    public int Width
    {
        get => _size.Width;
        set => Size = new Size(value, _size.Height);
    }

    public int X
    {
        get => _location.X;
        set => Location = new Point(value, Location.Y);
    }

    public int Y
    {
        get => _location.Y;
        set => Location = new Point(Location.X, value);
    }

    public Rectangle Bound => new(Point.Empty, _size);

    public byte Alpha
    {
        get => _alpha;
        set
        {
            if (_alpha == value)
                return;

            _alpha = value;
            UpdateLayeredWindow();
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        DestroyHandle();
        _disposed = true;
    }

    #endregion
}

#region Win32 interop

internal struct PAINTSTRUCT
{
    public IntPtr hdc;
    public int fErase;
    public Rectangle rcPaint;
    public int fRestore;
    public int fIncUpdate;
    public int Reserved1, Reserved2, Reserved3, Reserved4;
    public int Reserved5, Reserved6, Reserved7, Reserved8;
}

[StructLayout(LayoutKind.Sequential)]
internal struct TRACKMOUSEEVENTS
{
    public uint cbSize;
    public uint dwFlags;
    public IntPtr hWnd;
    public uint dwHoverTime;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MSG
{
    public IntPtr hwnd;
    public int message;
    public IntPtr wParam;
    public IntPtr lParam;
    public int time;
    public int pt_x;
    public int pt_y;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct BLENDFUNCTION
{
    public byte BlendOp;
    public byte BlendFlags;
    public byte SourceConstantAlpha;
    public byte AlphaFormat;
}

internal static class User32
{
    public const uint WS_POPUP = 0x80000000;
    public const int WS_EX_TOPMOST = 0x8;
    public const int WS_EX_TOOLWINDOW = 0x80;
    public const int WS_EX_LAYERED = 0x80000;
    public const int WS_EX_TRANSPARENT = 0x20;
    public const int WS_EX_NOACTIVATE = 0x08000000;
    public const int SW_SHOWNOACTIVATE = 4;
    public const int SW_HIDE = 0;

    public const uint AW_HOR_POSITIVE = 0x1;
    public const uint AW_HOR_NEGATIVE = 0x2;
    public const uint AW_VER_POSITIVE = 0x4;
    public const uint AW_VER_NEGATIVE = 0x8;
    public const uint AW_CENTER = 0x10;
    public const uint AW_HIDE = 0x10000;
    public const uint AW_ACTIVATE = 0x20000;
    public const uint AW_SLIDE = 0x40000;
    public const uint AW_BLEND = 0x80000;

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern bool AnimateWindow(IntPtr hWnd, uint dwTime, uint dwFlags);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr BeginPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern bool ClientToScreen(IntPtr hWnd, ref POINT pt);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern bool ScreenToClient(IntPtr hWnd, ref POINT pt);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint newLong);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int X, int Y, int Width, int Height, uint flags);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern bool SetCapture(IntPtr hWnd);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern bool ReleaseCapture();

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern int ShowWindow(IntPtr hWnd, short cmdShow);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern bool UpdateLayeredWindow(
        IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize,
        IntPtr hdcSrc, ref POINT pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);
}

internal static class Gdi32
{
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    internal static extern bool DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
}

#endregion
