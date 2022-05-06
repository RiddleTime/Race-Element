using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.Controls.HUD.Overlay.OverlayPressureTrace.TyrePressures;

namespace ACCManager.Controls.HUD.Overlay.OverlayPressureTrace
{
    internal class TyrePressureGraph
    {
        private int X, Y;
        private int Width, Height;
        private LinkedList<float> TirePressures;

        public static TyrePressureRange PressureRange { get; set; } = TyrePressures.DRY_DHE2020;
        public static float Padding = 0.4f;

        public TyrePressureGraph(int x, int y, int width, int height, LinkedList<float> tirePressures)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.TirePressures = tirePressures;
        }

        private int getRelativeNodeY(float value)
        {
            double range = (PressureRange.OptimalMaximum + Padding) - (PressureRange.OptimalMinimum - Padding);
            double percentage = (PressureRange.OptimalMaximum + Padding - value) / range;
            if (percentage > 0.95)
                percentage = 0.95;

            if (percentage < 0.05)
                percentage = 0.05;

            return (int)(percentage * Height);
        }

        public void Draw(Graphics g)
        {
            //this.SetMinAndMax();
            Rectangle graphRect = new Rectangle(X, Y, Width, Height);
            // draw background
            g.FillRectangle(new SolidBrush(Color.FromArgb(196, Color.Black)), graphRect);



            if (TirePressures.Count > 0)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                DrawData(g, TirePressures);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

                g.DrawRectangle(new Pen(Color.FromArgb(196, Color.Black)), graphRect);

            }
        }

        private void DrawData(Graphics g, LinkedList<float> Data)
        {
            if (Data.Count > 0)
            {
                List<Point> points = new List<Point>();
                lock (Data)
                    for (int i = 0; i < Data.Count - 1; i++)
                    {
                        int x = X + Width - i * (Width / Data.Count);
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

                            if (points.Count > 1)
                            {
                                GraphicsPath path = new GraphicsPath() { FillMode = FillMode.Winding };
                                path.AddLines(points.ToArray());
                                g.DrawPath(new Pen(GetLineColor(Data.ElementAt(i)), 2.5f), path);
                                points.Clear();
                                points.Add(new Point(x, y));
                            }

                        }

                    }
            }
        }

        private Brush GetLineColor(float pressure)
        {
            if (pressure < PressureRange.OptimalMinimum)
            {
                return Brushes.Blue;
            }
            if (pressure > PressureRange.OptimalMaximum)
            {
                return Brushes.Red;
            }

            return Brushes.Green;
        }
    }
}
