using ACC_Manager.Util.NumberExtensions;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.Overlay.OverlayUtil
{
    public class InfoTable
    {
        private const float _shadowDistance = 0.75f;
        private readonly Brush _shadowBrush = new SolidBrush(Color.FromArgb(60, Color.Black));


        private float _yMono;
        public bool _headerWidthSet;
        private float _maxHeaderWidth;
        private int[] _columnWidths;
        private List<TableRow> _rows = new List<TableRow>();

        public int X = 0;
        public int Y = 0;

        private Font _font;
        public Font Font { get { return _font; } }
        private int _fontHeight;
        public int FontHeight { get { return _fontHeight; } private set { _fontHeight = value; } }

        public bool DrawBackground { get; set; } = true;
        public bool DrawValueBackground { get; set; } = true;
        public bool DrawRowLines { get; set; } = true;

        public InfoTable(float fontSize, int[] columnWidths)
        {
            fontSize.ClipMin(9);
            _columnWidths = columnWidths;
            _font = FontUtil.FontUnispace(fontSize);
            _fontHeight = _font.Height;
            _yMono = _font.Height / 8;
        }

        public void Draw(Graphics g)
        {
            if (!_headerWidthSet) UpdateMaxheaderWidth(g);

            if (DrawBackground && _rows.Count > 0)
            {
                SmoothingMode previous = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(140, Color.Black)), new Rectangle(X, Y, (int)GetTotalWidth(), _rows.Count * this._font.Height + (int)_yMono), 4);
                g.SmoothingMode = previous;
            }

            TextRenderingHint previousHint = g.TextRenderingHint;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.TextContrast = 1;
            lock (_rows)
            {
                int length = _rows.Count;
                int counter = 0;
                int totalWidth = (int)GetTotalWidth();
                int valueWidth = (int)(totalWidth - this._maxHeaderWidth);

                if (DrawValueBackground)
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(25, Color.White)), new Rectangle((int)_maxHeaderWidth + 5, Y, valueWidth - 4, _rows.Count * this._font.Height + (int)_yMono + 1), 4);

                while (counter < length)
                {
                    TableRow row = _rows[counter];
                    float rowY = Y + counter * _font.Height;

                    if (row.HeaderBackground != Color.Transparent)
                        g.FillRoundedRectangle(new SolidBrush(row.HeaderBackground), new Rectangle(X, (int)rowY, (int)_maxHeaderWidth + 5, _font.Height), 4);

                    if (DrawRowLines && counter > 0)
                        g.DrawLine(new Pen(Color.FromArgb(45, Color.White)), new Point(X + 1, (int)rowY), new Point(totalWidth - 1, (int)rowY));

                    g.DrawStringWithShadow(row.Header, this._font, Color.White, new PointF(X, rowY + _yMono), _shadowDistance);

                    for (int i = 0; i < row.Columns.Length; i++)
                    {
                        float columnX = GetColumnX(i);
                        if (i == 0) columnX += Font.Size;
                        g.DrawStringWithShadow(row.Columns[i], this._font, row.ColumnColors[i], new PointF(columnX, rowY + _yMono), _shadowDistance);
                    }
                    counter++;
                }

                _rows.Clear();
            }

            g.TextRenderingHint = previousHint;
        }

        private float GetColumnX(int columnIndex)
        {
            float x = this.X;
            x += this._maxHeaderWidth;

            for (int i = 0; i < columnIndex; i++)
                x += this._columnWidths[i];

            return x;
        }

        private float GetTotalWidth()
        {
            float totalWidth = this._maxHeaderWidth;
            for (int i = 0; i < _columnWidths.Length; i++)
                totalWidth += this._columnWidths[i];

            return totalWidth;
        }

        public void AddRow(string header, string[] columns, Color[] columnColors = null)
        {
            this.AddRow(new TableRow() { Header = header, Columns = columns, ColumnColors = columnColors }); ;
        }

        public void AddRow(TableRow row)
        {
            if (row == null) return;
            if (row.Header == null) row.Header = string.Empty;
            if (row.Columns == null) row.Columns = new string[this._columnWidths.Length];
            if (row.ColumnColors == null)
            {
                row.ColumnColors = new Color[this._columnWidths.Length];
                for (int i = 0; i < this._columnWidths.Length; i++)
                    row.ColumnColors[i] = Color.White;
            }

            if (row.ColumnColors.Length != this._columnWidths.Length)
            {
                Color[] givenColors = row.ColumnColors;
                row.ColumnColors = new Color[this._columnWidths.Length];
                for (int i = 0; i < this._columnWidths.Length; i++)
                    if (i >= givenColors.Length)
                        row.ColumnColors[i] = Color.White;
                    else
                        row.ColumnColors[i] = givenColors[i];
            }

            if (row.Columns.Length > this._columnWidths.Length)
                row.Columns = row.Columns.Take(this._columnWidths.Length).ToArray();

            this._rows.Add(row);
        }

        public class TableRow
        {
            public string Header { get; set; }
            public Color HeaderBackground { get; set; } = Color.Transparent;
            public string[] Columns { get; set; }
            public Color[] ColumnColors { get; set; }
        }

        private void UpdateMaxheaderWidth(Graphics g)
        {
            lock (_rows)
            {
                _maxHeaderWidth = 0;

                int length = _rows.Count;
                int counter = 0;
                while (counter < length)
                {
                    TableRow line = _rows[counter];
                    if (line != null)
                    {
                        SizeF titleWidth;
                        if ((titleWidth = g.MeasureString(line.Header, _font)).Width > _maxHeaderWidth)
                            _maxHeaderWidth = titleWidth.Width;

                        counter++;
                        length = _rows.Count;
                    }
                }
                _maxHeaderWidth += _font.Size;
                _headerWidthSet = true;
            }
        }
    }
}
