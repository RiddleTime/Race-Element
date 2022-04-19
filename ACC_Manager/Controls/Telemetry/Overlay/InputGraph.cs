using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls
{
    internal class InputGraph
    {
        private int X, Y;
        private int Width, Height;
        private LinkedList<int> ThrottleData;
        private LinkedList<int> BrakeData;

        public InputGraph(int x, int y, int width, int height, LinkedList<int> throttleData, LinkedList<int> brakeData)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.ThrottleData = throttleData;
            this.BrakeData = brakeData;
        }

        private int getRelativeNodeY(int value)
        {
            double range = 100 - 0;
            double percentage = 1d - (value - 0) / range;
            return (int)(percentage * (Height - Height / 7.5))
                    + Height / 15;
        }

        public void Draw(Graphics g)
        {
            Rectangle graphRect = new Rectangle(X, Y, Width, Height);
            // draw background
            g.FillRectangle(new SolidBrush(Color.FromArgb(255, Color.DarkGray)), graphRect);
           
            DrawData(g, BrakeData, Brushes.Red);

            DrawData(g, ThrottleData, Brushes.Green);

            g.DrawRectangle(new Pen(Brushes.White), graphRect);
        }

        private void DrawData(Graphics g, LinkedList<int> Data, Brush color)
        {
            if (Data.Count > 0)
            {
                List<Point> points = new List<Point>();
                for (int i = Data.Count - 1; i >= 0; i--)
                {
                    int x = X + i * (Width / Data.Count);
                    lock (Data)
                    {
                        int y = Y + getRelativeNodeY(Data.ElementAt(i));
                        if (y > Y + Height)
                        {
                            y = Y + Height;
                        }
                        if (x < X)
                            break;
                        points.Add(new Point(x, y));
                    }

                }

                // draw the graph
                for (int i = 0; i < points.Count; i++)
                {
                    if (points.Count - 1 != i)
                    {
                        Point from = points[i];
                        Point to = points[i + 1];
                        g.DrawLine(new Pen(color, 1.3f), from, to);
                    }
                }
            }
        }
    }
}
