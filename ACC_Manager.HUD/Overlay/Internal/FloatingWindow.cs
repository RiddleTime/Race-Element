using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static ACCManager.HUD.Overlay.Internal.WindowStructs;

namespace ACCManager.HUD.Overlay.Internal
{
    public class FloatingWindow : NativeWindow, IDisposable
    {
        public string Name { get; internal set; }
        internal bool WindowMode { get; set; } = false;
        internal bool AlwaysOnTop { get; set; } = true;



        #region #  Enums  #
        public enum AnimateMode
        {
            Blend,
            SlideRightToLeft,
            SlideLeftToRight,
            SlideTopToBottom,
            SlideBottmToTop,
            RollRightToLeft,
            RollLeftToRight,
            RollTopToBottom,
            RollBottmToTop,
            ExpandCollapse
        }
        #endregion

        #region #  Fields  #
        protected bool _disposed = false;
        private byte _alpha = 255;
        private Size _size = new Size(250, 50);
        private Point _location = new Point(50, 50);
        #endregion

        #region #  Methods  #

        #region == Painting ==
        /// <summary>
        /// Performs the painting of the window. Overide this method to provide custom painting
        /// </summary>
        /// <param name="e">A <see cref="PaintEventArgs"/> containing the event data.</param>
        protected virtual void PerformPaint(PaintEventArgs e)
        {
        }
        #endregion

        #region == Updating ==
        protected internal void Invalidate()
        {
            this.UpdateLayeredWindow();
        }

        protected void UpdateLayeredWindow()
        {
            try
            {
                Bitmap bitmap1 = new Bitmap(this.Size.Width, this.Size.Height, PixelFormat.Format32bppPArgb);
                using (Graphics graphics1 = Graphics.FromImage(bitmap1))
                {
                    Rectangle rectangle1;
                    SIZE size1;
                    POINT point1;
                    POINT point2;
                    BLENDFUNCTION blendfunction1;
                    rectangle1 = new Rectangle(0, 0, this.Size.Width, this.Size.Height);
                    this.PerformPaint(new PaintEventArgs(graphics1, rectangle1));
                    IntPtr ptr1 = User32.GetDC(IntPtr.Zero);
                    IntPtr ptr2 = Gdi32.CreateCompatibleDC(ptr1);
                    IntPtr ptr3 = bitmap1.GetHbitmap(Color.FromArgb(0));
                    IntPtr ptr4 = Gdi32.SelectObject(ptr2, ptr3);
                    size1.cx = this.Size.Width;
                    size1.cy = this.Size.Height;
                    point1.X = this.Location.X;
                    point1.Y = this.Location.Y;
                    point2.X = 0;
                    point2.Y = 0;
                    blendfunction1 = new BLENDFUNCTION();
                    blendfunction1.BlendOp = 0;
                    blendfunction1.BlendFlags = 0;
                    blendfunction1.SourceConstantAlpha = this._alpha;
                    blendfunction1.AlphaFormat = 1;
                    User32.UpdateLayeredWindow(base.Handle, ptr1, ref point1, ref size1, ptr2, ref point2, 0, ref blendfunction1, 2); //2=ULW_ALPHA
                    Gdi32.SelectObject(ptr2, ptr4);
                    User32.ReleaseDC(IntPtr.Zero, ptr1);
                    Gdi32.DeleteObject(ptr3);
                    Gdi32.DeleteDC(ptr2);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        #endregion

        #region == Show / Hide ==
        /// <summary>
        /// Shows the window. Position determined by Location property in screen coordinates
        /// </summary>
        public virtual void Show()
        {
            //Debug.WriteLine("base handle: " + base.Handle);
            if (base.Handle == IntPtr.Zero)
            {  //if handle don't equal to zero - window was created and just hided
                //Debug.WriteLine("Creating window only with normal style");
                this.CreateWindowOnly(GetExStyle());
            }

            User32.ShowWindow(base.Handle, User32.SW_SHOWNOACTIVATE);
            //Debug.WriteLine("Showing window");
            //SetBoundsCore(X, Y, Width, Height);
        }

        public virtual void SetDraggy(bool toggle)
        {
            if (base.Handle == IntPtr.Zero)
            {  //if handle don't equal to zero - window was created and just hided
                //Debug.WriteLine($"Creating window only with draggy style: {toggle}");
                this.CreateWindowOnly(toggle ? GetExStyleDrag() : GetExStyle());
            }

            //Debug.WriteLine($"Settings Draggy style: {toggle}");

            //Debug.WriteLine("Current style" + User32.GetWindowLong(this.Handle, -20));
            //Debug.WriteLine("Drag style " + GetExStyleDrag());
            //Debug.WriteLine("Normal style" + GetExStyle());

            if (toggle)
                User32.SetWindowLong(base.Handle, -20, (uint)GetExStyleDrag());
            else
                User32.SetWindowLong(base.Handle, -20, (uint)GetExStyle());

            //SetBoundsCore(X, Y, Width, Height);
            User32.ShowWindow(base.Handle, User32.SW_SHOWNOACTIVATE);
        }

        /// <summary>
        /// Shows the window.
        /// </summary>
        /// <param name="x">x-coordinate of window in screen coordinates</param>
        /// <param name="y">x-coordinate of window in screen coordinates</param>
        public virtual void Show(int x, int y)
        {
            this._location.X = x;
            this._location.Y = y;
            this.Show();
        }
        /// <summary>
        /// Shows the window with animation effect. Position determined by Location property in screen coordinates
        /// </summary>
        /// <param name="mode">Effect to be applied</param>
        /// <param name="time">Time, in milliseconds, for effect playing</param>
        public virtual void ShowAnimate(AnimateMode mode, uint time)
        {
            uint dwFlag = 0;
            switch (mode)
            {
                case AnimateMode.Blend:
                    dwFlag = User32.AW_BLEND;
                    break;
                case AnimateMode.ExpandCollapse:
                    dwFlag = User32.AW_CENTER;
                    break;
                case AnimateMode.SlideLeftToRight:
                    dwFlag = User32.AW_HOR_POSITIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.SlideRightToLeft:
                    dwFlag = User32.AW_HOR_NEGATIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.SlideTopToBottom:
                    dwFlag = User32.AW_VER_POSITIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.SlideBottmToTop:
                    dwFlag = User32.AW_VER_NEGATIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.RollLeftToRight:
                    dwFlag = User32.AW_HOR_POSITIVE;
                    break;
                case AnimateMode.RollRightToLeft:
                    dwFlag = User32.AW_HOR_NEGATIVE;
                    break;
                case AnimateMode.RollBottmToTop:
                    dwFlag = User32.AW_VER_NEGATIVE;
                    break;
                case AnimateMode.RollTopToBottom:
                    dwFlag = User32.AW_VER_POSITIVE;
                    break;
            }
            if (base.Handle == IntPtr.Zero)
                this.CreateWindowOnly(GetExStyle());
            if ((dwFlag & User32.AW_BLEND) != 0)
                this.AnimateWithBlend(true, time);
            else
                User32.AnimateWindow(base.Handle, time, dwFlag);
        }
        /// <summary>
        /// Shows the window with animation effect.
        /// </summary>
        /// <param name="x">x-coordinate of window in screen coordinates</param>
        /// <param name="y">x-coordinate of window in screen coordinates</param>
        /// <param name="mode">Effect to be applied</param>
        /// <param name="time">Time, in milliseconds, for effect playing</param>
        public virtual void ShowAnimate(int x, int y, AnimateMode mode, uint time)
        {
            this._location.X = x;
            this._location.Y = y;
            this.ShowAnimate(mode, time);
        }
        /// <summary>
		/// Hides the window and release it's handle.
		/// </summary>
        public virtual void Hide()
        {
            if (base.Handle == IntPtr.Zero)
                return;

            //Debug.WriteLine("Hiding Window");
            User32.ShowWindow(base.Handle, User32.SW_HIDE);
        }
        /// <summary>
        /// Hides the window with animation effect and release it's handle.
        /// </summary>
        /// <param name="mode">Effect to be applied</param>
        /// <param name="time">Time, in milliseconds, for effect playing</param>
        public virtual void HideAnimate(AnimateMode mode, uint time)
        {
            if (base.Handle == IntPtr.Zero)
                return;
            uint dwFlag = 0;
            switch (mode)
            {
                case AnimateMode.Blend:
                    dwFlag = User32.AW_BLEND;
                    break;
                case AnimateMode.ExpandCollapse:
                    dwFlag = User32.AW_CENTER;
                    break;
                case AnimateMode.SlideLeftToRight:
                    dwFlag = User32.AW_HOR_POSITIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.SlideRightToLeft:
                    dwFlag = User32.AW_HOR_NEGATIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.SlideTopToBottom:
                    dwFlag = User32.AW_VER_POSITIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.SlideBottmToTop:
                    dwFlag = User32.AW_VER_NEGATIVE | User32.AW_SLIDE;
                    break;
                case AnimateMode.RollLeftToRight:
                    dwFlag = User32.AW_HOR_POSITIVE;
                    break;
                case AnimateMode.RollRightToLeft:
                    dwFlag = User32.AW_HOR_NEGATIVE;
                    break;
                case AnimateMode.RollBottmToTop:
                    dwFlag = User32.AW_VER_NEGATIVE;
                    break;
                case AnimateMode.RollTopToBottom:
                    dwFlag = User32.AW_VER_POSITIVE;
                    break;
            }
            dwFlag |= User32.AW_HIDE;
            if ((dwFlag & User32.AW_BLEND) != 0)
                this.AnimateWithBlend(false, time);
            else
                User32.AnimateWindow(base.Handle, time, dwFlag);
            this.Hide();
        }
        /// <summary>
        /// Close the window and destroy it's handle.
        /// </summary>
        public virtual void Close()
        {
            if (this.Handle != IntPtr.Zero)
                this.Hide();
            this.Dispose();
        }

        private void AnimateWithBlend(bool show, uint time)
        {
            byte originalAplha = this._alpha;
            byte p = (byte)(originalAplha / (time / 10));
            if (p == 0) p++;
            if (show)
            {
                this._alpha = 0;
                this.UpdateLayeredWindow();
                User32.ShowWindow(base.Handle, User32.SW_SHOWNOACTIVATE);
            }
            for (byte i = show ? (byte)0 : originalAplha; (show ? i <= originalAplha : i >= (byte)0); i += (byte)(p * (show ? 1 : -1)))
            {
                this._alpha = i;
                this.UpdateLayeredWindow();
                if ((show && i > originalAplha - p) || (!show && i < p))
                    break;
            }
            this._alpha = originalAplha;
            if (show)
                this.UpdateLayeredWindow();
        }

        private void CreateWindowOnly(int exStyle)
        {
            int nX = this._location.X;
            int nY = this._location.Y;

            var monitors = Monitors.GetMonitors();
            bool isInsideMonitor = false;
            foreach (var monitor in monitors)
            {
                int monitorWidth = monitor.MonitorInfo.monitor.right - monitor.MonitorInfo.monitor.left;
                int monitorHeight = monitor.MonitorInfo.monitor.bottom - monitor.MonitorInfo.monitor.top;
                int monitorX = monitor.MonitorInfo.monitor.left;
                int monitorY = monitor.MonitorInfo.monitor.top;

                if (nX > monitorX && nX < monitorX + monitorWidth)
                {
                    if (nY > monitorY && nY < monitorY + monitorHeight)
                    {
                        isInsideMonitor = true;

                        if ((nX + this._size.Width) > monitorX + monitorWidth)
                        {
                            nX = monitorX + monitorWidth - this._size.Width;
                        }
                        if ((nY + this._size.Height) > monitorY + monitorHeight)
                        {
                            nY = monitorY + monitorHeight - this._size.Height;
                        }

                        break;
                    }
                }
            }

            if (!isInsideMonitor)
            {
                Screen screen1 = Screen.FromHandle(base.Handle);
                if ((nX + this._size.Width) > screen1.Bounds.Width)
                {
                    nX = screen1.Bounds.Width - this._size.Width;
                }
                if ((nY + this._size.Height) > screen1.Bounds.Height)
                {
                    nY = screen1.Bounds.Height - this._size.Height;
                }
            }

            this._location = new Point(nX, nY);
            Size size1 = this._size;
            Point point1 = this._location;
            CreateParams params1 = new CreateParams();
            params1.Caption = Name;
            params1.X = nX;
            params1.Y = nY;
            params1.Height = size1.Height;
            params1.Width = size1.Width;
            params1.Parent = IntPtr.Zero;
            uint ui = User32.WS_POPUP;
            params1.Style = (int)ui;
            params1.ExStyle = (int)exStyle;

            try
            {
                this.CreateHandle(params1);
                this.UpdateLayeredWindow();
            }
            catch (InvalidOperationException) { }
        }
        #endregion

        public int GetExStyle()
        {
            int exStyle = User32.WS_EX_LAYERED | User32.WS_EX_TRANSPARENT | User32.WS_EX_NOACTIVATE;

            if (!WindowMode)
                exStyle |= User32.WS_EX_TOOLWINDOW;

            if (AlwaysOnTop)
                exStyle |= User32.WS_EX_TOPMOST;

            return exStyle;
        }

        public int GetExStyleDrag()
        {
            int exStyle = User32.WS_EX_LAYERED | User32.WS_EX_TOPMOST | User32.WS_EX_NOACTIVATE | 0x00020000;
            return exStyle;
        }

        #region == Other messages ==
        private void PerformWmPaint_WmPrintClient(ref Message m, bool isPaintMessage)
        {
            try
            {
                PAINTSTRUCT paintstruct1;
                RECT rect1;
                Rectangle rectangle1;
                paintstruct1 = new PAINTSTRUCT();
                IntPtr ptr1 = isPaintMessage ? User32.BeginPaint(m.HWnd, ref paintstruct1) : m.WParam;
                rect1 = new RECT();
                User32.GetWindowRect(base.Handle, ref rect1);
                rectangle1 = new Rectangle(0, 0, rect1.right - rect1.left, rect1.bottom - rect1.top);
                using (Graphics graphics1 = Graphics.FromHdc(ptr1))
                {
                    Bitmap bitmap1 = new Bitmap(rectangle1.Width, rectangle1.Height);
                    using (Graphics graphics2 = Graphics.FromImage(bitmap1))
                        this.PerformPaint(new PaintEventArgs(graphics2, rectangle1));
                    graphics1.DrawImageUnscaled(bitmap1, 0, 0);
                }
                if (isPaintMessage)
                    User32.EndPaint(m.HWnd, ref paintstruct1);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        private static POINT GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            // NOTE: If you need error handling
            // bool success = GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }

        private bool PerformWmNcHitTest(ref Message m)
        {
            POINT point1 = GetCursorPosition();
            point1.X -= _location.X;
            point1.Y -= _location.Y;
            Rectangle rect = new Rectangle(_location.Y, _location.X, _size.Width, _size.Height);
            if (!rect.Contains(point1.X, point1.Y))
                return false;

            m.Result = (IntPtr)(-1);
            return true;
        }


        protected override void WndProc(ref Message m)
        {

            if (m.Msg == 15) // WM_PAINT
            {
                this.PerformWmPaint_WmPrintClient(ref m, true);
                //return;
            }

            else if (m.Msg == 0x318) // WM_PRINTCLIENT
            {
                this.PerformWmPaint_WmPrintClient(ref m, false);
                return;
            }

            switch (m.Msg)
            {
                case 0x21: // WM_MOUSEACTIVATE
                    {
                        this.PerformWmMouseActivate(ref m);
                        return;
                    }
                //case 0x84: // WM_NCHITTEST
                //    {
                //        Debug.WriteLine("WM_NCHITTEST");
                //        //if (!this.PerformWmNcHitTest(ref m))
                //        //{
                //        //    Debug.WriteLine("not in");
                //        //}
                //    }
                case 0x200: // WM_MOUSEMOVE
                    //Debug.WriteLine("WM_MOUSEMOVE");

                    if (!this.isMouseIn)
                    {
                        this.OnMouseEnter();
                        this.isMouseIn = true;
                    }
                    Point p6 = new Point(m.LParam.ToInt32());
                    this.OnMouseMove(new MouseEventArgs(Control.MouseButtons, 1, p6.X, p6.X, 0));
                    if (this.onMouseMove)
                    {
                        this.PerformWmMouseMove(ref m);
                        this.onMouseMove = false;
                    }
                    break;
                case 0x201: // WM_MOUSEDOWN
                    {
                        Debug.WriteLine("WM_MOUSEDOWN");
                        POINT point1;
                        this.lastMouseDown = new Point(m.LParam.ToInt32());
                        point1 = new POINT();
                        point1.X = this.lastMouseDown.X;
                        point1.Y = this.lastMouseDown.Y;
                        point1 = this.MousePositionToScreen(point1);
                        deltaX = point1.X - this.Location.X;
                        deltaY = point1.Y - this.Location.Y;
                        Debug.WriteLine($"mouse is in {lastMouseDown}");


                        this.OnMouseDown(new MouseEventArgs(Control.MouseButtons, 1, lastMouseDown.X, lastMouseDown.Y, 0));
                        if (this.onMouseDown)
                        {
                            Debug.WriteLine("");
                            this.PerformWmMouseDown(ref m);
                            this.onMouseDown = false;
                        }

                        return;
                    }
                case 0x202: // WM_LBUTTONUP
                    {
                        //Debug.WriteLine("WM_LBUTTONUP");
                        Point p = new Point(m.LParam.ToInt32());
                        this.OnMouseUp(new MouseEventArgs(Control.MouseButtons, 1, p.X, p.Y, 0));
                        if (this.onMouseUp)
                        {
                            this.PerformWmMouseUp(ref m);
                            this.onMouseUp = false;
                        }
                        return;
                    }
                case 0x02A3: // WM_MOUSELEAVE
                    {
                        //Debug.WriteLine("WM_MOUSELEAVE");
                        if (this.isMouseIn)
                        {
                            this.OnMouseLeave();
                            this.isMouseIn = false;
                        }
                        break;
                    }

                    //case 0x001c: // WM_ACTIVATEAPP 
                    //    {
                    //        //Debug.WriteLine("WM_ACTIVATEAPP");

                    //        return;
                    //    }

                    //case 0x002: //WM_Destroy
                    //    {
                    //        //Debug.WriteLine("WM_Destroy");

                    //        return;
                    //    }

                    //case 0x0046:
                    //    {
                    //        //Debug.WriteLine("WM_WINDOWPOSCHANGING");
                    //        return;
                    //    }

                    //case 0x0047:
                    //    {
                    //        //Debug.WriteLine("WM_WINDOWPOSCHANGED");
                    //        return;
                    //    }

                    //case 0x007C:
                    //    {

                    //        //Debug.WriteLine("WM_STYLECHANGING");
                    //        return;
                    //    }

                    //case 0x007D:
                    //    {
                    //        //Debug.WriteLine("WM_STYLECHANGED");
                    //        return;
                    //    }
            }

            //Debug.WriteLine($"{m.Msg}");

            base.WndProc(ref m);
        }
        #endregion

        #region == Mouse ==

        private int deltaX;
        private int deltaY;
        private bool captured;
        private bool isMouseIn;
        private bool onMouseMove;
        private bool onMouseDown;
        private bool onMouseUp;

        private Point lastMouseDown = Point.Empty;

        private POINT MousePositionToClient(POINT point)
        {
            POINT point1;
            point1.X = point.X;
            point1.Y = point.Y;
            User32.ScreenToClient(base.Handle, ref point1);
            return point1;
        }
        private POINT MousePositionToScreen(MSG msg)
        {
            POINT point1;
            point1.X = (short)(((int)msg.lParam) & 0xffff);
            point1.Y = (short)((((int)msg.lParam) & -65536) >> 0x10);
            if ((((msg.message != 0xa2) && (msg.message != 0xa8)) && ((msg.message != 0xa5) && (msg.message != 0xac))) && (((msg.message != 0xa1) && (msg.message != 0xa7)) && ((msg.message != 0xa4) && (msg.message != 0xab))))
            {
                User32.ClientToScreen(msg.hwnd, ref point1);
            }
            return point1;
        }
        private POINT MousePositionToScreen(POINT point)
        {
            POINT point1;
            point1.X = point.X;
            point1.Y = point.Y;
            User32.ClientToScreen(base.Handle, ref point1);
            return point1;
        }
        private POINT MousePositionToScreen(Message msg)
        {
            POINT point1;
            point1.X = (short)(((int)msg.LParam) & 0xffff);
            point1.Y = (short)((((int)msg.LParam) & -65536) >> 0x10);
            if ((((msg.Msg != 0xa2) && (msg.Msg != 0xa8)) && ((msg.Msg != 0xa5) && (msg.Msg != 0xac))) && (((msg.Msg != 0xa1) && (msg.Msg != 0xa7)) && ((msg.Msg != 0xa4) && (msg.Msg != 0xab))))
            {
                User32.ClientToScreen(msg.HWnd, ref point1);
            }
            return point1;
        }

        private void PerformWmMouseDown(ref Message m)
        {
            POINT location = MousePositionToClient(new POINT() { X = Location.X, Y = Location.Y });
            if (new Rectangle(location, Size).Contains(this.lastMouseDown))
            {
                this.captured = true;
                User32.SetCapture(base.Handle);
            }
        }
        private void PerformWmMouseMove(ref Message m)
        {
            Point p = Control.MousePosition;
            POINT point1 = new POINT();
            point1.X = p.X;
            point1.Y = p.Y;
            point1 = this.MousePositionToClient(point1);

            if (new Rectangle(0, 0, Size.Width, Size.Height).Contains(point1.X, point1.Y))
                Cursor.Current = Cursors.Hand;
            else
                Cursor.Current = Cursors.Arrow;

            //Debug.WriteLine($"{deltaX}, {deltaY}");


            if (this.captured)
            {

                //if (this.resizing)
                //{
                //    int w = System.Math.Max(50, (p.X + deltaX) - this.Location.X);
                //    int h = System.Math.Max(50, (p.Y + deltaY) - this.Location.Y);
                //    this.Size = new Size(w, h);
                //}
                //else
                //{
                this.Location = new Point(p.X - deltaX, p.Y - deltaY);
                //}
            }
        }
        private void PerformWmMouseUp(ref Message m)
        {
            //this.resizing = false;
            if (this.captured)
            {
                this.captured = false;
                User32.ReleaseCapture();
            }
        }
        private void PerformWmMouseActivate(ref Message m)
        {
            m.Result = (IntPtr)3;
        }


        protected virtual void OnMouseMove(MouseEventArgs e)
        {
            if (this.MouseMove != null)
            {
                this.MouseMove(this, e);
            }
            this.onMouseMove = true;
        }
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            if (this.MouseDown != null)
            {
                this.MouseDown(this, e);
            }
            this.onMouseDown = true;
        }
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            if (this.MouseUp != null)
            {
                this.MouseUp(this, e);
            }
            this.onMouseUp = true;
        }

        protected virtual void OnMouseEnter()
        {
            if (this.MouseEnter != null)
            {
                this.MouseEnter(this, EventArgs.Empty);
            }
        }
        protected virtual void OnMouseLeave()
        {
            if (this.MouseLeave != null)
            {
                this.MouseLeave(this, EventArgs.Empty);
            }
        }

        #endregion

        #region #  Events  #

        public event PaintEventHandler Paint;
        public event EventHandler SizeChanged;
        public event EventHandler LocationChanged;
        public event EventHandler Move;
        public event EventHandler Resize;
        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseMove;
        public event EventHandler MouseEnter;
        public event EventHandler MouseLeave;

        #endregion

        #region == Event Methods ==

        protected virtual void OnLocationChanged(EventArgs e)
        {
            this.OnMove(EventArgs.Empty);
            if (this.LocationChanged != null)
            {
                this.LocationChanged(this, e);
            }
        }
        protected virtual void OnSizeChanged(EventArgs e)
        {
            this.OnResize(EventArgs.Empty);
            if (this.SizeChanged != null)
            {
                this.SizeChanged(this, e);
            }
        }
        protected virtual void OnMove(EventArgs e)
        {
            if (this.Move != null)
            {
                this.Move(this, e);
            }
        }
        protected virtual void OnResize(EventArgs e)
        {
            if (this.Resize != null)
            {
                this.Resize(this, e);
            }
        }

        #endregion

        #region == Size and Location ==
        protected virtual void SetBoundsCore(int x, int y, int width, int height)
        {
            if (((this.X != x) || (this.Y != y)) || ((this.Width != width) || (this.Height != height)))
            {
                if (base.Handle != IntPtr.Zero)
                {
                    int num1 = 20;
                    if ((this.X == x) && (this.Y == y))
                    {
                        num1 |= 2;
                    }
                    if ((this.Width == width) && (this.Height == height))
                    {
                        num1 |= 1;
                    }
                    User32.SetWindowPos(base.Handle, IntPtr.Zero, x, y, width, height, (uint)num1);
                }
                else
                {
                    this.Location = new Point(x, y);
                    this.Size = new Size(width, height);
                }
            }
        }
        #endregion

        #endregion

        #region #  Properties  #
        /// <summary>
        /// Get or set position of top-left corner of floating native window in screen coordinates
        /// </summary>
        public virtual Point Location
        {
            get { return this._location; }
            set
            {
                if (base.Handle != IntPtr.Zero)
                {
                    this.SetBoundsCore(value.X, value.Y, this._size.Width, this._size.Height);
                    RECT rect = new RECT();
                    User32.GetWindowRect(base.Handle, ref rect);
                    this._location = new Point(rect.left, rect.top);
                    //this.UpdateLayeredWindow();
                }
                else
                {
                    this._location = value;
                }
            }
        }
        /// <summary>
        /// Get or set size of client area of floating native window
        /// </summary>
        public virtual Size Size
        {
            get { return this._size; }
            set
            {
                if (base.Handle != IntPtr.Zero)
                {
                    this.SetBoundsCore(this._location.X, this._location.Y, value.Width, value.Height);
                    RECT rect = new RECT();
                    User32.GetWindowRect(base.Handle, ref rect);
                    this._size = new Size(rect.right - rect.left, rect.bottom - rect.top);
                    //this.UpdateLayeredWindow();
                }
                else
                {
                    this._size = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the height of the floating native window
        /// </summary>
		public int Height
        {
            get { return this._size.Height; }
            set
            {
                this._size = new Size(this._size.Width, value);
            }
        }
        /// <summary>
        /// Gets or sets the width of the floating native window
        /// </summary>
        public int Width
        {
            get { return this._size.Width; }
            set
            {
                this._size = new Size(value, this._size.Height);
            }
        }
        /// <summary>
        /// Get or set x-coordinate of top-left corner of floating native window in screen coordinates
        /// </summary>
		public int X
        {
            get { return this._location.X; }
            set
            {
                this.Location = new Point(value, this.Location.Y);
            }
        }
        /// <summary>
        /// Get or set y-coordinate of top-left corner of floating native window in screen coordinates
        /// </summary>
        public int Y
        {
            get { return this._location.Y; }
            set
            {
                this.Location = new Point(this.Location.X, value);
            }
        }
        /// <summary>
        /// Get rectangle represented client area of floating native window in client coordinates(top-left corner always has coord. 0,0)
        /// </summary>
		public Rectangle Bound
        {
            get
            {
                return new Rectangle(new Point(0, 0), this._size);
            }
        }
        /// <summary>
        /// Get or set full opacity(255) or full transparency(0) or any intermediate state for floating native window transparency
        /// </summary>
		public byte Alpha
        {
            get { return this._alpha; }
            set
            {
                if (this._alpha == value) return;
                this._alpha = value;
                this.UpdateLayeredWindow();
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                this.DestroyHandle();
                this._disposed = true;
            }
        }
        #endregion
    }

    #region #  Win32  #
    internal struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public int fErase;
        public Rectangle rcPaint;
        public int fRestore;
        public int fIncUpdate;
        public int Reserved1;
        public int Reserved2;
        public int Reserved3;
        public int Reserved4;
        public int Reserved5;
        public int Reserved6;
        public int Reserved7;
        public int Reserved8;
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
    internal class User32
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
        // Methods
        private User32()
        {
        }
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool AnimateWindow(IntPtr hWnd, uint dwTime, uint dwFlags);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr BeginPaint(IntPtr hWnd, ref PAINTSTRUCT ps);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ClientToScreen(IntPtr hWnd, ref POINT pt);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool DispatchMessage(ref MSG msg);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool DrawFocusRect(IntPtr hWnd, ref RECT rect);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetFocus();
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern ushort GetKeyState(int virtKey);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMessage(ref MSG msg, int hWnd, uint wFilterMin, uint wFilterMax);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool GetClientRect(IntPtr hWnd, [In, Out] ref RECT rect);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetWindow(IntPtr hWnd, int cmd);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool HideCaret(IntPtr hWnd);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool InvalidateRect(IntPtr hWnd, ref RECT rect, bool erase);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr LoadCursor(IntPtr hInstance, uint cursor);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In, Out] ref RECT rect, int cPoints);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool PeekMessage(ref MSG msg, int hWnd, uint wFilterMin, uint wFilterMax, uint wFlag);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool PostMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ReleaseCapture();
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ScreenToClient(IntPtr hWnd, ref POINT pt);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern uint SendMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SetCursor(IntPtr hCursor);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint newLong);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int X, int Y, int Width, int Height, uint flags);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool redraw);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ShowCaret(IntPtr hWnd);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool SetCapture(IntPtr hWnd);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int ShowWindow(IntPtr hWnd, short cmdShow);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref int bRetValue, uint fWinINI);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool TrackMouseEvent(ref TRACKMOUSEEVENTS tme);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool TranslateMessage(ref MSG msg);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool UpdateWindow(IntPtr hwnd);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool WaitMessage();
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool AdjustWindowRectEx(ref RECT lpRect, int dwStyle, bool bMenu, int dwExStyle);
    }

    internal class Gdi32
    {
        // Methods
        private Gdi32()
        {
        }
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern int CombineRgn(IntPtr dest, IntPtr src1, IntPtr src2, int flags);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateBrushIndirect(ref LOGBRUSH brush);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateRectRgnIndirect(ref RECT rect);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern bool DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetClipBox(IntPtr hDC, ref RECT rectBox);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern bool PatBlt(IntPtr hDC, int x, int y, int width, int height, uint flags);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern int SelectClipRgn(IntPtr hDC, IntPtr hRgn);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct LOGBRUSH
    {
        public uint lbStyle;
        public uint lbColor;
        public uint lbHatch;
    }

    #endregion
}
