using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Overlay.OverlayUtil.InfoPanel
{
    internal class PanelTextGrid : AbstractPanelGrid
    {
        public PanelTextGrid(int rows, int columns) : base(rows, columns)
        {
            Grid = new PanelText[rows][];
            for (int row = 0; row < rows; row++)
            {
                Grid[row] = new PanelText[columns];
                for (int column = 0; column < columns; column++)
                    Grid[row][column] = null;
            }
        }

        public void Draw(Graphics g)
        {
            for (int row = 0; row < Rows; row++)
                for (int column = 0; column < Columns; column++)
                    Grid[row][column]?.Draw(g);
        }
    }
}
