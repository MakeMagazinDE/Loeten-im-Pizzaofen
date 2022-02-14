using System;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

using System.Drawing;

namespace HPGL2GCODE
{
    internal partial class NCView : Form
    {
        Polygon polygon;
        Point startPoint = Point.Empty;
        Point offset = Point.Empty;
        float factor;

        public NCView()
        {
            InitializeComponent();
            polygon = new Polygon();
            factor = 0;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            PointF pt = new PointF();
            e.Graphics.Clear(pictureBox.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (factor == 0)
            {
                factor = Form1.factor;
                offset = new Point(1, 1);
            }

            if (Form1.allPolygons.Length >= 3)
            {
                polygon.Points.Clear();
                for (int p = 0; p < Form1.allPolygons.Length; p++)
                {
                    if (Form1.allPolygons[p].X != -1)
                    {
                        pt.X = Form1.allPolygons[p].X * factor + offset.X;
                        pt.Y = pictureBox.Bottom-Form1.allPolygons[p].Y * factor + offset.Y;
                        polygon.Points.Add(pt);
                    }
                    else
                    {
                        if (polygon.Points.Count > 0)
                        {
                            e.Graphics.DrawPolygon(Pens.White, polygon.Points.ToArray());
                            polygon.Points.Clear();
                        }
                    }
                }
            }
        }
        private void plusButton_Click(object sender, EventArgs e)
        {
            factor += 2;
            pictureBox.Refresh();
        }

        private void centerButton_Click(object sender, EventArgs e)
        {
            offset = new Point(1, 1);
            factor = Form1.factor;
            pictureBox.Refresh();
        }

        private void minusButton_Click(object sender, EventArgs e)
        {
            if (factor > 2)
                factor -= 2;
            pictureBox.Refresh();
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                startPoint = new Point(e.X, e.Y);
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (startPoint != Point.Empty)
            {
                offset.X += e.X - startPoint.X;
                startPoint.X = e.X;
                offset.Y += e.Y - startPoint.Y;
                startPoint.Y = e.Y;
                pictureBox.Refresh();
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            startPoint = Point.Empty;
        }
    }
}
