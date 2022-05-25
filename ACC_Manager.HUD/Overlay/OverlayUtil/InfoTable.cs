using ACC_Manager.Util.NumberExtensions;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.Overlay.OverlayUtil
{
    public class InfoTable
    {
        private const float _shadowDistance = 0.75f;
        private readonly Brush _shadowBrush = new SolidBrush(Color.FromArgb(60, Color.Black));

        private Font _font;
        private float _yMono;
        private bool _headerWidthSet;
        private float _maxHeaderWidth;
        private int[] _columnWidths;
        private List<TableRow> _rows = new List<TableRow>();

        public int X = 0;
        public int Y = 0;
        private int _fontHeight;
        public int FontHeight { get { return _fontHeight; } private set { _fontHeight = value; } }
        public bool DrawBackground { get; set; } = true;

        public InfoTable(float fontSize, int[] columnWidths)
        {
            fontSize.ClipMin(9);
            _columnWidths = columnWidths;
            _font = FontUtil.FontUnispace(fontSize);
            _fontHeight = _font.Height;
            _yMono = _font.Height / 6;
        }

        public void Draw(Graphics g)
        {
            if (!_headerWidthSet) UpdateMaxheaderWidth(g);

            if (DrawBackground)
            {
                SmoothingMode previous = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                float totalWidth = this._maxHeaderWidth;
                for (int i = 0; i < _columnWidths.Length; i++)
                    totalWidth += this._columnWidths[i];

                g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(140, 0, 0, 0)), new Rectangle(X, Y, (int)totalWidth, _rows.Count * this._font.Height), 4);
                g.SmoothingMode = previous;
            }

            lock (_rows)
            {
                int length = _rows.Count;
                int counter = 0;
                while (counter < length)
                {
                    TableRow row = _rows[counter];

                    float rowY = Y + counter * _font.Height;

                    g.DrawString(row.Header, this._font, this._shadowBrush, X + _shadowDistance, rowY + _shadowDistance + _yMono);
                    g.DrawString(row.Header, this._font, Brushes.White, X, rowY + _yMono);

                    for (int i = 0; i < row.Columns.Length; i++)
                    {
                        float columnX = GetColumnX(i);
                        g.DrawString(row.Columns[i], this._font, this._shadowBrush, columnX + _shadowDistance, rowY + _shadowDistance + _yMono);
                        g.DrawString(row.Columns[i], this._font, new SolidBrush(row.ColumnColors[i]), columnX, rowY + _yMono);
                    }

                    counter++;
                }

                _rows.Clear();
            }
        }

        private float GetColumnX(int columnIndex)
        {
            float x = this.X;
            x += this._maxHeaderWidth;

            for (int i = 0; i < columnIndex; i++)
                x += this._columnWidths[i];

            return x;
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
            public string[] Columns { get; set; }
            public Color[] ColumnColors { get; set; }
        }

        private void UpdateMaxheaderWidth(Graphics g)
        {
            lock (_rows)
            {
                int length = _rows.Count;
                int counter = 0;
                while (counter < length)
                {
                    TableRow line = _rows[counter];
                    SizeF titleWidth;
                    if ((titleWidth = g.MeasureString(line.Header, _font)).Width > _maxHeaderWidth)
                        _maxHeaderWidth = titleWidth.Width;

                    counter++;
                    length = _rows.Count;
                }
                _maxHeaderWidth += _font.Size;
                _headerWidthSet = true;
            }
        }
    }
}
