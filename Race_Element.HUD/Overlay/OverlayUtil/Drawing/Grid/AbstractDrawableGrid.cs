using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Overlay.OverlayUtil.Drawing
{
    public abstract class AbstractDrawableGrid<T> : IDrawable where T : IDrawable
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public T[][] Grid;

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

        public abstract void Draw(Graphics g);

        public void Dispose()
        {
            for (int row = 0; row < Rows; row++)
                for (int column = 0; column < Columns; column++)
                    Grid[row][column]?.Dispose();
        }
    }
}
