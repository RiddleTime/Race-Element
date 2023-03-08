using RaceElement.Util.SystemExtensions;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace RaceElement.HUD.Overlay.OverlayUtil
{
    public class InfoTable
    {
        private const float _shadowDistance = 0.75f;

        private float _yMono;
        public bool _headerWidthSet;
        private float _maxHeaderWidth;
        private int[] _columnWidths;
        private List<TableRow> _rows = new List<TableRow>();

        public int X = 0;
        public int Y = 0;

        public Font Font { get; }
        public int FontHeight { get; private set; }

        public bool DrawBackground { get; set; } = true;
        public bool DrawValueBackground { get; set; } = true;
        public bool DrawRowLines { get; set; } = true;

        private CachedBitmap _cachedBackground;
        private CachedBitmap _cachedLine;

        private int previousRowCount = 0;

        public InfoTable(float fontSize, int[] columnWidths)
        {
            fontSize.ClipMin(9);
            _columnWidths = columnWidths;
            Font = FontUtil.FontSegoeMono(fontSize);
            FontHeight = Font.Height;
            _yMono = Font.Height / 8;
        }

        public void Draw(Graphics g)
        {
            if (!_headerWidthSet) UpdateMaxheaderWidth(g);

            if (DrawBackground && _rows.Count > 0)
            {
                if (previousRowCount != _rows.Count)
                {
                    _cachedBackground = new CachedBitmap((int)Math.Ceiling(GetTotalWidth()), _rows.Count * FontHeight + (int)_yMono, bg =>
                    {
                        bg.FillRoundedRectangle(new SolidBrush(Color.FromArgb(158, Color.Black)), new Rectangle(0, 0, (int)GetTotalWidth(), _rows.Count * this.Font.Height + (int)_yMono), 4);
                    });
                    previousRowCount = _rows.Count;
                }

                _cachedBackground.Draw(g, new Point(X, Y));
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
                {
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(80, Color.Black)), new Rectangle((int)_maxHeaderWidth + 5, Y, valueWidth - 4, _rows.Count * this.Font.Height + (int)_yMono + 1), 4);
                }
                while (counter < length)
                {
                    TableRow row = _rows[counter];
                    float rowY = Y + counter * Font.Height;

                    if (row.HeaderBackground != Color.Transparent)
                        g.FillRoundedRectangle(new SolidBrush(row.HeaderBackground), new Rectangle(X, (int)rowY, (int)_maxHeaderWidth + 5, Font.Height), 4);

                    if (DrawRowLines && counter > 0)
                    {
                        if (_cachedLine == null)
                        {
                            _cachedLine = new CachedBitmap(totalWidth - 2, 1, lg =>
                            {
                                lg.DrawLine(new Pen(Color.FromArgb(42, Color.White)), new Point(1, 0), new Point(totalWidth - 1, 0));
                            });
                        }

                        _cachedLine.Draw(g, new Point(X + 1, (int)rowY));
                    }

                    g.DrawStringWithShadow(row.Header, this.Font, Color.White, new PointF(X, rowY + _yMono), _shadowDistance);

                    for (int i = 0; i < row.Columns.Length; i++)
                    {
                        float columnX = GetColumnX(i);
                        if (i == 0) columnX += Font.Size;
                        g.DrawStringWithShadow(row.Columns[i], this.Font, row.ColumnColors[i], new PointF(columnX, rowY + _yMono), _shadowDistance);
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
            this.AddRow(new TableRow() { Header = header, Columns = columns, ColumnColors = columnColors });
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
                        if ((titleWidth = g.MeasureString(line.Header, Font)).Width > _maxHeaderWidth)
                            _maxHeaderWidth = titleWidth.Width;

                        counter++;
                        length = _rows.Count;
                    }
                }
                if (_maxHeaderWidth == 0)
                    return;

                _maxHeaderWidth += Font.Size;
                _headerWidthSet = true;
            }
        }
    }
}
