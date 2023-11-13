using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Overlay.OverlayUtil.InfoPanel
{
    internal abstract class AbstractPanelGrid : IDrawable
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public AbstractPanel[][] Grid;

        protected AbstractPanelGrid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
        }

        public void Draw(Graphics g)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            for (int row = 0; row < Rows; row++)
                for (int column = 0; column < Columns; column++)
                    Grid[row][column]?.Dispose();
        }
    }
}
