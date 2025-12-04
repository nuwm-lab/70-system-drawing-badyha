using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LabWork
{
    // Програма малює графік y = tg(0.5x) / (x^3 + 7.5) для 0.1 ≤ x ≤ 1.2 з кроком 0.1.
    // Графік автоматично перерисовується при зміні розміру вікна.
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new GraphForm());
        }
    }

    class GraphForm : Form
    {
        private readonly float margin = 40f;

        public GraphForm()
        {
            Text = "Графік функції y = tg(0.5x) / (x^3 + 7.5)";
            Width = 800;
            Height = 600;
            DoubleBuffered = true;
            BackColor = Color.White;
            Resize += (s, e) => Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Compute data points
            double xMin = 0.1, xMax = 1.2;
            double dx = 0.1;
            var pts = new List<PointF>();
            double yMin = double.PositiveInfinity, yMax = double.NegativeInfinity;
            for (double x = xMin; x <= xMax + 1e-9; x += dx)
            {
                double y = Math.Tan(0.5 * x) / (Math.Pow(x, 3) + 7.5);
                if (double.IsFinite(y))
                {
                    yMin = Math.Min(yMin, y);
                    yMax = Math.Max(yMax, y);
                }
            }

            if (yMin == double.PositiveInfinity || yMax == double.NegativeInfinity)
            {
                // nothing to draw
                return;
            }

            // Add small padding to Y range
            double yPadding = (yMax - yMin) * 0.1;
            if (yPadding == 0) yPadding = 1e-3;
            yMin -= yPadding;
            yMax += yPadding;

            // Drawable area
            float left = margin;
            float top = margin;
            float right = ClientSize.Width - margin;
            float bottom = ClientSize.Height - margin;
            float w = Math.Max(10f, right - left);
            float h = Math.Max(10f, bottom - top);

            // Draw axes
            using (var axisPen = new Pen(Color.Black, 1))
            {
                // X axis: at y=0 if in range, else at bottom
                float y0;
                if (yMin <= 0 && 0 <= yMax)
                {
                    y0 = (float)(top + (yMax - 0) / (yMax - yMin) * h);
                }
                else
                {
                    // place x-axis at bottom
                    y0 = bottom;
                }
                g.DrawLine(axisPen, left, y0, left + w, y0);

                // Y axis: at x=0 if in range, else at left
                float x0;
                if (xMin <= 0 && 0 <= xMax)
                {
                    x0 = (float)(left + (0 - xMin) / (xMax - xMin) * w);
                }
                else
                {
                    x0 = left;
                }
                g.DrawLine(axisPen, x0, top, x0, top + h);
            }

            // Draw ticks and labels for X
            using (var font = new Font("Segoe UI", 8))
            using (var brush = new SolidBrush(Color.Black))
            {
                for (double x = xMin; x <= xMax + 1e-9; x += dx)
                {
                    float px = (float)(left + (x - xMin) / (xMax - xMin) * w);
                    float pyTickTop = top + h;
                    g.DrawLine(Pens.Gray, px, pyTickTop, px, pyTickTop - 5);
                    string xs = x.ToString("0.0");
                    var sz = g.MeasureString(xs, font);
                    g.DrawString(xs, font, brush, px - sz.Width / 2, pyTickTop + 2);
                }

                // Y ticks (choose 5 steps)
                int ySteps = 5;
                for (int i = 0; i <= ySteps; i++)
                {
                    double y = yMin + (yMax - yMin) * i / ySteps;
                    float py = (float)(top + (yMax - y) / (yMax - yMin) * h);
                    g.DrawLine(Pens.Gray, left, py, left + 5, py);
                    string ys = y.ToString("0.####");
                    var sz = g.MeasureString(ys, font);
                    g.DrawString(ys, font, brush, left - sz.Width - 6, py - sz.Height / 2);
                }
            }

            // Compute and draw graph polyline
            using (var pen = new Pen(Color.Blue, 2))
            {
                var points = new List<PointF>();
                for (double x = xMin; x <= xMax + 1e-9; x += dx)
                {
                    double y = Math.Tan(0.5 * x) / (Math.Pow(x, 3) + 7.5);
                    if (!double.IsFinite(y)) continue;
                    float px = (float)(left + (x - xMin) / (xMax - xMin) * w);
                    float py = (float)(top + (yMax - y) / (yMax - yMin) * h);
                    points.Add(new PointF(px, py));
                }

                if (points.Count >= 2)
                {
                    g.DrawLines(pen, points.ToArray());
                }
            }
        }
    }
}
