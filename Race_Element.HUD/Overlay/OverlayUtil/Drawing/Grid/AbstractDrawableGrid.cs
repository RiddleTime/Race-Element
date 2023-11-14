using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Overlay.OverlayUtil.Drawing
{
    public abstract class AbstractDrawableGrid<T> : IScalableDrawing where T : IScalableDrawing
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public T[][] Grid { get; private set; }

        protected AbstractDrawableGrid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            InitGrid(Rows, Columns);
        }

        private void InitGrid(int rows, int columns)
        {
            Grid = new T[rows][];
            for (int row = 0; row < rows; row++)
            {
                Grid[row] = new T[columns];
                for (int column = 0; column < columns; column++)
                    Grid[row][column] = default;
            }
        }

        public abstract void Draw(Graphics g, float scaling);

        public void Dispose()
        {
            for (int row = 0; row < Rows; row++)
                for (int column = 0; column < Columns; column++)
                    Grid[row][column]?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
